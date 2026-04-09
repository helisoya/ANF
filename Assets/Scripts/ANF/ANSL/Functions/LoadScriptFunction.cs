using ANF.ANSL;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The load script function is used to change the current script of the context.
    /// If the change is immediate, the script will be loaded instantly. If not, it will wait for the next Update
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 1,
        functionBody: "load",
        functionAutoComplete: "load(SCRIPT;IMMEDIATE)",
        functionDesc: "Jump to is an internal function. It cannot be called from anywhere")]
    public class LoadScriptFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT, FunctionParameterType.BOOL}
            };
        }

        public override bool Compile()
        {
            return true;
        }

        protected override void OnStartProcess()
        {
            EndProcess(); // Flags the function as ended to avoid potential problems if the script is loaded instantly

            if (parameters.GetParameter(0, out string scriptFile))
            {
                bool immediate;
                uint startLine;
                if (parameters.GetTemplateId() == 0 &&
                parameters.GetParameter(1, out immediate))
                    context.LoadScript(scriptFile, 0, immediate);
                else if (parameters.GetTemplateId() == 1 &&
                parameters.GetParameter(1, out startLine) &&
                parameters.GetParameter(2, out immediate))
                    context.LoadScript(scriptFile, startLine, immediate);
            }

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

