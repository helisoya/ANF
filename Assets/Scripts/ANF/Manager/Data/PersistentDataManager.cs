using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Manager
{
    /// <summary>
	/// Handles persistent data (Sounds, User data, ...)
	/// </summary>
    public class PersistentDataManager : MonoBehaviour
    {
        public static PersistentDataManager instance { get; private set; }

        [Header("Data")]
        [SerializeField] private ANFSettings anfSettings;
        [SerializeField] private PlayerData playerData;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                playerData = new PlayerData(anfSettings);
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
		/// Gets the player's data
		/// </summary>
		/// <returns>The player's data</returns>
        public PlayerData GetPlayerData()
        {
            return playerData;
        }
    }
}
