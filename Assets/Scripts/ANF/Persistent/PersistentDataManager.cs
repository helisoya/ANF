using ANF.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
	/// Handles persistent data (Sounds, User data, ...)
	/// </summary>
    public class PersistentDataManager : MonoBehaviour
    {
        public static PersistentDataManager instance { get; private set; }

        [Header("Data")]
        [SerializeField] private ANFSettings anfSettings;
        [SerializeField] private DataManager playerData;
        [SerializeField] private DataManager globalData;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                playerData = new DataManager("saveData",anfSettings.registeredPlayerDataContainers,anfSettings);
                globalData = new DataManager("globalData", anfSettings.registeredGlobalDataContainers, anfSettings);

                string globalDataSaveFile = FileManager.savPath + anfSettings.saveFolder + "global.json";
                if (SaveUtils.FileExists(globalDataSaveFile))
                    SaveUtils.LoadGlobalData(globalData, globalDataSaveFile);
                else
                    SaveUtils.SaveGlobalData(globalData, globalDataSaveFile);

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets the internal ANF settings
        /// </summary>
        /// <returns>The ANF Settings</returns>
        public ANFSettings GetANFSettings()
        {
            return anfSettings;
        }

        /// <summary>
		/// Gets the player's data (data that is local to a save)
		/// </summary>
		/// <returns>The player's data</returns>
        public DataManager GetPlayerData()
        {
            return playerData;
        }

        /// <summary>
        /// Gets the global data (data that is not local to a save, ex: Settings)
        /// </summary>
        /// <returns>The global data</returns>
        public DataManager GetGlobalData()
        {
            return globalData;
        }
    }
}
