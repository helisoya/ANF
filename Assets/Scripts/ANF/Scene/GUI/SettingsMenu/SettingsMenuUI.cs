using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
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
        [SerializeField] private SettingsTabUI[] tabsPrefabs;
        private SettingsTabUI[] tabs;

        [Header("Components")]
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private SettingsEntryUIToggle prefabToggleToggle;
        [SerializeField] private SettingsEntryUISlider prefabToggleSlider;
        [SerializeField] private SettingsEntryUIDropdown prefabToggleDropdown;
        [SerializeField] private SettingsEntryUIColorPicker prefabToggleColorPicker;

        private Persistent.AudioManager audioManager;
        private Selectable lastObject = null;

        public override void OnInitialize()
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {
        }


        public override void OnEnabled()
        {
            lastObject = null;

            foreach (Transform child in tabsRoot)
                Destroy(child.gameObject);

            tabs = new SettingsTabUI[tabsPrefabs.Length];
            for(int i = 0; i < tabsPrefabs.Length; i++)
            {
                tabs[i] = Instantiate(tabsPrefabs[i], tabsRoot);
                tabs[i].Initialize(this, manager);
            }

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

        /// <summary>
        /// Redraws the localized elements
        /// </summary>
        public void RedrawLocalizedElements()
        {
            foreach (SettingsTabUI tab in tabs)
                tab.RedrawLocalizedElements();
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
        /// Creates a new dropdown in the settings menu
        /// </summary>
        /// <param name="labelKey">The dropdown's label</param>
        /// <param name="root">The dropdown's root</param>
        /// <returns>The dropdown</returns>
        public TMP_Dropdown CreateDropdown(string labelKey, RectTransform root)
        {
            return CreateEntryInstance(labelKey,root,prefabToggleDropdown);
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

            if (lastObject == null)
            {
                EventSystem.current.SetSelectedGameObject(instance.GetItem().gameObject);
            }
            else
            {
                Navigation navigationTop = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    wrapAround = true,
                    selectOnDown = instance.GetItem(), 
                    selectOnUp = lastObject.navigation.selectOnUp
                };

                Navigation navigationDown = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    wrapAround = true,
                    selectOnDown = null,
                    selectOnUp = lastObject
                };

                lastObject.navigation = navigationTop;
                instance.GetItem().navigation = navigationDown;

            }
               
            lastObject = instance.GetItem();

            return instance.GetItem();
        }
    }
}

