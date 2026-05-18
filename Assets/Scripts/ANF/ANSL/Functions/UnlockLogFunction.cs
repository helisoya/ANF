using ANF.ANSL;
using ANF.Persistent;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Unlock Log function can be used to unlock a new log for the player to read
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 56,
        functionBody: "unlockLog",
        functionAutoComplete: new string[] {
            "unlockLog(Log)"
            },
        functionDesc: "Unlocks a log")]
    public class UnlockLogFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent<LogsContainer>(out LogsContainer logsContainer) &&
                parameters.GetParameter(0, out string logId))
            {
                logsContainer.UnlockLog(logId);
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

