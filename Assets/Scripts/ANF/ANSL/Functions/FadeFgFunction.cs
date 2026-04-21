using ANF.ANSL;
using ANF.GUI;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Fade Fg function can be used to fade the foreground
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 13,
        functionBody: "fadeFg",
        functionAutoComplete: new string[] {
            "fadeFg(Alpha;WaitForEnd)",
            "fadeFg(Alpha;WaitForEnd;Speed)" },
        functionDesc: "Fades the Foreground to a specific value")]
    public class FadeFgFunction : ANSLFunction
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

                manager.GetGUIManager().GetGUIComponent<GUI.Fade>("fadeFg", out currentFade);

                if (currentFade != null)
                    currentFade.FadeTo(target, false, speed);

                if (!waitingForFading || currentFade == null)
                    EndProcess();
            }
        }

        protected override void OnUpdate()
        {
            if (currentFade == null)
                manager.GetGUIManager().GetGUIComponent<GUI.Fade>("fadeFg", out currentFade);

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

