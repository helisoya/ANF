using ANF.Utils;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// ANF Components contains a variety of utility functions and are Jsonable
	/// </summary>
    public interface ANFComponent : Jsonable
    {
        /// <summary>
		/// On Initialize callback.
        /// Called when the component is created
		/// </summary>
        public abstract void OnInitialize();

        /// <summary>
        /// On Update callback.
        /// Called once per frame
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
		/// On Start callback.
        /// Called when all components are created
		/// </summary>
        public abstract void OnStart();
    }
}

