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
    public class ANFManager : MonoBehaviour, Jsonable
    {
        [Header("General")]
        [SerializeField] private Transform uiRoot;
        private World world;
        private GUIManager guiManager;

        /// <summary>
		/// Gets the GUI Manager
		/// </summary>
		/// <returns>The GUI Manager</returns>
        public GUIManager GetGUIManager()
        {
            return guiManager;
        }

        /// <summary>
		/// Gets the world
		/// </summary>
		/// <returns>The world</returns>
        public World GetWorld()
        {
            return world;
        }

        void Update()
        {
            world.OnUpdate();
            guiManager.OnUpdate();
        }

        void Start()
        {
            InitializeComponents();
            OnStartComponents();
        }

        /// <summary>
		/// Calls the On Start callback on all components
		/// </summary>
        private void OnStartComponents()
        {
            world.OnStart();
            guiManager.OnStart();
        }

        /// <summary>
        /// Initialize the various components (GUI & World Components)
        /// </summary>
        private void InitializeComponents()
        {
            guiManager = new GUIManager(this, uiRoot, PersistentDataManager.instance.GetANFSettings().registeredGUIComponents);
            world = new World(this, PersistentDataManager.instance.GetANFSettings().registeredWorldComponents);
        }

        public void Save(JSON json)
        {
            JSON individualDataJson = new JSON();
            world.Save(individualDataJson);
            json.Add("world", individualDataJson);

            individualDataJson = new JSON();
            guiManager.Save(individualDataJson);
            json.Add("gui", individualDataJson);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("gui"))
                guiManager.Load(json.GetJSON("gui"));

            if (json.ContainsKey("world"))
                world.Load(json.GetJSON("world"));
        }
    }
}
