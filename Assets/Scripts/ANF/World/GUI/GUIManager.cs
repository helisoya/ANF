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
                instance.name = entry.id;
                instance.Initialize(manager, this);
                components.Add(entry.id, instance);
            }
        }
    }
}

