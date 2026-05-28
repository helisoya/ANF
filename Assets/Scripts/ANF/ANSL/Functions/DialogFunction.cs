using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine.InputSystem;


namespace ANF.ANSL
{
    /// <summary>
    /// The Dialog function can be used to show a dialog
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 22,
        functionBody: "dialog",
        functionAutoComplete: new string[] {
            "dialog(SpeakerId;CharacterId;DialogId)",
            "dialog(SpeakerId;CharacterId;DialogId;Additive)",
            "dialog(SpeakerId;CharacterId;DialogId;Additive;NoEndUserInput)",
            "dialog(SpeakerId;CharacterId;DialogId;Additive;NoEndUserInput;CloseAfterwards)",
        },
        functionDesc: "Shows a dialog")]
    public class DialogFunction : ANSLFunction
    {
        private DialogUI dialogUI;
        private bool closeAfterwards;
        private string characterId;
        private Persistent.AudioManager audioManager;

        private bool inputDetected;
        private bool waitingForEndInput;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL,
                    FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL,
                    FunctionParameterType.BOOL, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            inputDetected = false;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnDialogSkip;
            if (parameters.GetParameter(0, out string speakerId) &&
                parameters.GetParameter(1, out characterId) &&
                parameters.GetParameter(2, out string dialogId) &&
                manager.GetGUIManager().GetComponent<DialogUI>(out dialogUI))
            {
                if (PersistentDataManager.instance.GetPlayerData().GetComponent<HistoryContainer>(out HistoryContainer historyContainer))
                    historyContainer.AddDialog(dialogId, speakerId);

                dialogUI.GetSkipButton().onClick.AddListener(OnDialogSkip);
                bool additive;
                bool noEndUserInput;
                if (!parameters.GetParameter(3, out additive))
                    additive = false;
                if (!parameters.GetParameter(4, out noEndUserInput))
                    noEndUserInput = false;
                if (!parameters.GetParameter(5, out closeAfterwards))
                    closeAfterwards = false;

                waitingForEndInput = !noEndUserInput;

                dialogUI.StartDialog(speakerId, dialogId, additive);
                dialogUI.SetEnabled(true);

                if (characterId != null &&
                    manager.GetWorld().GetComponent<Scene.CharacterManager>(out Scene.CharacterManager characterManager))
                {
                    if (characterManager.GetSceneObject(characterId, out Scene.Character character))
                        character.SetIsTalking(true);
                }
            }
            else
            {
                // Parsing error and/or no dialogUI
                EndProcess();
            }
        }

        protected override void OnUpdate()
        {
            if (dialogUI == null)
            {
                manager.GetGUIManager().GetComponent<DialogUI>(out dialogUI);
                dialogUI.GetSkipButton().onClick.AddListener(OnDialogSkip);
                PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnDialogSkip;
            }

            if (!dialogUI.showingDialog)
            {
                if (characterId != null &&
                    manager.GetWorld().GetComponent<Scene.CharacterManager>(out Scene.CharacterManager characterManager))
                {
                    if (characterManager.GetSceneObject(characterId, out Scene.Character character))
                        character.SetIsTalking(false);
                }

                if (inputDetected)
                {
                    waitingForEndInput = false;
                    inputDetected = false;
                }


                if (!waitingForEndInput)
                {
                    dialogUI.GetSkipButton().onClick.RemoveListener(OnDialogSkip);

                    if (closeAfterwards)
                        dialogUI.SetEnabled(false);
                    EndProcess();
                }
            }
            else
            {
                if (inputDetected)
                {
                    dialogUI.ToggleCanSkip();
                    inputDetected = false;
                }
            }
        }

        protected override void OnCleanup()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnDialogSkip;
        }

        private void OnDialogSkip()
        {
            if (!context.isPaused)
                inputDetected = true;
        }

        private void OnDialogSkip(InputAction.CallbackContext callbackContext)
        {
            if (!context.isPaused && callbackContext.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                OnDialogSkip();
            }
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForEndInput", waitingForEndInput);
            json.Add("closeAfterwards", closeAfterwards);
            json.Add("characterId", characterId);
        }

        protected override void OnLoad(JSON json)
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            if (json.ContainsKey("waitingForEndInput"))
                waitingForEndInput = json.GetBool("waitingForEndInput");
            if (json.ContainsKey("closeAfterwards"))
                closeAfterwards = json.GetBool("closeAfterwards");
            if (json.ContainsKey("characterId"))
                characterId = json.GetString("characterId");
        }
    }
}

