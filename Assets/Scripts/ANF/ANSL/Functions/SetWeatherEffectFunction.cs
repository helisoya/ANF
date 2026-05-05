using ANF.ANSL;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Weather Effect can be used to change the current weather effect
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 30,
        functionBody: "setWeatherEffect",
        functionAutoComplete: new string[] {
            "setWeatherEffect(WeatherEffect)"
        },
        functionDesc: "Changes the current background's weather effect")]
    public class SetWeatherEffectFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string weatherEffect) &&
                manager.GetWorld().GetComponent<ANF.Scene.BackgroundManager>(out ANF.Scene.BackgroundManager backgroundManager))
            {
                backgroundManager.SetWeatherEffect(weatherEffect);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

