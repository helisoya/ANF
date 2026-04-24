using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// Represent a world component
    /// Ex : The ANSL manager, the Background, the Characters, ...
	/// </summary>
    [System.Serializable]
    public abstract class WorldComponent : ANFComponent
    {
        protected ANFManager manager;
        public bool isEnabled { get; protected set; } = true;


        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager)
        {
            this.manager = manager;
            OnInitialize();
        }

        /// <summary>
        /// Changes if the component is enabled or not
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        public void ChangeIsEnabled(bool enabled)
        {
            if(isEnabled != enabled)
            {
                isEnabled = enabled;
                if (isEnabled)
                    OnEnabled();
                else
                    OnDisabled();
            }

        }

        /// <summary>
        /// Clones the component
        /// </summary>
        /// <returns>The cloned component</returns>
        public abstract WorldComponent CloneComponent();

        public abstract void OnInitialize();
        public abstract void OnUpdate();
        public abstract void OnStart();
        public abstract void OnEnabled();
        public abstract void OnDisabled();
        public abstract void Save(JSON json);
        public abstract void Load(JSON json);
    }
}
