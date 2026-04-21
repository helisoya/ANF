using Leguar.TotalJSON;
using System.Collections.Generic;

namespace ANF.Utils
{

    /// <summary>
	/// Handles multiple containers/components
	/// </summary>
    public abstract class DataManager<T> : Jsonable where T : Jsonable
    {
        protected Dictionary<string, T> components;

        /// <summary>
		/// Gets a specific component
		/// </summary>
		/// <typeparam name="T">The component's type</typeparam>
		/// <param name="result">The out result</param>
		/// <returns>True if the component was found</returns>
        public bool GetComponent<P>(out P result) where P : T
        {
            foreach (T component in components.Values)
            {
                if (component.GetType().IsSubclassOf(typeof(T)) || component.GetType() == typeof(T))
                {
                    result = (P)component;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Gets a specific component
        /// </summary>
        /// <typeparam name="id">The component's id</typeparam>
        /// <typeparam name="P">The component's type</typeparam>
        /// <param name="result">The out result</param>
        /// <returns>True if the component was found</returns>
        public bool GetComponent<P>(string id, out P result) where P : T
        {
            if (components.TryGetValue(id, out T component))
            {
                if (component.GetType().IsSubclassOf(typeof(T)) || component.GetType() == typeof(T))
                {
                    result = (P)component;
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
            foreach (string key in components.Keys)
                if (json.ContainsKey(key))
                    components[key].Load(json.GetJSON(key));
        }

        /// <summary>
		/// Saves the data containers to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON individualDataJson;

            foreach (string containerId in components.Keys)
            {
                individualDataJson = new JSON();
                components[containerId].Save(individualDataJson);
                json.Add(containerId, individualDataJson);
            }
        }
    }
}
