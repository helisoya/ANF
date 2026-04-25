using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a GUI Component
    /// A component can be opened or closed, which will be saved.
    /// It can also be enabled and disabled, which will restrict some of its functions, but will not be saved.
    /// You may use isEnabled to disable components when in a pause menu, without compromising a potential save
    /// </summary>
    [System.Serializable]
    public abstract class GUIComponent : MonoBehaviour, ANFComponent
    {
        [Header("General")]
        [SerializeField] protected GameObject root;
        [SerializeField] protected bool canBeSaved = true;
        [SerializeField] protected bool hideRootWhenClosed = true;
        [SerializeField] protected bool openByDefault = true;
        [Tooltip("A component may be opened only if all its lock components are closed. For instance, the load menu cannot be opened if the map menu is open")]
        [SerializeField] protected string[] lockComponents;

        protected ANFManager manager;
        protected GUIManager gui;
        public bool isOpen { get; private set; }
        public bool isEnabled { get; private set; }

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager, GUIManager gui)
        {
            this.manager = manager;
            this.gui = gui;
            isOpen = false;
            isEnabled = false;
            OnInitialize();

            root.SetActive(!hideRootWhenClosed);

            if (openByDefault)
                Open();
        }

        /// <summary>
        /// Changes if the component is enabled (can be updated).
        /// A visible component can be disabled, it won't be updated with OnUpdate
        /// </summary>
        /// <param name="enabled"></param>
        public void SetIsEnabled(bool enabled)
        {
            bool newValue = isOpen && enabled;

            if (newValue != isEnabled)
            {
                isEnabled = isOpen && enabled;
                if (newValue)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        /// <summary>
		/// Callback for when a component is enabled
		/// </summary>
        protected abstract void OnEnabled();

        /// <summary>
		/// Callback for when a component is disabled
		/// </summary>
        protected abstract void OnDisabled();

        /// <summary>
		/// Opens the GUI component
		/// </summary>
        public void Open()
        {
            if (gui.AnyComponentsActive(lockComponents))
                return;

            if (!isOpen)
            {
                isOpen = true;
                isEnabled = true;
                root.SetActive(true);
                OnOpen();
                OnEnabled();
            }
        }

        /// <summary>
        /// Closes the GUI component
        /// </summary>
        public void Close()
        {
            if (isOpen)
            {
                isOpen = false;
                isEnabled = false;
                root.SetActive(!hideRootWhenClosed);
                OnClose();
                OnDisabled();
            }
        }

        /// <summary>
		/// On Open callback
		/// </summary>
        protected abstract void OnOpen();

        /// <summary>
		/// On close callback
		/// </summary>
        protected abstract void OnClose();

        public void Save(JSON json)
        {
            if (!canBeSaved)
                return;

            json.Add("isOpen", isOpen);
            OnSave(json);
        }

        public void Load(JSON json)
        {
            if (!canBeSaved)
                return;

            OnLoad(json);

            if (json.ContainsKey("isOpen"))
            {

                bool open = json.GetBool("isOpen");

                root.SetActive(false);
                if (open)
                    OnOpen();
                else
                    OnClose();
            }
        }

        /// <summary>
		/// On Save callback
		/// </summary>
		/// <param name="json">The JSON to save to</param>
        protected abstract void OnSave(JSON json);

        /// <summary>
		/// On Load Callback
		/// </summary>
		/// <param name="json">The JSON to load from</param>
        protected abstract void OnLoad(JSON json);

        public abstract void OnInitialize();
        public abstract void OnUpdate();
        public abstract void OnStart();
    }
}
