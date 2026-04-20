using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using NUnit.Framework;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a GUI Component
    /// </summary>
    [System.Serializable]
    public abstract class GUIComponent : MonoBehaviour, Jsonable
    {
        [Header("General")]
        [SerializeField] protected GameObject root;
        [SerializeField] protected bool openByDefault = true;

        protected ANFManager manager;
        protected GUIManager gui;
        public bool isOpen { get { return root.activeInHierarchy; } }

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager, GUIManager gui)
        {
            this.manager = manager;
            this.gui = gui;
            OnInitialize();

            root.SetActive(!openByDefault);


            if (openByDefault)
                Open();
            else
                Close();
        }

        /// <summary>
		/// Opens the GUI component
		/// </summary>
        public void Open()
        {
            if (!isOpen)
            {
                root.SetActive(true);
                OnOpen();
            }
        }

        /// <summary>
        /// Closes the GUI component
        /// </summary>
        public void Close()
        {
            if (isOpen)
            {
                root.SetActive(false);
                OnClose();
            }
        }

        /// <summary>
		/// Updates the GUI Component
		/// </summary>
        public abstract void UpdateComponent();

        /// <summary>
		/// On Initialize Callback
		/// </summary>
        protected abstract void OnInitialize();

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
            json.Add("isOpen", isOpen);
            OnSave(json);
        }

        public void Load(JSON json)
        {
            OnLoad(json);

            if (json.ContainsKey("isOpen"))
            {
                bool open = json.GetBool("isOpen");

                root.SetActive(open);
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
    }
}
