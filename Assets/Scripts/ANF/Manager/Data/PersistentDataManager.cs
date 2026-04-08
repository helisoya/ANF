using UnityEngine;

namespace ANF.Manager
{
    /// <summary>
	/// Handles persistent data (Sounds, User data, ...)
	/// </summary>
    public class PersistentDataManager : MonoBehaviour
    {
        public static PersistentDataManager instance { get; private set; }

        [Header("Data")]
        [SerializeField] private ANFSettings anfSettings;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets the internal ANF settings
        /// </summary>
        /// <returns>The ANF Settings</returns>
        public ANFSettings GetANFSettings()
        {
            return anfSettings;
        }
    }
}
