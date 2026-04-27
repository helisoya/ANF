using ANF.Utils;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// Handles multiple ANFComponents
	/// </summary>
    public abstract class ANFComponentManager<T> : DataManager<T> where T : ANFComponent
    {
        public void OnInitialize()
        {
            foreach (T component in components.Values)
                component.OnInitialize();
        }

        public virtual void OnUpdate()
        {
            foreach (T component in components.Values)
                component.OnUpdate();
        }

        public void OnStart()
        {
            foreach (T component in components.Values)
                component.OnStart();
        }
        
        /// <summary>
        /// Changes if all components are enabled or not
        /// </summary>
        /// <param name="enabled">True if all components should be enabled</param>
        public void SetEnabledAll(bool enabled)
        {
            foreach (T component in components.Values)
                component.SetEnabled(enabled);
        }

        /// <summary>
        /// Changes if all components are paused or not
        /// </summary>
        /// <param name="paused">True if all components should be paused</param>
        public void SetPausedAll(bool paused)
        {
            foreach (T component in components.Values)
                component.SetPaused(paused);
        }

    }
}
