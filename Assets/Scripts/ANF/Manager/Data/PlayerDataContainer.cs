using UnityEngine;
using Leguar.TotalJSON;

namespace ANF.Manager
{

    /// <summary>
	/// Represents a data container for the player
    /// Can save to and be loaded from a file
	/// </summary>
    public interface PlayerDataContainer
    {
        /// <summary>
        /// Gets the container's JSON name
        /// </summary>
        /// <returns>Its name</returns>
        public abstract string GetJSONName();

        /// <summary>
		/// Initialize the container
		/// </summary>
        /// <param name="settings">The ANF Settings</param>
        public abstract void Initialize(ANFSettings settings);

        /// <summary>
		/// Resets the container
		/// </summary>
        public abstract void Reset();

        /// <summary>
		/// Saves the container to a json
		/// </summary>
		/// <param name="json">The JSON to save to</param>
        public abstract void Save(JSON json);

        /// <summary>
		/// Loads the container from a json
		/// </summary>
		/// <param name="json">The json</param>
        public abstract void Load(JSON json);
    }
}
