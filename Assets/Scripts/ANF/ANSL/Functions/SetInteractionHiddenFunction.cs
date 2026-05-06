using ANF.ANSL;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Interaction Hidden can be used to change if an interactable object should be hidden or not
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 32,
        functionBody: "setInteractionHidden",
        functionAutoComplete: new string[] {
            "setInteractionHidden(Id;Hidden)"
        },
        functionDesc: "Changes if interactable object is hidden or not")]
    public class SetInteractionHiddenFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out bool hidden) &&
                manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode))
            {
                interactionMode.SetInteractableObjectHidden(id, hidden);
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

