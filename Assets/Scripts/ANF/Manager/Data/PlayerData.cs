using System.Collections.Generic;
using System.IO;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.Manager
{

    /// <summary>
	/// Handles the player's data (variables, player name, ...)
	/// </summary>
    public class PlayerData
    {
        private PlayerDataContainer[] containers;

        public PlayerData(ANFSettings settings)
        {
            containers = new PlayerDataContainer[settings.registeredPlayerDataContainers.Length];

            for (int i = 0; i < containers.Length; i++)
            {
                containers[i] = settings.registeredPlayerDataContainers[i].GetType().Instantiate() as PlayerDataContainer;
                containers[i].Initialize(settings);
            }
        }

        /// <summary>
		/// Gets a specific data container
		/// </summary>
		/// <typeparam name="T">The container's type</typeparam>
		/// <param name="result">The out result</param>
		/// <returns>True if the container was found</returns>
        public bool GetDataContainer<T>(out T result)
        {
            foreach (PlayerDataContainer container in containers)
            {
                if (containers.GetType().IsSubclassOf(typeof(T)) || containers.GetType() == typeof(T))
                {
                    result = (T)container;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
		/// Loads the data containers from the json
		/// </summary>
		/// <param name="json">The data containers</param>
        public void Load(JSON json)
        {
            foreach (PlayerDataContainer container in containers)
                if (json.ContainsKey(container.GetJSONName()))
                    container.Load(json.GetJSON(container.GetJSONName()));
        }

        /// <summary>
		/// Saves the data containers to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON playerDataJson = new JSON();
            JSON individualDataJson;

            foreach (PlayerDataContainer container in containers)
            {
                individualDataJson = new JSON();
                container.Save(individualDataJson);
                playerDataJson.Add(container.GetJSONName(), individualDataJson);
            }

            json.Add("playerData", playerDataJson);
        }
    }
}
