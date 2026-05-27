using ANF.Utils;
using Leguar.TotalJSON;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents a settings container.
    /// Settings contained here are usually settings for the world & gui components.
    /// (For instance, this does not contain screen & language settings)
    /// </summary>
    [System.Serializable]
    public class SettingsContainer : DataContainer
    {
        public enum SettingsDataType
        {
            Bool,
            String,
            Int,
            UInt,
            Float,
            Vector2,
            Vector3,
            Vector4,
            Color
        }

        private Dictionary<string, SettingsObjectData> savedObjects;
        public DataContainer CloneContainer()
        {
            return new SettingsContainer();
        }

        public void Initialize(ANFSettings settings)
        {
            savedObjects = new Dictionary<string, SettingsObjectData>();
        }

        /// <summary>
        /// Registers a callback to a setting's value and gets the current value.
        /// The value will be created if not found.
        /// Keep to simple types (bool, int, Vector2, Vector3, ...)
        /// </summary>
        /// <param name="key">The value's key</param>
        /// <param name="defaultValue">The default value if the key wasn't found</param>
        /// <param name="type">The value's type</param>
        /// <param name="drawParameters">The draw parameters for this value (if created)</param>
        /// <param name="onValueChange">The callback when a value changed</param>
        /// <returns>The current value</returns>
        public object RegisterOrCreate(string key, object defaultValue, SettingsDataType type
        , SettingsObjectDrawParameters drawParameters, UnityAction<object> onValueChange)
        {
            if (!TypeGood(defaultValue, type))
                return default;

            if (!savedObjects.TryGetValue(key, out SettingsObjectData value))
            {
                value = new SettingsObjectData();
                value.onValueChange = new UnityEvent<object>();
                value.value = defaultValue;
                value.defaultValue = defaultValue;
                value.type = type;
                value.drawParameters = drawParameters;
                savedObjects.Add(key, value);
            }
            else if (!TypeGood(value.value, type))
            {
                return default;
            }

            value.onValueChange.AddListener(onValueChange);
            return value.value;
        }

        /// <summary>
		/// Gets a value from the settings
		/// </summary>
		/// <param name="key">The value's key</param>
		/// <param name="type">The value's type</param>
		/// <returns>The value if found (or valid with the type provided)</returns>
        public object GetValue(string key, SettingsDataType type)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData value) &&
                TypeGood(value.value, type))
            {
                return value.value;
            }

            return default;
        }

        /// <summary>
		/// Changes a setting's value
		/// </summary>
		/// <param name="key">The value's key</param>
		/// <param name="type">The value's type</param>
		/// <param name="value">The new value</param>
        public void SetValue(string key, SettingsDataType type, object value)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData obj) &&
                TypeGood(obj.value, type) && TypeGood(value, type))
            {
                obj.SetValue(value);
            }
        }

        /// <summary>
        /// Unregisters a callback for a settings' value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="onValueChange">The callback to remove</param>
        public void Unregister(string key, UnityAction<object> onValueChange)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData value))
            {
                value.onValueChange.RemoveListener(onValueChange);
            }
        }

        /// <summary>
		/// Checks if a value is of a specific serialized type
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="type">The supposed type</param>
		/// <returns>True if the value is of the correct type</returns>
        private bool TypeGood(object value, SettingsDataType type)
        {
            switch (type)
            {
                case SettingsDataType.Bool:
                    return value is bool;
                case SettingsDataType.String:
                    return value is string;
                case SettingsDataType.Int:
                    return value is int;
                case SettingsDataType.UInt:
                    return value is uint;
                case SettingsDataType.Float:
                    return value is float;
                case SettingsDataType.Color:
                    return value is Color;
                case SettingsDataType.Vector2:
                    return value is Vector2;
                case SettingsDataType.Vector3:
                    return value is Vector3;
                case SettingsDataType.Vector4:
                    return value is Vector4;
            }

            return false;
        }

        /// <summary>
		/// Gets the known values
		/// </summary>
		/// <returns>The known values</returns>
        public Dictionary<string, SettingsObjectData> GetValues()
        {
            return savedObjects;
        }

        public void Reset()
        {
            foreach (SettingsObjectData data in savedObjects.Values)
                data.ResetValue();
        }

        public void Load(JSON json)
        {
            Reset();
            foreach (string key in json.Keys)
            {
                JSON valueJSON = json.GetJSON(key);
                if (valueJSON.ContainsKey("type") && valueJSON.ContainsKey("value") && valueJSON.ContainsKey("drawParameters"))
                {
                    SettingsObjectData value = new SettingsObjectData();
                    value.Load(valueJSON);
                    savedObjects.Add(key, value);
                }

            }
        }

        public void Save(JSON json)
        {
            foreach (string key in savedObjects.Keys)
            {
                JSON valueJSON = new JSON();
                savedObjects[key].Save(valueJSON);
                json.Add(key, valueJSON);
            }
        }

        /// <summary>
        /// Represents a settings object's data
        /// </summary>
        public class SettingsObjectData : Jsonable
        {
            public UnityEvent<object> onValueChange;
            public SettingsDataType type;
            public object value;
            public object defaultValue;
            public SettingsObjectDrawParameters drawParameters;

            public void SetValue(object newValue)
            {
                value = newValue;
                onValueChange.Invoke(newValue);
            }

            /// <summary>
            /// Resets the object's value
            /// </summary>
            public void ResetValue()
            {
                value = defaultValue;
                onValueChange.Invoke(value);
            }

            public void Save(JSON json)
            {
                json.Add("value", value);
                json.Add("defaultValue", defaultValue);
                json.Add("type", (int)type);
                JSON drawJSON = new JSON();
                drawJSON.Add("label", drawParameters.label);
                drawJSON.Add("sliderMinValue", drawParameters.sliderMinValue);
                drawJSON.Add("sliderMaxValue", drawParameters.sliderMaxValue);
                if (drawParameters.dropdownLabels != null)
                    drawJSON.Add("dropdownLabels", drawParameters.dropdownLabels.ToArray());
                json.Add("drawParameters", drawJSON);
            }

            public void Load(JSON json)
            {
                if (!json.ContainsKey("type") || !json.ContainsKey("value") || !json.ContainsKey("drawParameters") || !json.ContainsKey("defaultValue"))
                    return;

                onValueChange = new UnityEvent<object>();
                type = (SettingsDataType)json.GetInt("type");
                switch (type)
                {
                    case SettingsDataType.Bool:
                        {
                            defaultValue = json.GetBool("defaultValue");
                            value = json.GetBool("value");
                            break;
                        }
                    case SettingsDataType.String:
                        {
                            defaultValue = json.GetString("defaultValue");
                            value = json.GetString("value");
                            break;
                        }
                    case SettingsDataType.Int:
                        {
                            defaultValue = json.GetInt("defaultValue");
                            value = json.GetInt("value");
                            break;
                        }
                    case SettingsDataType.UInt:
                        {
                            defaultValue = json.GetJNumber("defaultValue").AsUInt();
                            value = json.GetJNumber("value").AsUInt();
                            break;
                        }
                    case SettingsDataType.Float:
                        {
                            defaultValue = json.GetFloat("defaultValue");
                            value = json.GetFloat("value");
                            break;
                        }
                    case SettingsDataType.Color:
                        {
                            defaultValue = json.GetJArray("defaultValue").AsColor();
                            value = json.GetJArray("value").AsColor();
                            break;
                        }
                    case SettingsDataType.Vector2:
                        {
                            defaultValue = json.GetJArray("defaultValue").AsVector2();
                            value = json.GetJArray("value").AsVector2();
                            break;
                        }
                    case SettingsDataType.Vector3:
                        {
                            defaultValue = json.GetJArray("defaultValue").AsVector3();
                            value = json.GetJArray("value").AsVector3();
                            break;
                        }
                    case SettingsDataType.Vector4:
                        {
                            defaultValue = json.GetJArray("defaultValue").AsVector4();
                            value = json.GetJArray("value").AsVector4();
                            break;
                        }
                }

                JSON drawJSON = json.GetJSON("drawParameters");

                drawParameters = new SettingsObjectDrawParameters();
                if (drawJSON.ContainsKey("label"))
                    drawParameters.label = drawJSON.GetString("label");
                if (drawJSON.ContainsKey("sliderMinValue"))
                    drawParameters.sliderMinValue = drawJSON.GetFloat("sliderMinValue");
                if (drawJSON.ContainsKey("sliderMaxValue"))
                    drawParameters.sliderMaxValue = drawJSON.GetFloat("sliderMaxValue");
                if (drawJSON.ContainsKey("dropdownLabels"))
                    drawParameters.dropdownLabels = drawJSON.GetJArray("dropdownLabels").AsStringArray();
            }
        }

        /// <summary>
		/// Represents a setting's object draw parameters
		/// </summary>
        public struct SettingsObjectDrawParameters
        {
            public string label;
            public string[] dropdownLabels;
            public float sliderMinValue;
            public float sliderMaxValue;
            public SettingsObjectDrawParameters(string label, string[] dropdownLabels = null,
                float sliderMinValue = 0, float sliderMaxValue = 1)
            {
                this.label = label;
                this.dropdownLabels = dropdownLabels;
                this.sliderMaxValue = sliderMaxValue;
                this.sliderMinValue = sliderMinValue;
            }
        }
    }
}
