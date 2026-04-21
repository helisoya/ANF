using UnityEngine;
using ANF.ANSL;
using ANF.Persistent;
using System.Collections.Generic;
using Unity.VisualScripting;
using ANF.Utils;
using Leguar.TotalJSON;
using ANF.GUI;

namespace ANF.World
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    [RequireComponent(typeof(GUIManager))]
    public class ANFManager : MonoBehaviour, Jsonable
    {
        private Dictionary<string, WorldComponent> worldComponents;
        private GUIManager guiManager;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled = false;
        [SerializeField] private string debugScriptToLoad;

        /// <summary>
		/// Gets the GUI Manager
		/// </summary>
		/// <returns>The GUI Manager</returns>
        public GUIManager GetGUIManager()
        {
            return guiManager;
        }

        /// <summary>
        /// Gets a world component
        /// </summary>
        /// <typeparam name="T">The type to search</typeparam>
        /// <param name="result">The component if found</param>
        /// <returns>True if the component was found</returns>
        public bool GetWorldComponent<T>(out T result) where T : WorldComponent
        {
            foreach (WorldComponent component in worldComponents.Values)
            {
                if (component.GetType() == typeof(T) || component.GetType().IsSubclassOf(typeof(T)))
                {
                    result = (T)component;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a world component
        /// </summary>
        /// <typeparam name="id">The component's id</typeparam>
        /// <typeparam name="T">The type to search</typeparam>
        /// <param name="result">The component if found</param>
        /// <returns>True if the component was found</returns>
        public bool GetWorldComponent<T>(string id, out T result) where T : WorldComponent
        {
            if (worldComponents.TryGetValue(id, out WorldComponent component))
            {

                if (component.GetType() == typeof(T) || component.GetType().IsSubclassOf(typeof(T)))
                {
                    result = (T)component;
                    return true;
                }
            }

            result = default;
            return false;
        }

        void Update()
        {
            foreach (WorldComponent component in worldComponents.Values)
            {
                if (component.isEnabled)
                    component.Update();
            }

            guiManager.UpdateManager();
        }

        void Start()
        {
            InitializeComponents();

            if (debugEnabled && GetWorldComponent(out ANSLManager anslManager))
                anslManager.StartNewContext(debugScriptToLoad);
        }

        /// <summary>
        /// Initialize the various components (GUI & World Components)
        /// </summary>
        private void InitializeComponents()
        {
            guiManager = GetComponent<GUIManager>();
            guiManager.Initialize(this);


            ComponentRegisterEntry<WorldComponent>[] componentsToCopy = PersistentDataManager.instance.GetANFSettings().registeredWorldComponents;
            worldComponents = new Dictionary<string, WorldComponent>();
            foreach (ComponentRegisterEntry<WorldComponent> entry in componentsToCopy)
            {
                WorldComponent copy = entry.data.CloneComponent();
                copy.Initialize(this);
                worldComponents.Add(entry.id, copy);
            }
        }

        public void Save(JSON json)
        {
            JSON individualDataJson;

            foreach (string containerId in worldComponents.Keys)
            {
                individualDataJson = new JSON();
                worldComponents[containerId].Save(individualDataJson);
                json.Add(containerId, individualDataJson);
            }

            individualDataJson = new JSON();
            guiManager.Save(individualDataJson);
            json.Add("guiManager", individualDataJson);
        }

        public void Load(JSON json)
        {
            foreach (string key in worldComponents.Keys)
                if (json.ContainsKey(key))
                    worldComponents[key].Load(json.GetJSON(key));

            if (json.ContainsKey("guiManager"))
                guiManager.Load(json.GetJSON("guiManager"));
        }
    }
}
