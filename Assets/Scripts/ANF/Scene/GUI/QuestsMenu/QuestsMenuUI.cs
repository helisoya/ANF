using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the quest menu located within the pause menu
    /// </summary>
    public class QuestsMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Quest Buttons")]
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private QuestsMenuUIButton buttonPrefab;
        [SerializeField] private QuestsMenuUICategory categoryPrefab;

        [Header("Quest Info")]
        [SerializeField] private Locals.LocalizedText infoNameText;
        [SerializeField] private Locals.LocalizedText infoDescText;
        [SerializeField] private Transform infoObjectivesRoot;
        [SerializeField] private Locals.LocalizedText infoObjectivePrefab;

        private int currentButtonIdx;
        private List<QuestsMenuUIButton> buttons;
        private int currentButtonInputSide = 0;
        private float cooldownToNextButtonIncrement = 0;

        private Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> visibleQuests;

        public override void OnInitialize()
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
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
        /// Computes the list of visible quests by category
        /// </summary>
        /// <returns>The list of visible quests</returns>
        private Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> ComputeVisibleQuests()
        {
            Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> result = new();

            if(PersistentDataManager.instance.GetGlobalData().GetComponent<QuestInfosContainer>(out QuestInfosContainer questInfos) &&
                PersistentDataManager.instance.GetGlobalData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer playerVariables))
            {
                Dictionary<string, List<Persistent.QuestInfo>> quests = questInfos.GetQuests();

                List<KeyValuePair<Persistent.QuestInfo, int>> tmpList = null;
                int tmpValue = 0;

                foreach (string category in quests.Keys)
                {
                    tmpList = new();

                    foreach(ANF.Persistent.QuestInfo info in quests[category])
                    {
                        if(playerVariables.GetVariable(info.variableID, out tmpValue) && tmpValue > -1)
                        {
                            tmpList.Add(new(info, tmpValue));
                        }
                    }

                    if(tmpList.Count > 0)
                    {
                        result.Add(category, tmpList);
                    }
                }
            }

            return result;
        }

        public override void OnEnabled()
        {
            visibleQuests = ComputeVisibleQuests();

            foreach (Transform child in buttonsRoot)
                Destroy(child.gameObject);

            buttons = new();

            int id = 0;
            foreach(string category in visibleQuests.Keys)
            {
                Instantiate(categoryPrefab, buttonsRoot).SetLabelKey(category);

                foreach(KeyValuePair<Persistent.QuestInfo,int> info in visibleQuests[category])
                {
                    QuestsMenuUIButton button = Instantiate(buttonPrefab, buttonsRoot);
                    button.Initialize(id,this,info);
                    id++;
                    buttons.Add(button);
                }
            }

            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;


            if(visibleQuests.Count != 0)
            {
                SetCurrentButton(0, true);
            }
            else
            {
                // No visible quests

                infoNameText.SetNewKey("QuestsMenu_Unknown");
                infoDescText.SetNewKey("QuestsMenu_Unknown");

                foreach (Transform child in infoObjectivesRoot)
                    Destroy(child.gameObject);
            }
            
        }

        public override void OnDisabled()
        {
            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);
            visibleQuests = null;
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && buttons.Count != 0 && !isPaused && context.ReadValueAsButton())
            {
                ShowQuest(buttons[currentButtonIdx].GetData());
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                SetEnabled(false);
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && buttons.Count != 0)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide = value.x < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (noMovement)
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
            if (id < 0)
                return;

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
            SetCurrentButton((currentButtonIdx + currentButtonInputSide + buttons.Count) % buttons.Count);
        }

        /// <summary>
        /// Shows a quest's infos on screen
        /// </summary>
        /// <param name="data">The quest data</param>
        public void ShowQuest(KeyValuePair<Persistent.QuestInfo, int> data)
        {
            infoNameText.SetNewKey(data.Key.GetNameKey());
            infoDescText.SetNewKey(data.Key.GetDescKey());

            foreach (Transform child in infoObjectivesRoot)
                Destroy(child.gameObject);

            int max = data.Value >= 100 ? data.Key.maxQuestState : data.Value;

            for(int i = 0; i <= max;i++)
            {
                Locals.LocalizedText text = Instantiate(infoObjectivePrefab, infoObjectivesRoot);
                text.SetNewKey(data.Key.GetStateKey(i));
                text.GetText().fontStyle = (i == max && data.Value < 100) ? TMPro.FontStyles.Normal : TMPro.FontStyles.Strikethrough;
            }

            if (data.Value == 100)
                Instantiate(infoObjectivePrefab, infoObjectivesRoot).SetNewKey(data.Key.GetDoneKey());
            else if(data.Value == 101)
                Instantiate(infoObjectivePrefab, infoObjectivesRoot).SetNewKey(data.Key.GetCanceledKey());
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Pause").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled -= OnMove;
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
    }
}

