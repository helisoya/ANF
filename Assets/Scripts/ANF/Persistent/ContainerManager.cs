using ANF.Utils;
using System.Collections.Generic;

namespace ANF.Persistent
{

    /// <summary>
	/// Handles multiple containers
	/// </summary>
    public class ContainerManager : DataManager<DataContainer>
    {

        public ContainerManager(ComponentRegisterEntry<DataContainer>[] containers, ANFSettings settings)
        {
            this.components = new Dictionary<string, DataContainer>();

            foreach (ComponentRegisterEntry<DataContainer> entry in containers)
            {
                DataContainer copy = entry.data.CloneContainer();

                this.components.Add(entry.id, copy);
            }

            foreach (DataContainer container in components.Values)
            {
                container.Initialize(settings);
            }
        }

        /// <summary>
		/// Resets all components
		/// </summary>
        public void ResetAll()
        {
            foreach (DataContainer container in components.Values)
            {
                container.Reset();
            }
        }
    }
}
