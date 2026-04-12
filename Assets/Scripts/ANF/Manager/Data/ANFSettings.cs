using UnityEngine;

namespace ANF.Manager
{
    /// <summary>
    /// Represents the internal ANF Settings.
    /// For user settings such as the resolution and fonts, check UserSettings
    /// </summary>
    [CreateAssetMenu(fileName = "ANFSettings", menuName = "ANF/ANFSettings")]
    public class ANFSettings : ScriptableObject
    {
        [Header("ANSL")]
        [Tooltip("ANSL source files location")]
        public string anslSourceFolder;
        [Tooltip("ANSL destination file location (Is inside Resources/)")]
        public string anslDestinationFolder;
        [Tooltip("Path to the ANSL .code-snippets file (auto complete for VS code)")]
        public string anslVSCodeSnippetsPath;
        [Tooltip("How many ANSL function can be called per frame (per context)")]
        public uint anslMaxFunctionsPerFrame = 10;
        [Tooltip("How many concurrent contexts can coexist")]
        public uint anslMaxContexts = 20;
        [Tooltip("How large the script stack can be (per context)")]
        public uint anslContextStackLength = 10;
    }

}
