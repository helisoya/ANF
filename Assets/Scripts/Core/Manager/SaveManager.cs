using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles the player's save file
/// </summary>
public class SaveManager
{
    private string savePath = FileManager.savPath + "Saves/";
    private List<GAMEFILE.VARIABLE> items;
    public GAMEFILE saveFile { get; private set; }


    /// <summary>
    /// Checks if a savefile exists on disk
    /// </summary>
    /// <param name="saveName">The save's name</param>
    /// <returns>True if the savefile exists on disk</returns>
    public bool SaveFileExists(string saveName)
    {
        return File.Exists(savePath + saveName + ".txt");
    }

    public SaveManager()
    {
        InitSaveFile();
    }

    /// <summary>
    /// Initializes the save file
    /// </summary>
    private void InitSaveFile()
    {
        saveFile = new GAMEFILE();
        items = new List<GAMEFILE.VARIABLE>();

        List<string> lines = FileManager.ReadTextAsset(Resources.Load<TextAsset>("General/items"));
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
            {
                string[] split = line.Split(' ');
                items.Add(new GAMEFILE.VARIABLE(split[0], split[1]));
            }
        }
    }

    /// <summary>
    /// Resets the items to their default values
    /// </summary>
    public void ResetVariables()
    {
        foreach (GAMEFILE.VARIABLE item in items)
        {
            item.value = item.defaultValue;
        }

        saveFile.variables.Clear();
    }

    /// <summary>
    /// Saves the current save file
    /// <param name="saveName">The save's name</param>
    /// </summary>
    public void Save(string saveName = "save")
    {
        // Setup variables in the savefile
        saveFile.variables.Clear();
        foreach (GAMEFILE.VARIABLE variable in items)
        {
            if (!variable.defaultValue.Equals(variable.value))
            {
                saveFile.variables.Add(variable);
            }
        }

        FileManager.SaveJSON(savePath + saveName + ".txt", saveFile);
    }

    /// <summary>
    /// Loads the save file from disk
    /// </summary>
    /// <param name="saveName">The save's name</param>
    /// <returns>The new save file</returns>
    public GAMEFILE Load(string saveName)
    {
        if (SaveFileExists(saveName))
        {
            saveFile = FileManager.LoadJSON<GAMEFILE>(savePath + saveName + ".txt");
            foreach (GAMEFILE.VARIABLE item in items)
            {
                GAMEFILE.VARIABLE inSave = saveFile.variables.Find(x => x.name == item.name);
                item.value = inSave == null ? item.defaultValue : inSave.value;
            }
        }

        return saveFile;
    }

    /// <summary>
    /// Finds the index of an item
    /// </summary>
    /// <param name="key">The item's key</param>
    /// <returns>The item's index</returns>
    private int GetIndexOfItem(string key)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == key)
            {
                return i;
            }
        }
        Debug.LogWarning($"Variable {key} not found");
        return 0;
    }

    /// <summary>
    /// Edits an item's value
    /// </summary>
    /// <param name="key">The item's key</param>
    /// <param name="value">The item's new value</param>
    public void EditVariable(string key, string value)
    {
        int index = GetIndexOfItem(key);

        if (value.Equals("++"))
        {
            value = (int.Parse(items[index].value) + 1).ToString();
        }
        else if (value.Equals("--"))
        {
            value = (int.Parse(items[index].value) - 1).ToString();
        }

        Debug.Log(key + " at index " + index + " (" + items[index].value + ") -> (" + value + ")");
        items[index].value = value;
    }

    /// <summary>
    /// Returns the value of an item
    /// </summary>
    /// <param name="key">The item's key</param>
    /// <returns>The item's value</returns>
    public string GetItem(string key)
    {
        return items[GetIndexOfItem(key)].value;
    }

    /// <summary>
    /// Checks if the specified log has been unlocked
    /// </summary>
    /// <param name="logId">The log's ID</param>
    /// <returns>True if the log is unlocked</returns>
    public bool HasUnlockedLog(string logId)
    {
        return saveFile.unlockedLogs.Contains(logId);
    }

    /// <summary>
    /// Unlocks a new log
    /// </summary>
    /// <param name="logId">The log's ID</param>
    public void UnlockLog(string logId)
    {
        if (!HasUnlockedLog(logId))
            saveFile.unlockedLogs.Add(logId);
    }

    public SaveInfo[] GetAvailableSaves()
    {
        GAMEFILE temp;
        string[] toTest = { "auto", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        SaveInfo[] list = new SaveInfo[toTest.Length];

        for (int i = 0; i < toTest.Length; i++)
        {
            if (SaveFileExists(toTest[i]))
            {
                temp = FileManager.LoadJSON<GAMEFILE>(savePath + toTest[i] + ".txt");

                GAMEFILE.VARIABLE item = temp.variables.Find(item => item.name.Equals("playerName"));
                string playerName = item != null ? item.value : items.Find(item => item.name.Equals("playerName")).defaultValue;
                list[i] = new SaveInfo
                {
                    slot = toTest[i],
                    playerName = playerName,
                    playerLevel = temp.rpgCharacters[0].level,
                    location = temp.locationName,
                };
            }
        }

        return list;
    }

    public class SaveInfo
    {
        public string slot;
        public string playerName;
        public int playerLevel;
        public string location;
    }
}
