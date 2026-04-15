using ANF.Utils;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// Represent a world component
    /// Ex : The ANSL manager, the Background, the Characters, ...
	/// </summary>
    public interface WorldComponent : Jsonable
    {
        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public abstract void Initialize(ANFManager manager);

        /// <summary>
        /// Clones the component
        /// </summary>
        /// <returns>The cloned component</returns>
        public abstract WorldComponent CloneComponent();

        /// <summary>
		/// Updates the component
		/// </summary>
        public abstract void Update();
    }
}
