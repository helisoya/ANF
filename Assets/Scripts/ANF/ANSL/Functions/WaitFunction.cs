using ANF.ANSL;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The wait function can be used to wait for a certain number of seconds
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 4,
        functionBody: "wait",
        functionAutoComplete: new string[] { "wait(Seconds)" },
        functionDesc: "Waits for a certain number of seconds")]
    public class WaitFunction : ANSLFunction
    {
        private float timeToWait;


        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float time))
                timeToWait = time;
            else
                EndProcess();
        }

        protected override void OnUpdate()
        {
            timeToWait -= Time.deltaTime;
            if (timeToWait <= 0)
                EndProcess();
        }

        protected override void OnCleanup()
        {
            // Unused
        }

        public override void Save(JSON json)
        {
            json.Add("timeToWait", timeToWait);
        }

        public override void Load(JSON json)
        {
            if (json.ContainsKey("timeToWait"))
                timeToWait = json.GetFloat("timeToWait");
        }
    }
}

