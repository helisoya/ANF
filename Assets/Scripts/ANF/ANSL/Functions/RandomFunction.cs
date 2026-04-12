using ANF.ANSL;
using ANF.Manager;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The random function generates a random value and stores it inside the "random" variable
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 5,
        functionBody: "random",
        functionAutoComplete: new string[] { "random(Min;Max)" },
        functionDesc: "Generates a random variable between [Min,Max[")]
    public class RandomFunction : ANSLFunction
    {


        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.INT, FunctionParameterType.INT}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetDataContainer(out PlayerVariableContainer container))
            {
                if (parameters.GetParameter(0, out int min) && parameters.GetParameter(1, out int max))
                    container.GenerateRandom(min, max);
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

