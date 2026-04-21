using ANF.Persistent;
using ANF.World;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
    /// Contains various functions for saving and loading game states
    /// </summary>
    public class SaveUtils
    {

        /// <summary>
        /// Checks if a file exists on disk.
        /// The filepath should only be the part inside the save folder (Ex: Saves/global.json, which will translate to Assets/Saves/global.json in the editor)
        /// </summary>
        /// <param name="filepath">The filepath inside the save</param>
        /// <returns>True if the save file exists</returns>
        public static bool FileExists(string filepath)
        {
            return File.Exists(filepath);
        }

        /// <summary>
        /// Loads a JSON
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static JSON LoadJSON(string filepath)
        {
            if (!File.Exists(filepath))
                return null;

            List<string> lines = FileManager.LoadFile(filepath);
            string result = "";
            foreach (string line in lines)
                result += line;

            try
            {
                return JSON.ParseString(result);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the global data to disk
        /// </summary>
        /// <param name="globalData">The global data manager</param>
        /// <param name="savePath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool SaveGlobalData(DataManager globalData, string savePath)
        {
            JSON json = new JSON();

            globalData.Save(json);

            FileManager.SaveFile(savePath, json.CreateString());

            return true;
        }

        /// <summary>
        /// Loads the global data from disk
        /// </summary>
        /// <param name="globalData">The global data manager</param>
        /// <param name="loadPath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool LoadGlobalData(DataManager globalData, string loadPath)
        {
            JSON loadedJSON = LoadJSON(loadPath);
            if (loadedJSON == null)
                return false;

            globalData.Load(loadedJSON);

            return true;
        }


        /// <summary>
        /// Saves the player & world data to disk
        /// </summary>
        /// <param name="playerData">The player data</param>
        /// <param name="anfManager">The ANF Manager</param>
        /// <param name="savePath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool SavePlayerData(DataManager playerData, ANFManager anfManager, string savePath)
        {
            JSON json = new JSON();

            JSON partJSON = new JSON();
            playerData.Save(partJSON);
            json.Add("playerData", partJSON);

            partJSON = new JSON();
            anfManager.Save(partJSON);
            json.Add("worldData",partJSON);

            FileManager.SaveFile(savePath, json.CreateString());

            return true;
        }

        /// <summary>
        /// Loads the player & world data from disk
        /// </summary>
        /// <param name="playerData">The player data</param>
        /// <param name="anfManager">The ANF Manager</param>
        /// <param name="loadPath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool LoadPlayerData(DataManager playerData, ANFManager anfManager, string loadPath)
        {
            JSON loadedJSON = LoadJSON(loadPath);
            if (loadedJSON == null)
                return false;

            if (loadedJSON.ContainsKey("playerData"))
                playerData.Load(loadedJSON.GetJSON("playerData"));

            if (loadedJSON.ContainsKey("worldData"))
                anfManager.Load(loadedJSON.GetJSON("worldData"));

            return true;
        }
    }
}

