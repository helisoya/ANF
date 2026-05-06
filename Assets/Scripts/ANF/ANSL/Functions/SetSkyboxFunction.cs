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
    /// The Set Skybox function can be used to change the skybox
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 31,
        functionBody: "setSkybox",
        functionAutoComplete: new string[] {
            "setSkybox(Skybox)"
        },
        functionDesc: "Changes the current background's skybox")]
    public class SetSkyboxFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string skybox) &&
                manager.GetWorld().GetComponent<ANF.Scene.BackgroundManager>(out ANF.Scene.BackgroundManager backgroundManager))
            {
                backgroundManager.SetSkybox(skybox);
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

