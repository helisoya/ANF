using System.Collections.Generic;
using ANF.ANSL;
using ANF.World;
using UnityEngine;

namespace ANF.Persistent
{

    /// <summary>
    /// Represents the internal ANF Settings.
    /// For user settings such as the resolution and fonts, check UserSettings
    /// </summary>
    [CreateAssetMenu(fileName = "ANFSettings", menuName = "ANF/ANFSettings")]
    public class ANFSettings : ScriptableObject
    {
        [Header("Data")]
        [Tooltip("Filepath to the save files (Inside Assets/ in the editor, and inside the game's persistent data folder in build)")]
        public string saveFolder;
        [Tooltip("Path to the general data files (in Resources/) Ex: variables.txt, maps.txt")]
        public string generalDataPath;
        [Tooltip("Player datas containers are responsible for datas that are local to a save file (Ex: variables)")]
        [SerializeReference, SubclassSelector(AllowNull = false)] public DataContainer[] registeredPlayerDataContainers;
        [Tooltip("Global datas containers are responsible for datas that are not local to a save file (Ex: settings)")]
        [SerializeReference, SubclassSelector(AllowNull = false)] public DataContainer[] registeredGlobalDataContainers;

        [Header("World")]
        [Tooltip("World loaders are responsible for loading and saving world data (Ex : Characters). DefaultLoader handles all base components of ANF.")]
        [SerializeReference, SubclassSelector(AllowNull = false)] public WorldLoader[] registeredWorldLoaders;

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
