using UnityEngine;

namespace ANF.Manager
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour
    {
        [Header("Base Component")]
        [SerializeField] private ANSLManager anslManager;

        /// <summary>
        /// Gets the ANSL Manager
        /// </summary>
        /// <returns>The ANSL Manager</returns>
        public ANSLManager GetANSLManager()
        {
            return anslManager;
        }

        void Update()
        {
            anslManager.Initialize(this);

        }
    }
}
