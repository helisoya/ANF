using ANF.ANSL;
using ANF.GUI;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Color Bg function can be used to change the background's fade color
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 14,
        functionBody: "colorBg",
        functionAutoComplete: new string[] { "colorBg(R;G;B;A)" },
        functionDesc: "Changes the color of the background fade")]
    public class SetBgColorFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float r) &&
                parameters.GetParameter(1, out float g) &&
                parameters.GetParameter(2, out float b) &&
                parameters.GetParameter(3, out float a) &&
                manager.GetGUIManager().GetComponent<GUI.Fade>("fadeBg", out GUI.Fade currentFade))
            {
                currentFade.SetColor(new Color(r, g, b, a));
            }
            EndProcess();
        }

        protected override void OnUpdate()
        {
            // Unused
        }

        protected override void OnCleanup()
        {
            // Unused
        }
    }
}

