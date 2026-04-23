using ANF.ANSL;
using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Locals function allows you to change the additional files for the locals system
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 24,
        functionBody: "setLocals",
        functionAutoComplete: new string[] {
            "setLocals(Locals)"
        },
        functionDesc: "Shows/Hides the dialog window")]
    public class SetLocalsFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.LISTSTRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string[] list) &&
                PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out Locals.Locals locals))
            {
                if (list.Length == 0 && list[0] == null)
                    list = null;
                locals.ChangeAdditionalFiles(list);
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

        public override void Save(JSON json)
        {
            // Unused
        }

        public override void Load(JSON json)
        {
            // Unused
        }
    }
}

