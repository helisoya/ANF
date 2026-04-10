using UnityEngine;
using ANF.ANSL;

namespace ANF.Manager
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour
    {
        [Header("Base Components")]
        [SerializeField] private ANSLManager anslManager;

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

        void Start()
        {
            anslManager.Initialize(this);

            if (debugEnabled)
                anslManager.StartNewContext(debugScriptToLoad);
        }
    }
}
