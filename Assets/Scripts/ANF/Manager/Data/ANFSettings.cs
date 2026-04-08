using UnityEngine;

namespace ANF.Manager
{
    /// <summary>
    /// Represents the internal ANF Settings.
    /// For user settings such as the resolution and fonts, check UserSettings
    /// </summary>
    [CreateAssetMenu(fileName = "ANFSettings",menuName = "ANF/ANFSettings")]
    public class ANFSettings : ScriptableObject
    {
        [Header("ANSL")]
        public string anslSourceFolder;
        public string anslDestinationFolder;
        public uint anslMaxFunctionsPerFrame = 10;
        public uint anslMaxContexts = 20;
    }

}
