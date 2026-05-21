using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the game's settings
    /// </summary>
    [System.Serializable]
    public class SettingsContainer : DataContainer
    {
        [SerializeReference, SubclassSelector(AllowNull = false)] private SettingsTab[] tabs;

        public DataContainer CloneContainer()
        {
            return new SettingsContainer()
            {
                tabs = tabs,
            };
        }

        /// <summary>
        /// Gets the settings tabs
        /// </summary>
        /// <returns>The tabs</returns>
        public SettingsTab[] GetTabs()
        {
            return tabs;
        }

        public void Initialize(ANFSettings settings)
        {
            foreach (SettingsTab tab in tabs)
            {
                tab.Initialize();
            }
        }

        public void Reset()
        {
            foreach (SettingsTab tab in tabs)
            {
                tab.Reset();
            }
        }

        public void Save(JSON json)
        {
            foreach (SettingsTab tab in tabs)
            {
                JSON partJSON = new JSON();
                tab.Save(partJSON);

                if (partJSON.Keys.Length != 0 &&
                    !json.ContainsKey(tab.GetName()))
                    json.Add(tab.GetName(), partJSON);
            }
        }

        public void Load(JSON json)
        {
            foreach (SettingsTab tab in tabs)
            {
                if (json.ContainsKey(tab.GetName()))
                    tab.Load(json.GetJSON(tab.GetName()));
            }
        }
    }
}

