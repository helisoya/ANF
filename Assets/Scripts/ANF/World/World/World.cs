using ANF.Utils;
using ANF.World;
using UnityEngine;

namespace ANF.World
{
    /// <summary>
	/// Represents the 3D world and handles its components
	/// </summary>
    public class World : ANFComponentManager<WorldComponent>
    {
        protected ANFManager manager;

        public World(ANFManager manager, ComponentRegisterEntry<WorldComponent>[] registeredComponents)
        {
            this.manager = manager;

            components = new System.Collections.Generic.Dictionary<string, WorldComponent>();
            foreach (ComponentRegisterEntry<WorldComponent> entry in registeredComponents)
            {
                WorldComponent copy = entry.data.CloneComponent();
                copy.Initialize(manager);
                components.Add(entry.id, copy);
            }
        }
    }

}
