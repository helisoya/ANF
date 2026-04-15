using System;
using System.Collections.Generic;
using System.IO;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.Persistent
{

    /// <summary>
	/// Handles multiple containers
	/// </summary>
    public class DataManager
    {
        public string name { get; private set; }
        private DataContainer[] containers;
        
        public DataManager(string name, DataContainer[] containers, ANFSettings settings)
        {
            this.name = name;
            this.containers = new DataContainer[containers.Length];

            for (int i = 0; i < containers.Length; i++)
            {
                this.containers[i] = containers[i].CloneContainer();
                this.containers[i].Initialize(settings);
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
            foreach (DataContainer container in containers)
            {
                if (container.GetType().IsSubclassOf(typeof(T)) || container.GetType() == typeof(T))
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
            foreach (DataContainer container in containers)
                if (json.ContainsKey(container.GetJSONName()))
                    container.Load(json.GetJSON(container.GetJSONName()));
        }

        /// <summary>
		/// Saves the data containers to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON dataJSON = new JSON();
            JSON individualDataJson;

            foreach (DataContainer container in containers)
            {
                individualDataJson = new JSON();
                container.Save(individualDataJson);
                dataJSON.Add(container.GetJSONName(), individualDataJson);
            }

            json.Add(name, dataJSON);
        }
    }
}
