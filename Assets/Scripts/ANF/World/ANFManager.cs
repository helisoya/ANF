using UnityEngine;
using ANF.ANSL;
using ANF.Persistent;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace ANF.World
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour
    {
        private WorldComponent[] worldComponents;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled = false;
        [SerializeField] private string debugScriptToLoad;


        /// <summary>
		/// Gets a world component
		/// </summary>
		/// <typeparam name="T">The type to search</typeparam>
		/// <param name="result">The component if found</param>
		/// <returns>True if the component was found</returns>
        public bool GetWorldComponent<T>(out T result)
        {
            foreach (WorldComponent component in worldComponents)
            {
                if (component.GetType() == typeof(T) || component.GetType().IsSubclassOf(typeof(T)))
                {
                    result = (T)component;
                    return true;
                }
            }
            result = default;
            return false;
        }

        void Update()
        {
            foreach (WorldComponent component in worldComponents)
            {
                component.Update();
            }
        }

        void Start()
        {
            WorldComponent[] componentsToCopy = PersistentDataManager.instance.GetANFSettings().registeredWorldComponents;
            worldComponents = new WorldComponent[componentsToCopy.Length];
            for (int i = 0; i < worldComponents.Length; i++)
            {
                worldComponents[i] = componentsToCopy[i].GetType().Instantiate() as WorldComponent;
                worldComponents[i].Initialize(this);
            }

            if (debugEnabled && GetWorldComponent(out ANSLManager anslManager))
                anslManager.StartNewContext(debugScriptToLoad);
        }
    }
}
