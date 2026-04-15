using System;
using System.Collections;
using System.Collections.Generic;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents the game's variable and the player name
    /// Variables are represented as integers
    /// </summary>
    [System.Serializable]
    public class PlayerVariableContainer : DataContainer
    {
        private string playerName;
        private List<Variable> variables;

        [Tooltip("Name of the random variable, automatically generated")]
        [SerializeField] private string randomVariableName;

        public string GetJSONName()
        {
            return "playerVariables";
        }

        public DataContainer CloneContainer()
        {
            return new PlayerVariableContainer() { randomVariableName = this.randomVariableName};
        }

        public void Initialize(ANFSettings settings)
        {
            GenerateVariablesList(settings.generalDataPath + "variables");
            Reset();
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("playerName"))
            {
                playerName = json.GetString("playerName");
            }
            if (json.ContainsKey("variables"))
            {
                JArray array = json.GetJArray("variables");
                JSON tmpVar;
                foreach (JValue variable in array.Values)
                {
                    if (variable is JSON)
                    {
                        tmpVar = variable as JSON;
                        SetVariable(tmpVar.GetString("name"), tmpVar.GetInt("value"));
                    }
                }
            }
        }

        public void Reset()
        {
            playerName = "Man";
            foreach (Variable variable in variables)
            {
                variable.value = variable.defaultValue;
            }
        }

        public void Save(JSON json)
        {
            json.Add("playerName", playerName);

            JArray variableArray = new JArray();
            JSON variableNode;

            foreach (Variable variable in variables)
            {
                variableNode = new JSON();
                variableNode.Add("name", variable.name);
                variableNode.Add("value", variable.value);
                variableArray.Add(variableNode);
            }
            json.Add("variables", variableArray);
        }

        /// <summary>
		/// Gets the player's name
		/// </summary>
		/// <returns>The player's name</returns>
        public string GetPlayerName()
        {
            return playerName;
        }

        /// <summary>
		/// Changes the player's name
		/// </summary>
		/// <param name="playerName">The new player's name</param>
        public void SetPlayerName(string playerName)
        {
            this.playerName = playerName;
        }

        /// <summary>
		/// Gets all variables
		/// </summary>
		/// <returns>All variables</returns>
        public List<Variable> GetAllVariables()
        {
            return variables;
        }

        /// <summary>
		/// Checks if the specified variable exists
		/// </summary>
		/// <param name="variableName">The variable's name</param>
		/// <returns>True if it exists</returns>
        public bool VariableExists(string variableName)
        {
            foreach (Variable variable in variables)
                if (variable.name.Equals(variableName))
                    return true;
            return false;
        }

        /// <summary>
		/// Sets a variable's value
		/// </summary>
		/// <param name="variableName">The variable's name</param>
		/// <param name="value">The variable's new value</param>
		/// <returns>True if the variable was found</returns>
        public bool SetVariable(string variableName, int value)
        {
            foreach (Variable variable in variables)
            {
                if (variable.name.Equals(variableName))
                {
                    variable.value = value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
		/// Resets the variable to its default value
		/// </summary>
		/// <param name="variableName">The variable's name</param>
		/// <returns>True if the variable was found</returns>
        public bool ResetVariable(string variableName)
        {
            foreach (Variable variable in variables)
            {
                if (variable.name.Equals(variableName))
                {
                    variable.value = variable.defaultValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
		/// Gets a variable's value
		/// </summary>
		/// <param name="variableName">The variable's name</param>
		/// <param name="value">The variable's value</param>
		/// <returns>True if the variable was found</returns>
        public bool GetVariable(string variableName, out int value)
        {
            foreach (Variable variable in variables)
            {
                if (variable.name.Equals(variableName))
                {
                    value = variable.value;
                    return true;
                }
            }

            value = -1;
            return false;
        }

        /// <summary>
		/// Gets the random variable's value
		/// </summary>
		/// <param name="randomValue">The random value</param>
		/// <returns>True if it was found</returns>
        public bool GetRandom(out int randomValue)
        {
            return GetVariable(randomVariableName, out randomValue);
        }

        /// <summary>
		/// Generates a random value between a min and a mex
		/// </summary>
		/// <param name="minInclusive">The min value (inclusive)</param>
		/// <param name="maxExlusive">The max value (exclusive)</param>
		/// <returns>True if the operation was a success</returns>
        public bool GenerateRandom(int minInclusive, int maxExlusive)
        {
            return SetVariable(randomVariableName, UnityEngine.Random.Range(minInclusive, maxExlusive));
        }

        /// <summary>
		/// Generates the variable's list
		/// </summary>
		/// <param name="filePath">The variable's filepath</param>
        private void GenerateVariablesList(string filePath)
        {
            variables = new List<Variable>();
            List<string> lines = FileManager.ReadTextAsset(Resources.Load<TextAsset>(filePath));
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    string[] split = line.Split(' ');
                    if (split.Length == 2 && int.TryParse(split[1], out int value))
                        variables.Add(new Variable(split[0], value));
                }
            }

            if (!VariableExists(randomVariableName))
                variables.Add(new Variable(randomVariableName, 0));
        }
    }

    /// <summary>
	/// Represents a user variable for ANSL
	/// </summary>
    public class Variable
    {
        public string name;
        public int value;
        public int defaultValue;

        public Variable(string key, int defaultValue)
        {
            this.name = key;
            this.value = defaultValue;
            this.defaultValue = defaultValue;
        }
    }
}

