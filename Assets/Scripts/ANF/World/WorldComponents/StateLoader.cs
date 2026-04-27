using ANF.ANSL;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// The state loader component is responsible for loading the state of the game
	/// </summary>
    [System.Serializable]
    public class StateLoader : WorldComponent
    {
#if UNITY_EDITOR
        [Header("Debug Mode")]
        [SerializeField] private bool startInDebugMode;
        [SerializeField] private string debugModeScript;
        [SerializeField] private bool autoLoadSaveFile;
        [SerializeField] private string saveFileToAutoLoad = "autosave";
#endif


        public override WorldComponent CloneComponent()
        {
            return new StateLoader()
            {
#if UNITY_EDITOR
                startInDebugMode = startInDebugMode,
                debugModeScript = debugModeScript,
                autoLoadSaveFile = autoLoadSaveFile,
                saveFileToAutoLoad = saveFileToAutoLoad
#endif
            };
        }



        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
#if UNITY_EDITOR
            if (autoLoadSaveFile)
            {
                string savePath = Utils.FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder
                    + saveFileToAutoLoad + ".json";
                SaveUtils.LoadPlayerData(PersistentDataManager.instance.GetPlayerData(), manager, savePath);
                return;
            }
            else if (startInDebugMode)
            {
                if (manager.GetWorld().GetComponent<ANSLManager>(out ANSLManager anslManager))
                    anslManager.StartNewContext(debugModeScript);
                return;
            }
#endif
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<LoadStateContainer>(out LoadStateContainer container))
            {
                if (container.loadingASaveFile)
                {
                    string savePath = Utils.FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder
                        + container.GetSaveFileToLoad() + ".json";
                    SaveUtils.LoadPlayerData(PersistentDataManager.instance.GetPlayerData(), manager, savePath);
                }
                else
                {
                    if (manager.GetWorld().GetComponent<ANSLManager>(out ANSLManager anslManager))
                        anslManager.StartNewContext(container.GetScriptToLoad());
                }
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }

        public override void OnSave(JSON json)
        {
        }

        public override void OnLoad(JSON json)
        {
        }
    }

}
