using ANF.GUI;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the save menu
    /// </summary>
    [System.Serializable]
    public class PauseMenuUI : GUIComponent
    {
        [Header("Infos")]
        [SerializeField] private string[] guiComponentsToPause = { "fadeBg", "fadeFg", "dialog" };
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Base UI")]
        [SerializeField] private RectTransform buttonsRoot;

        [Header("Buttons")]
        [SerializeReference, SubclassSelector(AllowNull = false)] private PauseMenuButtonData[] buttonDatas;
        [SerializeField] private PauseMenuButton buttonPrefab;
        private int currentButtonIdx;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement;
        private PauseMenuButton[] buttons;

        public override void OnInitialize()
        {
            buttonsRoot.anchoredPosition = new Vector2(-buttonsRoot.sizeDelta.x / 2f, 0);

            buttons = new PauseMenuButton[buttonDatas.Length];
            for (int i = 0; i < buttonDatas.Length; i++)
            {
                buttons[i] = Instantiate(buttonPrefab, buttonsRoot);
                buttons[i].Initialize(i, buttonDatas[i], this, manager);
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Pause").performed += OnPauseInput;
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.25f;
                }
            }
        }

        protected override void OnClose()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled -= OnMove;

            buttonsRoot.DOComplete();
            float halfSizeButtonsRoot = buttonsRoot.sizeDelta.x / 2f;
            buttonsRoot.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsEnabled(guiComponentsToPause, true);
            manager.GetWorld().EnableWorldComponents(true);
            if (gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if (dialog.isOpen)
                    dialog.SetIsEnabled(true);
            }
        }

        protected override void OnOpen()
        {
            SetCurrentButton(0, true);

            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;

            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled += OnMove;

            buttonsRoot.DOComplete();
            float halfSizeButtonsRoot = buttonsRoot.sizeDelta.x / 2f;
            buttonsRoot.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsEnabled(guiComponentsToPause, false);
            manager.GetWorld().EnableWorldComponents(false);
            if (gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if (dialog.isOpen)
                    dialog.SetIsEnabled(false);
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                if (!isOpen)
                    Open();
                else
                    Close();
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isOpen && isEnabled && context.ReadValueAsButton())
                buttons[currentButtonIdx].OnClick();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isOpen && isEnabled)
            {
                float value = context.ReadValue<Vector2>().y;

                if (Mathf.Abs(value) >= 0.9f)
                {
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = 0.25f;
                        currentButtonInputSide = value < 0 ? -1 : 1;
                        IncrementButtonWithInput();
                    }
                }
                else
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide = 0;
                }
            }
        }

        /// <summary>
		/// Changes the current button
		/// </summary>
		/// <param name="id">The new button's id</param>
        /// <param name="force">True if the id check should be skipped</param>
        public void SetCurrentButton(int id, bool force = false)
        {
            if (force || currentButtonIdx != id)
            {
                buttons[currentButtonIdx].OnExit();
                currentButtonIdx = id;
                buttons[currentButtonIdx].OnEnter();
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            SetCurrentButton((currentButtonIdx + currentButtonInputSide + buttons.Length) % buttons.Length);
        }

        protected override void OnSave(JSON json)
        {
        }
        protected override void OnLoad(JSON json)
        {
        }
    }
}
