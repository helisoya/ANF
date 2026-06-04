using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the settings menu located within the pause menu
    /// </summary>
    public class SettingsMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Tabs")]
        [SerializeField] private Transform tabsRoot;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private SettingsTabUI tabPrefab;
        [SerializeField, SerializeReference, SubclassSelector(AllowNull = false)] private SettingsHandlerUI[] handlers;
        private Dictionary<string, SettingsTabUI> tabs;

        [Header("Components")]
        [Tooltip("The UI/Navigate action used by controller and keyboard to use menus")]
        [SerializeField] InputActionReference navigateAction;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private SettingsEntryUIToggle prefabToggleToggle;
        [SerializeField] private SettingsEntryUISlider prefabToggleSlider;
        [SerializeField] private SettingsEntryUIDropdown prefabToggleDropdown;
        [SerializeField] private SettingsEntryUIColorPicker prefabToggleColorPicker;
        [SerializeField] private SettingsEntryUIButton prefabToggleButton;

        private AudioManager audioManager;
        private List<Selectable> objects;
        private GameObject lastSelectedObject;
        private bool selectFirstObject;

        private bool movingWithInput;

        public override void OnInitialize()
        {
            objects = new List<Selectable>();
            tabs = new Dictionary<string, SettingsTabUI>();
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
            movingWithInput = false;
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {
            if (selectFirstObject)
            {
                selectFirstObject = false;
                if (objects.Count != 0)
                    EventSystem.current.SetSelectedGameObject(objects[0].gameObject);
            }

            if (navigateAction.action.triggered)
                movingWithInput = !movingWithInput;

            if (!movingWithInput)
                return;

            if (EventSystem.current.currentSelectedGameObject && lastSelectedObject != EventSystem.current.currentSelectedGameObject)
            {
                lastSelectedObject = EventSystem.current.currentSelectedGameObject;
                Selectable selectable = lastSelectedObject.GetComponent<Selectable>();

                if (selectable != null)
                {
                    int index = objects.IndexOf(selectable);

                    if (index != -1)
                    {
                        scrollbar.value = 1.0f - (float)index / (objects.Count - 1);
                    }
                }
            }
        }


        public override void OnEnabled()
        {
            objects.Clear();

            selectFirstObject = true;
            tabs.Clear();

            foreach (Transform child in tabsRoot)
                Destroy(child.gameObject);

            for(int i = 0;i < handlers.Length;i++)
            {
                handlers[i].Initialize(this, manager);

                foreach (SettingsTabUI tab in tabs.Values)
                    tab.Rebuild();

                LayoutRebuilder.ForceRebuildLayoutImmediate(tabsRoot.GetComponent<RectTransform>());
            }

            objects.Sort((Selectable s1, Selectable s2) => { return s2.transform.position.y.CompareTo(s1.transform.position.y); });

            RegenerateObjectsNavigation();

            scrollbar.value = 1;

            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);
        }


        public override void OnDisabled()
        {
            if (colorPicker.IsOpen)
                colorPicker.Close();

            EventSystem.current.SetSelectedGameObject(null);
            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);

            string globalDataSaveFile = FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder + "global.json";
            SaveUtils.SaveGlobalData(PersistentDataManager.instance.GetGlobalData(), globalDataSaveFile);
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                if (colorPicker.IsOpen)
                    colorPicker.Close();
                else
                    SetEnabled(false);
            }
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Pause").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Pause").performed -= OnPauseInput;
        }

        public override void OnChangeScene()
        {
            OnUnRegisterInputs();
        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }

        private void RegenerateObjectsNavigation()
        {
            if (objects.Count > 1)
            {
                for(int i = 0; i < objects.Count-1; i++)
                {
                    Navigation navigationTop = new Navigation()
                    {
                        mode = Navigation.Mode.Explicit,
                        wrapAround = true,
                        selectOnDown = objects[i+1],
                        selectOnUp = objects[i].navigation.selectOnUp
                    };

                    Navigation navigationDown = new Navigation()
                    {
                        mode = Navigation.Mode.Explicit,
                        wrapAround = true,
                        selectOnDown = null,
                        selectOnUp = objects[i]
                    };

                    objects[i].navigation = navigationTop;
                    objects[i + 1].navigation = navigationDown;
                }
            }
        }

        /// <summary>
        /// Redraws the localized elements
        /// </summary>
        public void RedrawLocalizedElements()
        {
            foreach (SettingsHandlerUI handler in handlers)
                handler.RedrawLocalizedElements();
        }

        /// <summary>
        /// Opens the color picker
        /// </summary>
        /// <param name="startColor">The color picker</param>
        /// <param name="initiator">The initiator</param>
        /// <param name="callback">The callback once the color is selected</param>
        public void OpenColorPicker(Color startColor, Selectable initiator, Action<Color> callback)
        {
            colorPicker.Open(startColor, initiator, callback);
        }

        /// <summary>
        /// Gets or create a tab from the settings
        /// </summary>
        /// <param name="tabKey">The tab's key</param>
        /// <returns>The tab's content root</returns>
        public RectTransform GetTab(string tabKey)
        {
            if (tabs.TryGetValue(tabKey, out SettingsTabUI tab))
                return tab.GetRoot();

            SettingsTabUI instance = Instantiate(tabPrefab, tabsRoot);
            instance.SetLabelKey(tabKey);
            tabs.Add(tabKey, instance);
            return instance.GetRoot();
        }

        /// <summary>
        /// Registers a new reset action for a button
        /// </summary>
        /// <param name="tabKey">The tab's key</param>
        /// <param name="action">The reset action</param>
        public void RegisterResetAction(string tabKey, UnityAction action)
        {
            if (tabs.TryGetValue(tabKey, out SettingsTabUI tab))
            {
                tab.RegisterResetAction(action);
                if (!objects.Contains(tab.GetResetButton()))
                    objects.Add(tab.GetResetButton());
            }
        }

        /// <summary>
        /// Creates a new toggle in the settings menu
        /// </summary>
        /// <param name="labelKey">The toggle's label</param>
        /// <param name="root">The toggle's root</param>
        /// <returns>The toggle</returns>
        public Toggle CreateToggle(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey, root, prefabToggleToggle);
        }

        /// <summary>
        /// Creates a new slider in the settings menu
        /// </summary>
        /// <param name="labelKey">The slider's label</param>
        /// <param name="root">The slider's root</param>
        /// <returns>The slider</returns>
        public Slider CreateSlider(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey, root, prefabToggleSlider);
        }

        /// <summary>
        /// Creates a new color picker in the settings menu
        /// </summary>
        /// <param name="labelKey">The color picker's label</param>
        /// <param name="root">The color picker's root</param>
        /// <returns>The color picker</returns>
        public Button CreateColorPicker(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey, root, prefabToggleColorPicker);
        }

        /// <summary>
        /// Creates a new button in the settings menu
        /// </summary>
        /// <param name="labelKey">The button's label</param>
        /// <param name="root">The button's root</param>
        /// <returns>The button</returns>
        public Button CreateButton(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey, root, prefabToggleButton);
        }

        /// <summary>
        /// Creates a new dropdown in the settings menu
        /// </summary>
        /// <param name="labelKey">The dropdown's label</param>
        /// <param name="root">The dropdown's root</param>
        /// <returns>The dropdown</returns>
        public TMP_Dropdown CreateDropdown(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey, root, prefabToggleDropdown);
        }

        /// <summary>
        /// Creates an instance of an entry prefab
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="labelKey">The label's key</param>
        /// <param name="root">The root</param>
        /// <param name="prefab">The prefab</param>
        /// <returns>The item created</returns>
        private T CreateEntryInstance<T>(string labelKey, RectTransform root, SettingsEntryUI<T> prefab) where T : Selectable
        {
            SettingsEntryUI<T> instance = Instantiate(prefab, root);
            instance.SetLabel(labelKey);

            objects.Add(instance.GetItem());


            return instance.GetItem();
        }
    }
}

