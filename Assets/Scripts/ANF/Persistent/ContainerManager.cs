using ANF.Utils;
using Leguar.TotalJSON;
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
                copy.Initialize(settings);

                this.components.Add(entry.id, copy);
            }
        }
    }
}
