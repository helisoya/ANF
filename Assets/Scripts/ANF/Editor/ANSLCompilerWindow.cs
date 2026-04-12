using ANF.ANSL;
using ANF.Manager;
using ANF.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static ANF.Editor.ANSLCompilerWindow;
using static LJDNUtilsEditor;

namespace ANF.Editor
{
    /// <summary>
    /// Represents the ANSL Compiler Window.
    /// Used to compile and show scripting errors
    /// </summary>
    public class ANSLCompilerWindow : EditorWindow
    {

        [MenuItem("ANF/ANSL Compiler")]
        public static void OpenWindow()
        {
            ANSLCompilerWindow wnd = GetWindow<ANSLCompilerWindow>();
            wnd.titleContent = new GUIContent("ANSL Compiler");
        }


        public void OnGUI()
        {
            if (GUILayout.Button("Regenerate VS Code Snippets"))
            {
                ANFSettings settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");
                if (settings != null)
                    ANSLUtils.RegenerateVSCodeSnippets(settings.anslVSCodeSnippetsPath);
            }

            if (GUILayout.Button("Compile ANSL Files"))
            {
                CompileANSLFiles();
            }
        }

        /// <summary>
        /// Compiles the ANSL files of the project
        /// </summary>
        private void CompileANSLFiles()
        {
            ANFSettings settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");

            if (settings != null)
            {
                List<ANSLUtils.ANSLError> errors = ANSLUtils.CompileAll(settings);

                ANSLErrorListPopup.Show(errors);
            }
        }
    }

    /// <summary>
    /// Represents a popup for the ANSL Errors
    /// </summary>
    public class ANSLErrorListPopup : EditorWindow
    {

        private List<ANSLUtils.ANSLError> data;

        private Vector2 scrollPosition;

        /// <summary>
		/// Initialize the popup
		/// </summary>
		/// <param name="data">The ANSL error's data</param>
        public void Init(List<ANSLUtils.ANSLError> data)
        {
            this.data = data;
            this.scrollPosition = Vector2.zero;
        }

        /// <summary>
		/// Shows the popup
		/// </summary>
		/// <param name="data">The error's data</param>
        public static void Show(List<ANSLUtils.ANSLError> data)
        {
            ANSLErrorListPopup wnd = GetWindow<ANSLErrorListPopup>();
            wnd.titleContent = new GUIContent("Errors");
            wnd.minSize = new Vector2(600, 300);
            wnd.maxSize = new Vector2(750, 600);
            wnd.Init(data);
        }

        public void OnGUI()
        {
            if (data == null) return;

            if (data.Count == 0)
            {
                GUILayout.Label("No errors detected");
            }
            else
            {
                GUILayoutOption[] options2 = { GUILayout.Width(50) };
                GUILayoutOption[] options = { GUILayout.Width(250) };
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < data.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(data[i].type.ToString(), options);
                    if (data[i].type == ANSLUtils.ANSLErrorType.FUNCTION)
                    {
                        GUILayout.Label(data[i].filePath, options);
                    }
                    else
                    {
                        GUILayout.Label($"{data[i].filePath}, {data[i].line}", options);
                    }

                    GUILayout.Label(data[i].errorMessage, options);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }
    }
}

