using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using System.Collections.Generic;
using TMPro;
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
        [SerializeField] private SettingsTabUI prefabTab;
        [SerializeField] private Scrollbar scrollbar;
        private SettingsTabUI[] tabs;

        [Header("Components")]
        [SerializeField] private SettingsEntryUIToggle prefabToggleToggle;
        [SerializeField] private SettingsEntryUISlider prefabToggleSlider;
        [SerializeField] private SettingsEntryUIDropdown prefabToggleDropdown;
        [SerializeField] private SettingsEntryUIColorPicker prefabToggleColorPicker;

        private Persistent.AudioManager audioManager;
        private bool foundFirstObject = false;

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
            foundFirstObject = false;

            foreach (Transform child in tabsRoot)
                Destroy(child.gameObject);

            tabs = null;
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settingsContainer))
            {
                SettingsTab[] dataTabs = settingsContainer.GetTabs();


                tabs = new SettingsTabUI[dataTabs.Length];
                for (int i = 0; i < dataTabs.Length; i++)
                {
                    tabs[i] = Instantiate(prefabTab, tabsRoot);
                    tabs[i].Initialize(this, manager, dataTabs[i]);
                }
            }


            scrollbar.value = 1;

            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);
        }


        public override void OnDisabled()
        {
            EventSystem.current.SetSelectedGameObject(null);
            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);
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
        /// Creates a new toggle in the settings menu
        /// </summary>
        /// <param name="labelKey">The toggle's label</param>
        /// <param name="root">The toggle's root</param>
        /// <returns>The toggle</returns>
        public Toggle CreateToggle(string labelKey, RectTransform root)
        {
            SettingsEntryUIToggle toggle = Instantiate(prefabToggleToggle, root);
            toggle.SetLabel(labelKey);

            if (!foundFirstObject)
            {
                EventSystem.current.SetSelectedGameObject(toggle.GetItem().gameObject);
                foundFirstObject = true;
            }

            return toggle.GetItem();
        }

        /// <summary>
        /// Creates a new slider in the settings menu
        /// </summary>
        /// <param name="labelKey">The slider's label</param>
        /// <param name="root">The slider's root</param>
        /// <returns>The slider</returns>
        public Slider CreateSlider(string labelKey, RectTransform root)
        {
            SettingsEntryUISlider slider = Instantiate(prefabToggleSlider, root);
            slider.SetLabel(labelKey);

            if (!foundFirstObject)
            {
                EventSystem.current.SetSelectedGameObject(slider.GetItem().gameObject);
                foundFirstObject = true;
            }

            return slider.GetItem();
        }

        /// <summary>
        /// Creates a new color picker in the settings menu
        /// </summary>
        /// <param name="labelKey">The color picker's label</param>
        /// <param name="root">The color picker's root</param>
        /// <returns>The color picker</returns>
        public Button CreateColorPicker(string labelKey, RectTransform root)
        {
            SettingsEntryUIColorPicker colorPicker = Instantiate(prefabToggleColorPicker, root);
            colorPicker.SetLabel(labelKey);

            if (!foundFirstObject)
            {
                EventSystem.current.SetSelectedGameObject(colorPicker.GetItem().gameObject);
                foundFirstObject = true;
            }

            return colorPicker.GetItem();
        }

        /// <summary>
        /// Creates a new dropdown in the settings menu
        /// </summary>
        /// <param name="labelKey">The dropdown's label</param>
        /// <param name="root">The dropdown's root</param>
        /// <returns>The dropdown</returns>
        public TMP_Dropdown CreateDropdown(string labelKey, RectTransform root)
        {
            SettingsEntryUIDropdown dropdown = Instantiate(prefabToggleDropdown, root);
            dropdown.SetLabel(labelKey);

            if (!foundFirstObject)
            {
                EventSystem.current.SetSelectedGameObject(dropdown.GetItem().gameObject);
                foundFirstObject = true;
            }

            return dropdown.GetItem();
        }
    }
}

