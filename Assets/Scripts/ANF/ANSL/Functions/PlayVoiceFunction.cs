using ANF.ANSL;
using ANF.Persistent;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Play Voice function can be used to play a Voice
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 53,
        functionBody: "playVoice",
        functionAutoComplete: new string[] {
            "playVoice(Voice)",
            "playVoice(Voice;BaseVolume)"
            },
        functionDesc: "Plays a Voice")]
    public class PlayVoiceFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Persistent.AudioManager>(out Persistent.AudioManager audioManager) &&
                parameters.GetParameter(0, out string name))
            {
                float baseVolume;
                if (parameters.GetTemplateId() == 0 || !parameters.GetParameter(1, out baseVolume))
                    baseVolume = 1.0f;

                audioManager.PlayVoice(name, baseVolume);
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

