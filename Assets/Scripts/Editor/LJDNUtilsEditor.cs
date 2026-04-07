using System.Collections.Generic;
using System.IO;
using System.Text;
using PlasticGui;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents the LJDN's Utils window
/// </summary>
public class LJDNUtilsEditor : EditorWindow
{
    public struct MissingLocalData
    {
        public string file;
        public string missingLocal;
        public MissingLocalItemType type;
    }

    public enum MissingLocalItemType
    {
        MISSING_LOCAL,
        MISSING_ITEM
    }

    [MenuItem("LJDN/Utils")]
    public static void ShowExample()
    {
        LJDNUtilsEditor wnd = GetWindow<LJDNUtilsEditor>();
        wnd.titleContent = new GUIContent("LJDN Utils");
    }


    public void OnGUI()
    {
        if (GUILayout.Button("Check Locals"))
        {
            CheckLocals();
        }
    }

    /// <summary>
    /// Checks missing locals
    /// </summary>
    private void CheckLocals()
    {
        List<MissingLocalData> data = new List<MissingLocalData>();

        string[] languages = { "eng", "fra" };
        string[] files = { "common", "system", "endings", "forest", "intro", "islandRegion", "kaliaCity", "kaliaRegion", "kosky", "marklicht", "orkvaRegion" };

        foreach (string originalLang in languages)
        {
            foreach (string toCheckLanguage in languages)
            {
                if (originalLang.Equals(toCheckLanguage)) continue;

                foreach (string file in files)
                {
                    FindMissingLocals($"Assets/Resources/Locals/{originalLang}_{file}.txt",
                    $"Assets/Resources/Locals/{toCheckLanguage}_{file}.txt", data);
                }
            }

            FindMissingItemLocals($"Assets/Resources/Locals/{originalLang}_system.txt", data);
        }

        if (data.Count > 0)
        {
            LJDNUtilsPopupEditor.Show(data);
        }
    }

    private void FindMissingLocals(string original, string toCheck, List<MissingLocalData> data)
    {
        if (!File.Exists(original) || !File.Exists(toCheck)) return;

        string[] lines = File.ReadAllLines(original);
        List<string> existsInOriginal = new List<string>();
        string[] tmpSplit;

        foreach (string line in lines)
        {
            if (line.StartsWith("#") || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;

            tmpSplit = line.Split(" = ");
            existsInOriginal.Add(tmpSplit[0]);
        }

        lines = File.ReadAllLines(toCheck);
        foreach (string line in lines)
        {
            if (line.StartsWith("#")) continue;

            tmpSplit = line.Split(" = ");
            if (existsInOriginal.Contains(tmpSplit[0]))
                existsInOriginal.Remove(tmpSplit[0]);
        }

        foreach (string line in existsInOriginal)
        {
            data.Add(new MissingLocalData() { file = toCheck, missingLocal = line, type = MissingLocalItemType.MISSING_LOCAL });
        }
    }

    public void FindMissingItemLocals(string filePath, List<MissingLocalData> data)
    {
        if (File.Exists(filePath))
        {
            string[] filesUnCleaned = Directory.GetFiles("Assets/Resources/RPG/Items/");
            List<string> files = new List<string>();
            int start;
            int end;
            foreach (string file in filesUnCleaned)
            {
                if (!file.EndsWith(".meta"))
                {
                    start = file.LastIndexOf("/") + 1;
                    end = file.LastIndexOf(".");
                    files.Add(file.Substring(start, end - start));
                }
            }


            string[] lines = File.ReadAllLines(filePath);
            string line;
            for (int i = 0; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                line = line.Split(" = ")[0];
                if (line.EndsWith("_name"))
                {
                    line = line.Substring(0, line.Length - 5);
                    files.Remove(line);
                }
            }

            if (files.Count == 0) return;

            foreach (string file in files)
            {
                data.Add(new MissingLocalData() { file = filePath, missingLocal = file + "_name", type = MissingLocalItemType.MISSING_ITEM });
                data.Add(new MissingLocalData() { file = filePath, missingLocal = file + "_desc", type = MissingLocalItemType.MISSING_ITEM });
            }
        }
    }

    /// <summary>
	/// Represents a popup for the locals debug
	/// </summary>
    public class LJDNUtilsPopupEditor : EditorWindow
    {

        private List<MissingLocalData> data;

        private Vector2 scrollPosition;
        private bool[] checks;

        /// <summary>
		/// Initialize the popup
		/// </summary>
		/// <param name="data">The missing local's data</param>
        public void Init(List<MissingLocalData> data)
        {
            this.data = data;
            this.scrollPosition = Vector2.zero;
            if (data == null)
            {
                checks = null;

            }
            else
            {
                checks = new bool[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    checks[i] = true;
                }
            }
        }

        /// <summary>
		/// Shows the popup
		/// </summary>
		/// <param name="data">The missing local's data</param>
        public static void Show(List<MissingLocalData> data)
        {
            LJDNUtilsPopupEditor wnd = GetWindow<LJDNUtilsPopupEditor>();
            wnd.titleContent = new GUIContent("Local Popup");
            wnd.minSize = new Vector2(800, 300);
            wnd.maxSize = new Vector2(800, 600);
            wnd.Init(data);
        }

        public void OnGUI()
        {
            if (data == null) return;

            if (data.Count == 0)
            {
                GUILayout.Label("No missing locals detected");
            }
            else
            {
                GUILayoutOption[] options2 = { GUILayout.Width(50) };
                GUILayoutOption[] options = { GUILayout.Width(250) };
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < data.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    checks[i] = GUILayout.Toggle(checks[i], "", options2);
                    GUILayout.Label(data[i].file, options);
                    GUILayout.Label(data[i].missingLocal, options);
                    GUILayout.Label(data[i].type.ToString(), options);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Toggle All"))
                {
                    SetAllChecksTo(!checks[0]);
                }

                if (GUILayout.Button("Fix Selected"))
                {
                    FixAndClose();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
		/// Toggles all checks to a specific value
		/// </summary>
		/// <param name="value">The new value</param>
        public void SetAllChecksTo(bool value)
        {
            for (int i = 0; i < checks.Length; i++)
            {
                checks[i] = value;
            }
        }

        /// <summary>
		/// Fixes the selected issues and closes the popup
		/// </summary>
        public void FixAndClose()
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (checks[i]) FixData(data[i]);
            }
            Close();
        }

        /// <summary>
		/// Fixes an issue
		/// </summary>
		/// <param name="data"></param>
        public void FixData(MissingLocalData data)
        {
            using (StreamWriter sw = new StreamWriter(data.file, true))
            {
                sw.Write("\n");
                sw.WriteLine(data.missingLocal + " = TODO");
            }
        }
    }
}
