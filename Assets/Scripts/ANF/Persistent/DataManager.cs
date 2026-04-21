using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using System.IO;
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
        private Dictionary<string, DataContainer> containers;

        public DataManager(string name, ComponentRegisterEntry<DataContainer>[] containers, ANFSettings settings)
        {
            this.name = name;
            this.containers = new Dictionary<string, DataContainer>();

            foreach (ComponentRegisterEntry<DataContainer> entry in containers)
            {
                DataContainer copy = entry.data.CloneContainer();
                copy.Initialize(settings);

                this.containers.Add(entry.id, copy);
            }
        }

        /// <summary>
		/// Gets a specific data container
		/// </summary>
		/// <typeparam name="T">The container's type</typeparam>
		/// <param name="result">The out result</param>
		/// <returns>True if the container was found</returns>
        public bool GetDataContainer<T>(out T result) where T : DataContainer
        {
            foreach (DataContainer container in containers.Values)
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
        /// Gets a specific data container
        /// </summary>
        /// <typeparam name="id">The container's id</typeparam>
        /// <typeparam name="T">The container's type</typeparam>
        /// <param name="result">The out result</param>
        /// <returns>True if the container was found</returns>
        public bool GetDataContainer<T>(string id, out T result) where T : DataContainer
        {
            if (containers.TryGetValue(id, out DataContainer container))
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
            foreach (string key in containers.Keys)
                if (json.ContainsKey(key))
                    containers[key].Load(json.GetJSON(key));
        }

        /// <summary>
		/// Saves the data containers to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON individualDataJson;

            foreach (string containerId in containers.Keys)
            {
                individualDataJson = new JSON();
                containers[containerId].Save(individualDataJson);
                json.Add(containerId, individualDataJson);
            }
        }
    }
}
