using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.World
{
    /// <summary>
    /// Represents an object capable of loading and saving the state of world objects
    /// </summary>
    public interface WorldLoader : Jsonable
    {


        /// <summary>
        /// Initialize the loader
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public abstract void Initialize(ANFManager manager);

        /// <summary>
        /// Clones the loader
        /// </summary>
        /// <returns>The cloned loader</returns>
        public abstract WorldLoader CloneLoader();
    }
}

