using ANF.World;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
    /// The default ANF World Loader.
    /// Loads and save the states of every base componens of ANF (ANSL, backgrounds, characters, ...)
    /// </summary>
    [System.Serializable]
    public class DefaultLoader : WorldLoader
    {
        private ANFManager manager;

        public void Initialize(ANFManager manager)
        {
            this.manager = manager;
        }

        public WorldLoader CloneLoader()
        {
            return new DefaultLoader();
        }

        public string GetJSONName()
        {
            return "defaultLoader";
        }

        public void Load(JSON json)
        {
        }

        public void Save(JSON json)
        {
        }
    }
}


