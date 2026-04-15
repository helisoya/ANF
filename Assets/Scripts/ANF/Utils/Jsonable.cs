using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
    /// A Jsonable can be loaded and saved to a JSON
    /// </summary>
    public interface Jsonable
    {
        /// <summary>
        /// Gets the object's JSON name
        /// </summary>
        /// <returns>Its name</returns>
        public abstract string GetJSONName();

        /// <summary>
		/// Saves the object to a json
		/// </summary>
		/// <param name="json">The JSON to save to</param>
        public abstract void Save(JSON json);

        /// <summary>
		/// Loads the object from a json
		/// </summary>
		/// <param name="json">The json</param>
        public abstract void Load(JSON json);
    }
}

