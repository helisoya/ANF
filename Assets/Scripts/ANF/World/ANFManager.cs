using UnityEngine;
using ANF.ANSL;
using ANF.Persistent;

namespace ANF.World
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour
    {
        [Header("Base Components")]
        [SerializeField] private ANSLManager anslManager;
        private LoadersManager loadersManager;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled = false;
        [SerializeField] private string debugScriptToLoad;

        /// <summary>
        /// Gets the ANSL Manager
        /// </summary>
        /// <returns>The ANSL Manager</returns>
        public ANSLManager GetANSLManager()
        {
            return anslManager;
        }
        
        /// <summary>
        /// Gets the loader's manager
        /// </summary>
        /// <returns>The loader's manager</returns>
        public LoadersManager GetLoadersManager()
        {
            return loadersManager; 
        }

        void Start()
        {
            anslManager.Initialize(this);
            loadersManager = new LoadersManager("worldLoaders", PersistentDataManager.instance.GetANFSettings().registeredWorldLoaders, this);

            if (debugEnabled)
                anslManager.StartNewContext(debugScriptToLoad);
        }
    }
}
