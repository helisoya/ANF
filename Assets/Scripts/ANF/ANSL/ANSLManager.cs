using ANF.Manager;
using ANF.Utils;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// Handles the different ANSL Contexts
    /// </summary>
    public class ANSLManager : MonoBehaviour
    {
        private ANSLContext[] contexts;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager)
        {
            GenerateContexts(manager);
        }

        /// <summary>
        /// Generate the context pool
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        private void GenerateContexts(ANFManager manager)
        {
            List<Type> functions = ANSLUtils.GetANSLFunctionsList();

            contexts = new ANSLContext[PersistentDataManager.instance.GetANFSettings().anslMaxContexts];
            for (int i = 0; i < contexts.Length; i++)
            {
                Dictionary<uint, ANSLFunction> instances = new Dictionary<uint, ANSLFunction>();
                foreach (Type type in functions)
                {
                    ANSLFunctionAttribute attribute = type.GetAttribute<ANSLFunctionAttribute>();
                    if (attribute != null && !instances.ContainsKey(attribute.functionId))
                    {
                        instances.Add(attribute.functionId, (ANSLFunction)type.Instantiate());
                    }
                }

                contexts[i] = new ANSLContext(instances, manager);
            }
        }

        /// <summary>
        /// Gets a specific context
        /// </summary>
        /// <param name="contextId">The context's Id</param>
        /// <returns>The context if found</returns>
        public ANSLContext GetContext(int contextId)
        {
            if (contextId < 0 || contextId > contexts.Length)
                return null;

            return contexts[contextId];
        }

        /// <summary>
        /// Starts a new context
        /// </summary>
        /// <param name="scriptPath">The script's filepath</param>
        /// <param name="startImmediately">True if the script should be started this frame</param>
        /// <returns>The new context's Id. -1 if no context was avaiable</returns>
        public int StartNewContext(string scriptPath, bool startImmediately)
        {
            for (int i = 0; i < contexts.Length; i++)
            {
                if (!contexts[i].isRunning)
                {
                    contexts[i].LoadScript(scriptPath, startImmediately);
                    return i;
                }
            }
            return -1;
        }

        void Update()
        {
            foreach (ANSLContext context in contexts)
            {
                if (context.isRunning)
                    context.Update();
            }
        }
    }
}