using ANF.ANSL;
using ANF.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
    /// Contains various utilitary functions
    /// </summary>
    public class ANSLUtils
    {
        #region General

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

        #endregion

        #region Compilation

        /// <summary>
        /// Represents an error when compiling ANSL files
        /// </summary>
        public struct ANSLError
        {
            public ANSLErrorType type;
            public string filePath;
            public uint line;
            public string errorMessage;
        }

        /// <summary>
        /// Error types for ANSL
        /// </summary>
        public enum ANSLErrorType
        {
            WARNING,
            ERROR,
            FUNCTION
        }

        /// <summary>
        /// Compiles all ANSL Files
        /// </summary>
        /// <returns>The error list</returns>
        public static List<ANSLError> CompileAll(ANFSettings settings)
        {
            List<ANSLError> errors = new List<ANSLError>();
            List<Type> functions = GetANSLFunctionsList();
            ANSLCompiler compiler = new ANSLCompiler();

            if (CheckANSLFunctions(functions, errors))
            {
                Dictionary<string,ANSLFunction> functionInstances = new Dictionary<string,ANSLFunction>();
                foreach (Type type in functions)
                {
                    ANSLFunctionAttribute attribute = type.GetAttribute<ANSLFunctionAttribute>();
                    if(attribute != null)
                    {
                        functionInstances.Add(attribute.functionBody, (ANSLFunction)type.Instantiate());
                    }
                }

                Stack<string> directories = new Stack<string>();
                directories.Push(settings.anslSourceFolder);

                while(directories.Count > 0)
                {
                    string directory = directories.Pop();

                    foreach (string subDir in Directory.GetDirectories(directory))
                        directories.Push(subDir);

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".ansl"))
                        {
                            string destPath = "Assets/Resources/" + settings.anslDestinationFolder + file.Substring(settings.anslSourceFolder.Length).Replace(".ansl", ".txt");
                            compiler.Compile(file, destPath, functionInstances, errors);
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Checks the ANSL Functions for errors
        /// </summary>
        /// <param name="types">The functions list</param>
        /// <param name="errors">The global error list</param>
        /// <returns>True if no errors were found</returns>
        private static bool CheckANSLFunctions(List<Type> functions, List<ANSLError> errors)
        {
            List<uint> usedIds = new List<uint>();

            bool errorFound = false;

            foreach (Type function in functions)
            {
                ANSLFunctionAttribute attribute = function.GetAttribute<ANSLFunctionAttribute>(false);
                if (attribute != null)
                {
                    if (usedIds.Contains(attribute.functionId))
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.FUNCTION,
                            filePath = function.Name,
                            errorMessage = $"Id {attribute.functionId} is already used by another function."
                        });
                        errorFound = true;
                    }
                    else
                    {
                        usedIds.Add(attribute.functionId);
                    }
                }
                else
                {
                    errors.Add(new ANSLError()
                    {
                        type = ANSLErrorType.FUNCTION,
                        filePath = function.Name,
                        errorMessage = $"Failed to retrieve {function.Name}'s class Attribute."
                    });
                    errorFound = true;
                }
            }
            return !errorFound;
        }
        #endregion
    }
}

