using ANF.ANSL;
using ANF.Manager;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

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
        List<Type> functions = GetFunctionsList();

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
    /// Find the 
    /// </summary>
    /// <returns></returns>
    private List<Type> GetFunctionsList()
    {
        List<Type> output = new List<Type>();

        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (System.Reflection.Assembly assembly in assemblies)
        {
            Type[] assemblyTypes = assembly.GetTypes();

            foreach (Type type in assemblyTypes)
            {
                if (type.IsDefined(typeof(ANSLFunctionAttribute), false) && type.IsSubclassOf(typeof(ANSLFunction)))
                    output.Add(type);
            }
        }
        return output;
    }


    // Update is called once per frame
    void Update()
    {
        foreach (ANSLContext context in contexts)
        {
            if (context.isRunning)
                context.Update();
        }
    }
}
