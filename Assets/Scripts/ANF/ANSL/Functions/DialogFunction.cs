using ANF.ANSL;
using ANF.GUI;
using Leguar.TotalJSON;
using UnityEngine;


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
        private bool noEndUserInput;
        private bool closeAfterwards;
        private string characterId;

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
            if (parameters.GetParameter(0, out string speakerId) &&
                parameters.GetParameter(1, out characterId) &&
                parameters.GetParameter(2, out string dialogId) &&
                manager.GetGUIManager().GetComponent<DialogUI>(out dialogUI))
            {
                bool additive;
                if (!parameters.GetParameter(3, out additive))
                    additive = false;
                if (!parameters.GetParameter(4, out noEndUserInput))
                    noEndUserInput = false;
                if (!parameters.GetParameter(5, out closeAfterwards))
                    closeAfterwards = false;

                dialogUI.StartDialog(speakerId, dialogId, additive);
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
                manager.GetGUIManager().GetComponent<DialogUI>(out dialogUI);

            if(!dialogUI.showingDialog)
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            // Unused
        }

        public override void Save(JSON json)
        {
            json.Add("noEndUserInput", noEndUserInput);
            json.Add("closeAfterwards", closeAfterwards);
            json.Add("characterId", characterId);
        }

        public override void Load(JSON json)
        {
            if (json.ContainsKey("noEndUserInput"))
                noEndUserInput = json.GetBool("noEndUserInput");
            if (json.ContainsKey("closeAfterwards"))
                closeAfterwards = json.GetBool("closeAfterwards");
            if (json.ContainsKey("characterId"))
                characterId = json.GetString("characterId");
        }
    }
}

