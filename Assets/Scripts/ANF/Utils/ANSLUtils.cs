using ANF.ANSL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
    /// Contains various utilitary functions
    /// </summary>
    public class ANSLUtils
    {

        /// <summary>
        /// Find all ANSL Functions
        /// </summary>
        /// <returns>All ANSL Functions</returns>
        public static List<Type> GetANSLFunctionsList()
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

        /// <summary>
        /// Finds the correct template for the specified parameters and create an interface for it
        /// Returns null if none found
        /// </summary>
        /// <param name="parameters">The parameters list</param>
        /// <param name="templates">The templates list</param>
        /// <returns>The interfaced template</returns>
        public static FunctionParameters CreateParametersInterface(string[] parameters, FunctionParameterType[][] templates)
        {
            FunctionParameters parameterInterface = new FunctionParameters();
            for(uint i = 0; i < templates.Length;i++)
            {
                parameterInterface.Clear();
                parameterInterface.Initialize(parameters, templates[i],i);
                if (parameterInterface.IsValid())
                    return parameterInterface;
            }

            return null;
        }
    }
}

