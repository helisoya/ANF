using ANF.ANSL;
using ANF.GUI;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Fade Bg function can be used to fade the background
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 12,
        functionBody: "fadeBg",
        functionAutoComplete: new string[] {
            "fadeBg(Alpha;WaitForEnd)",
            "fadeBg(Alpha;WaitForEnd;Speed)" },
        functionDesc: "Fades the Background to a specific value")]
    public class FadeBgFunction : ANSLFunction
    {
        private bool waitingForFading;
        private GUI.Fade currentFade;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.BOOL, FunctionParameterType.FLOAT},
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float target) &&
                parameters.GetParameter(1, out waitingForFading))
            {
                float speed;

                if (!parameters.GetParameter(2, out speed))
                    speed = 1.0f;

                manager.GetGUIManager().GetComponent<GUI.Fade>("fadeBg", out currentFade);

                if (currentFade != null)
                    currentFade.FadeTo(target, false, speed);

                if (!waitingForFading || currentFade == null)
                    EndProcess();
            }
        }

        protected override void OnUpdate()
        {
            if (currentFade == null)
                manager.GetGUIManager().GetComponent<GUI.Fade>("fadeBg", out currentFade);

            if (currentFade != null && !currentFade.fading)
            {
                waitingForFading = false;
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            // Unused
        }

        public override void Save(JSON json)
        {
            json.Add("waitingForFading", waitingForFading);
        }

        public override void Load(JSON json)
        {
            if (json.ContainsKey("waitingForFading"))
                waitingForFading = json.GetBool("waitingForFading");
        }
    }
}

