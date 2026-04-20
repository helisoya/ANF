using System.Collections.Generic;
using ANF.Persistent;
using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Handles the ANF GUI
    /// </summary>
    public class GUIManager : MonoBehaviour, Jsonable
    {
        [SerializeField] private Transform uiRoot;

        private ANFManager manager;
        private Dictionary<string, GUIComponent> guiComponents;

        /// <summary>
		/// Gets a gui component
		/// </summary>
		/// <typeparam name="T">The type to search</typeparam>
		/// <param name="result">The component if found</param>
		/// <returns>True if the component was found</returns>
        public bool GetGUIComponent<T>(out T result) where T : GUIComponent
        {
            foreach (GUIComponent component in guiComponents.Values)
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
        /// Gets a gui component
        /// </summary>
        /// <typeparam name="id">The component's id</typeparam>
        /// <typeparam name="T">The type to search</typeparam>
        /// <param name="result">The component if found</param>
        /// <returns>True if the component was found</returns>
        public bool GetGUIComponent<T>(string id, out T result) where T : GUIComponent
        {
            if (guiComponents.TryGetValue(id, out GUIComponent component))
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

        private void Update()
        {
            foreach (GUIComponent component in guiComponents.Values)
            {
                if (component.isOpen)
                    component.UpdateComponent();
            }
        }

        public void Initialize(ANFManager manager)
        {
            this.manager = manager;

            GUIRegisterEntry<GUIComponent>[] componentsToCopy = PersistentDataManager.instance.GetANFSettings().registeredGUIComponents;
            guiComponents = new Dictionary<string, GUIComponent>();
            foreach (GUIRegisterEntry<GUIComponent> entry in componentsToCopy)
            {
                GUIComponent instance = Instantiate(entry.data, uiRoot);
                instance.name = entry.id;
                instance.Initialize(manager, this);
                guiComponents.Add(entry.id, instance);
            }
        }

        public void Save(JSON json)
        {
            JSON individualDataJson;

            foreach (string containerId in guiComponents.Keys)
            {
                individualDataJson = new JSON();
                guiComponents[containerId].Save(individualDataJson);
                json.Add(containerId, individualDataJson);
            }
        }

        public void Load(JSON json)
        {
            foreach (string key in guiComponents.Keys)
                if (json.ContainsKey(key))
                    guiComponents[key].Load(json.GetJSON(key));
        }
    }
}

