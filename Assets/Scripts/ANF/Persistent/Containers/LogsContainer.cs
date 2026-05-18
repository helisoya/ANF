using Leguar.TotalJSON;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the logs (in game definitions for the user to read, not the history of dialogs)
    /// </summary>
    [System.Serializable]
    public class LogsContainer : DataContainer
    {
        [Header("Logs")]
        [SerializeField] private string pathToImagesInResources;
        private List<string> allLogs;
        private bool[] knownLogs;

        public DataContainer CloneContainer()
        {
            return new LogsContainer()
            {
                pathToImagesInResources = pathToImagesInResources
            };
        }

        public void Initialize(ANFSettings settings)
        {
            allLogs = new List<string>();
            LoadAllLogs(settings);
            knownLogs = new bool[allLogs.Count];
            Reset();
        }

        /// <summary>
        /// Unlock a log if possible
        /// </summary>
        /// <param name="log">The log to unlock</param>
        public void UnlockLog(string log)
        {
            int idx = allLogs.IndexOf(log);
            if (idx != -1)
                knownLogs[idx] = true;
        }

        /// <summary>
        /// Gets a log's sprite
        /// </summary>
        /// <param name="logID">The log's ID</param>
        /// <returns>The log's sprite if found</returns>
        public Sprite GetLogSprite(string logID)
        {
            return Resources.Load<Sprite>(pathToImagesInResources + logID);
        }

        /// <summary>
        /// Loads the known logs
        /// <paramref name="settings"/>The ANF Settings</param>
        /// </summary>
        private void LoadAllLogs(ANFSettings settings)
        {
            allLogs = new List<string>();

            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "logs")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    allLogs.Add(line);
                }
            }
        }

        public void Reset()
        {
            for(int i = 0; i < knownLogs.Length;i++)
                knownLogs[i] = false;
        }

        public void Save(JSON json)
        {
            JSON array = new JSON();
            for(int i = 0; i < allLogs.Count;i++)
                array.Add(allLogs[i], knownLogs[i]);

            json.Add("knownLogs", array);
        }

        public void Load(JSON json)
        {
            if(json.ContainsKey("knownLogs"))
            {
                JSON array = json.GetJSON("knownLogs");
                Reset();

                for (int i = 0; i < allLogs.Count; i++)
                    if (json.ContainsKey(allLogs[i]))
                        knownLogs[i] = json.GetBool(allLogs[i]);
            }
        }
    }
}

