using ANF.Persistent;
using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// Handles the different ANSL Contexts
    /// </summary>
    [System.Serializable]
    public class ANSLManager : WorldComponent
    {
        private ANSLContext[] contexts;

        [Tooltip("How many ANSL function can be called per frame (per context)")]
        [SerializeField] private uint maxFunctionsPerFrame = 10;
        [Tooltip("How many concurrent contexts can coexist")]
        [SerializeField] private uint maxContexts = 20;
        [Tooltip("How large the script stack can be (per context)")]
        [SerializeField] private uint contextStackLength = 10;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        protected override void OnInitialize()
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

            contexts = new ANSLContext[maxContexts];
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

                contexts[i] = new ANSLContext(instances, contextStackLength, maxFunctionsPerFrame, manager);
            }
        }

        /// <summary>
        /// Gets a specific context
        /// </summary>
        /// <param name="contextId">The context's Id</param>
        /// <returns>The context if found</returns>
        public ANSLContext GetContext(uint contextId)
        {
            if (contextId < 0 || contextId >= contexts.Length)
                return null;

            return contexts[contextId];
        }

        /// <summary>
        /// Starts a new context
        /// </summary>
        /// <param name="scriptPath">The script's filepath</param>
        /// <param name="startLine">The starting line counter. 0 by default</param>
        /// <param name="startImmediately">True if the script should be started this frame</param>
        /// <returns>The new context's Id. -1 if no context was avaiable</returns>
        public int StartNewContext(string scriptPath, uint startLine = 0, bool startImmediately = true)
        {
            for (int i = 0; i < contexts.Length; i++)
            {
                if (!contexts[i].isRunning)
                {
                    contexts[i].LoadScript(scriptPath, startLine, startImmediately);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Pauses all contexts
        /// </summary>
        /// <param name="paused">True if the contexts are now paused</param>
        public void PauseAllContexts(bool paused)
        {
            foreach(ANSLContext context in contexts)
            {
                context.PauseContext(paused);
            }
        }

        /// <summary>
        /// Pauses a specific context
        /// </summary>
        /// <param name="contextId">The context's Id</param>
        /// <param name="paused">True if the context should be paused</param>
        public void PauseContext(uint contextId,bool paused)
        {
            if (contextId >= 0 && contextId < contexts.Length)
                contexts[contextId].PauseContext(paused);
        }

        public override WorldComponent CloneComponent()
        {
            return new ANSLManager()
            {
                manager = manager,
                maxFunctionsPerFrame = maxFunctionsPerFrame,
                maxContexts = maxContexts,
                contextStackLength = contextStackLength
            };
        }

        public override void Update()
        {
            foreach (ANSLContext context in contexts)
            {
                if (context.isRunning)
                    context.Update();
            }
        }

        public override string GetJSONName()
        {
            return "anslManager";
        }

        public override void Save(JSON json)
        {
            JArray contextsArray = new JArray();
            JSON contextJSON;
            foreach (ANSLContext context in contexts)
            {
                contextJSON = new JSON();
                context.Save(contextJSON);
                contextsArray.Add(contextJSON);
            }

            json.Add("contexts", contextsArray);
        }

        public override void Load(JSON json)
        {
            if (json.ContainsKey("contexts"))
            {
                JArray contextsArray = json.GetJArray("contexts");
                JSON contextJSON;
                for (int i = 0; i < contextsArray.Length; i++)
                {
                    if (i >= contexts.Length)
                        continue;

                    if (contextsArray.Values[i] is JSON)
                    {
                        contextJSON = contextsArray.Values[i] as JSON;
                        contexts[i].Load(contextJSON);
                    }
                }
            }
        }
    }
}