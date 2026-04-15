using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
    /// Handles the different world loaders
    /// </summary>
    public class LoadersManager
    {
        public string name { get; private set; }
        private WorldLoader[] loaders;

        public LoadersManager(string name, WorldLoader[] loaders, ANFManager manager)
        {
            this.name = name;
            this.loaders = new WorldLoader[loaders.Length];

            for (int i = 0; i < loaders.Length; i++)
            {
                this.loaders[i] = loaders[i].CloneLoader();
                this.loaders[i].Initialize(manager);
            }
        }

        /// <summary>
		/// Gets a specific loader
		/// </summary>
		/// <typeparam name="T">The loader's type</typeparam>
		/// <param name="result">The out result</param>
		/// <returns>True if the container was found</returns>
        public bool GetLoader<T>(out T result)
        {
            foreach (WorldLoader loader in loaders)
            {
                if (loader.GetType().IsSubclassOf(typeof(T)) || loader.GetType() == typeof(T))
                {
                    result = (T)loader;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
		/// Loads every loader from the json
		/// </summary>
		/// <param name="json">The data containers</param>
        public void Load(JSON json)
        {
            foreach (WorldLoader loader in loaders)
                if (json.ContainsKey(loader.GetJSONName()))
                    loader.Load(json.GetJSON(loader.GetJSONName()));
        }

        /// <summary>
		/// Saves every loader to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON dataJSON = new JSON();
            JSON individualDataJson;

            foreach (WorldLoader loader in loaders)
            {
                individualDataJson = new JSON();
                loader.Save(individualDataJson);
                dataJSON.Add(loader.GetJSONName(), individualDataJson);
            }

            json.Add(name, dataJSON);
        }
    }
}

