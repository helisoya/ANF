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

        public void OnUpdate()
        {
            foreach (T component in components.Values)
                component.OnUpdate();
        }

        public void OnStart()
        {
            foreach (T component in components.Values)
                component.OnStart();
        }
    }
}
