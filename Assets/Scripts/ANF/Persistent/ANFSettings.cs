using System.Collections.Generic;
using ANF.ANSL;
using ANF.GUI;
using ANF.Utils;
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
        public string saveFolder = "Saves/";
        [Tooltip("Path to the general data files (in Resources/) Ex: variables.txt, maps.txt")]
        public string generalDataPath = "General/";
        [Tooltip("Player datas containers are responsible for datas that are local to a save file (Ex: variables)")]
        public ComponentRegisterEntry<DataContainer>[] registeredPlayerDataContainers;
        [Tooltip("Global datas containers are responsible for datas that are not local to a save file (Ex: settings)")]
        public ComponentRegisterEntry<DataContainer>[] registeredGlobalDataContainers;

        [Header("World")]
        [Tooltip("World components are responsible for individual features. (Ex: Background Manager, ANSL Manager)")]
        public ComponentRegisterEntry<WorldComponent>[] registeredWorldComponents;
        [Tooltip("GUI components are responsible for drawing things on screen. (Ex : Pause menu, Fade, Dialogs, ...)")]
        public GUIRegisterEntry<GUIComponent>[] registeredGUIComponents;


        [Header("ANSL")]
        [Tooltip("ANSL source files location")]
        public string anslSourceFolder = "Assets/ANSL/";
        [Tooltip("ANSL destination file location (Is inside Resources/)")]
        public string anslDestinationFolder = "Story/";
        [Tooltip("Path to the ANSL .code-snippets file (auto complete for VS code)")]
        public string anslVSCodeSnippetsPath = ".vscode/";
    }

}
