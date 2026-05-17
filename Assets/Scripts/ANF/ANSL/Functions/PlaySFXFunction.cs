using ANF.ANSL;
using ANF.Persistent;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Play SFX function can be used to play an SFX
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 52,
        functionBody: "playSFX",
        functionAutoComplete: new string[] {
            "playSFX(Sfx)",
            "playSFX(Sfx;BaseVolume)"
            },
        functionDesc: "Plays an SFX")]
    public class PlaySFXFunction : ANSLFunction
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
            if (PersistentDataManager.instance.GetPlayerData().GetComponent<Persistent.AudioManager>(out Persistent.AudioManager audioManager) &&
                parameters.GetParameter(0, out string name))
            {
                float baseVolume;
                if (parameters.GetTemplateId() == 0 || !parameters.GetParameter(1, out baseVolume))
                    baseVolume = 1.0f;

                audioManager.PlaySFX(name, baseVolume);
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

