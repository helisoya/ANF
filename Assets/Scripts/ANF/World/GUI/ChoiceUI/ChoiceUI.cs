using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the choice UI.
    /// The player can select an option, and load the linked script
    /// </summary>
    public class ChoiceUI : GUIComponent
    {
        [Header("Tittle")]
        [SerializeField] private Locals.LocalizedText titleText;
        [SerializeField] private RectTransform titleTransform;

        [Header("Buttons")]
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private ChoiceUIButton buttonPrefab;
        private ChoiceUIButton[] buttons;


        private ChoiceData currentData;
        private int currentButtonIndex;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement;

        public bool showingDialog { get; private set; } = false;
        public string selectedScript { get; private set; } = null;

        public override void OnInitialize()
        {
            titleTransform.anchoredPosition = new Vector2(0, titleTransform.sizeDelta.y / 2.0f);
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.5f;
                }
            }
        }

        /// <summary>
        /// Changes if the component is enabled or not.
        /// Also sets the currently displayed choice
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        /// <param name="choiceData">The choice's data</param>
        public void SetEnabled(bool enabled, ChoiceData choiceData)
        {
            if (enabled && !isEnabled)
            {
                currentData = choiceData;
                showingDialog = true;
                selectedScript = null;
                currentButtonIndex = 0;
            }

            SetEnabled(enabled);
        }

        public override void OnEnabled()
        {
            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;

            titleText.SetNewKey(currentData.title);
            titleTransform.DOAnchorPosY(-titleTransform.sizeDelta.y / 2.0f - 30f, 0.5f).SetEase(Ease.OutQuad);

            foreach(Transform child in buttonsRoot)
            {
                child.DOKill(false);
                Destroy(child.gameObject);
            }

            buttons = new ChoiceUIButton[currentData.entries.Length];

            for(int i = 0; i < buttons.Length; i++)
            {
                ChoiceUIButton button = Instantiate(buttonPrefab, buttonsRoot);
                button.Initialize(i, currentData.entries[i].textKey,this);
                buttons[i] = button;

                if (i == 0)
                    button.OnEnter();
            }
        }

        public override void OnDisabled()
        {
            titleTransform.DOAnchorPosY(titleTransform.sizeDelta.y / 2.0f, 0.5f).SetEase(Ease.OutQuad);
            for(int i = 0; i < buttons.Length;i++)
            {
                if (i == currentButtonIndex)
                    buttons[i].FadeAndDestroy(0.5f, () => { showingDialog = false; });
                else
                    buttons[i].FadeAndDestroy(0.0f, null);
            }
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled += OnMove;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled -= OnMove;
        }

        /// <summary>
        /// Selects a choice and closes the menu
        /// </summary>
        /// <param name="choiceIndex">The choice's index</param>
        public void SelectChoice(int choiceIndex)
        {
            if(showingDialog && isEnabled && !isPaused)
            {
                selectedScript = currentData.entries[choiceIndex].linkedScript;
                SetEnabled(false);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && showingDialog && context.ReadValueAsButton())
                SelectChoice(currentButtonIndex);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && showingDialog)
            {
                float value = context.ReadValue<Vector2>().y;

                if (Mathf.Abs(value) >= 0.9f)
                {
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide = value < 0 ? 1 : -1;
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
            if (force || currentButtonIndex != id)
            {
                buttons[currentButtonIndex].OnExit();
                currentButtonIndex = id;
                buttons[currentButtonIndex].OnEnter();
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            SetCurrentButton((currentButtonIndex + currentButtonInputSide + buttons.Length) % buttons.Length);
        }

        public override void OnChangeScene()
        {
        }

        public override void OnSave(JSON json)
        {
            json.Add("showingDialog", showingDialog);
            json.Add("selectedScript", selectedScript);

            if (showingDialog)
            {
                JSON choiceDataJson = new JSON();
                JArray choiceEntriesJson = new JArray();
                choiceDataJson.Add("title", currentData.title);

                foreach(ChoiceData.ChoiceDataEntry entry in currentData.entries)
                {
                    JSON entryJson = new JSON();
                    entryJson.Add("textKey", entry.textKey);
                    entryJson.Add("linkedScript", entry.linkedScript);

                    choiceEntriesJson.Add(entryJson);
                }

                choiceDataJson.Add("entries", choiceEntriesJson);

                json.Add("choiceData", choiceDataJson);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingDialog"))
                showingDialog = json.GetBool("showingDialog");

            if (json.ContainsKey("selectedScript"))
                selectedScript = json.GetString("selectedScript");

            if(showingDialog && json.ContainsKey("choiceData"))
            {
                JSON choiceData = json.GetJSON("choiceData");

                if (choiceData.ContainsKey("title"))
                    currentData.title = choiceData.GetString("title");

                if (choiceData.ContainsKey("entries"))
                {
                    JSON[] arrayData = choiceData.GetJArray("entries").AsJSONArray();

                    currentData.entries = new ChoiceData.ChoiceDataEntry[arrayData.Length];
                    for(int i = 0; i < arrayData.Length;i++)
                    {
                        if (arrayData[i].ContainsKey("textKey"))
                            currentData.entries[i].textKey = arrayData[i].GetString("textKey");

                        if (arrayData[i].ContainsKey("linkedScript"))
                            currentData.entries[i].linkedScript = arrayData[i].GetString("linkedScript");
                    }
                }

                SetEnabled(true, currentData);

                if (json.ContainsKey("isEnabled"))
                    json.Remove("isEnabled");
            }
        }
    }

    /// <summary>
    /// Represents a choice's data
    /// </summary>
    public struct ChoiceData
    {
        public string title;
        public ChoiceDataEntry[] entries;

        /// <summary>
        /// Represents an entry (button) in the choice
        /// </summary>
        public struct ChoiceDataEntry
        {
            public string textKey;
            public string linkedScript;
        }
    }
}

