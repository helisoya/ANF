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
    public class GUIManager : ANFComponentManager<GUIComponent>
    {
        private Transform uiRoot;
        private ANFManager manager;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="manager">The ANFManager</param>
        /// <param name="uiRoot">The UI's root</param>
        /// <param name="componentsToCopy">The registered UI components</param>
        public GUIManager(ANFManager manager, Transform uiRoot, GUIRegisterEntry<GUIComponent>[] componentsToCopy)
        {
            this.uiRoot = uiRoot;
            this.manager = manager;

            components = new Dictionary<string, GUIComponent>();
            foreach (GUIRegisterEntry<GUIComponent> entry in componentsToCopy)
            {
                GUIComponent instance = Object.Instantiate(entry.data, uiRoot);
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                instance.name = entry.id;
                instance.Initialize(manager, this);
                components.Add(entry.id, instance);
            }
        }

        /// <summary>
        /// Changes if all components are enabled or not
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        public void SetComponentsEnabled(bool enabled)
        {
            foreach (GUIComponent component in components.Values)
                component.SetIsEnabled(enabled);
        }

        /// <summary>
        /// Changes if all components are enabled or not
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        public void SetComponentsEnabled(string[] componentIds, bool enabled)
        {
            GUIComponent tmp;
            foreach (string componentId in componentIds)
            {
                if (components.TryGetValue(componentId, out tmp))
                    tmp.SetIsEnabled(enabled);
            }
        }

        /// <summary>
        /// Checks if any of the specified components are open
        /// </summary>
        /// <param name="components">The components to check</param>
        /// <returns>True if any of the specified components is open</returns>
        public bool AnyComponentsActive(string[] componentIds)
        {
            GUIComponent tmp;
            foreach(string componentId in componentIds)
            {
                if(components.TryGetValue(componentId, out tmp))
                    if(tmp.isOpen)
                        return true;
            }
            return false;
        }

        public override void OnUpdate()
        {
            foreach (GUIComponent component in components.Values)
                if(component.isEnabled && component.isOpen)
                    component.OnUpdate();
        }
    }
}

