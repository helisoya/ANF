using UnityEditor;
using UnityEngine;

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
            if (GUILayout.Button("Compile"))
            {

            }
        }
    }
}

