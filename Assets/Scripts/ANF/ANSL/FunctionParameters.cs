using Unity.VisualScripting;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// Represents an ANSL function parameter
    /// </summary>
    public class FunctionParameters
    {
        private string[] parameters;
        private FunctionParameterType[] template;
        private bool isValid;
        private uint id;

        public void Initialize(string[] parameters, FunctionParameterType[] template, uint id)
        {
            this.parameters = parameters;
            this.template = template;
            this.id = id;
            isValid = CheckIfValid();
        }

        /// <summary>
        /// Gets the template Id
        /// </summary>
        /// <returns>The template Id</returns>
        public uint GetTemplateId()
        {
            return id; 
        }

        /// <summary>
        /// Gets the template
        /// </summary>
        /// <returns>The template</returns>
        public FunctionParameterType[] GetTemplate()
        {
            return template;
        }

        /// <summary>
        /// Gets a parameter's type
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <returns>The index</returns>
        public FunctionParameterType GetParameterType(int parameterIndex)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < template.Length)
            {
                return template[parameterIndex];
            }
            return FunctionParameterType.UNKNOWN;
        }

        /// <summary>
        /// Gets if the interface is valid or not
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            return isValid;
        }

        /// <summary>
        /// Gets an int parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out int value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length 
                && template[parameterIndex] == FunctionParameterType.INT)
            {
                if (int.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a uint parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out uint value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.UINT)
            {
                if (uint.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a bool parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out bool value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.BOOL)
            {
                if (bool.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = false;
            return false;
        }

        /// <summary>
        /// Gets a float parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out float value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.FLOAT)
            {
                if (float.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a string parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out string value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.STRING)
            {
                value = parameters[parameterIndex];
                return true;
            }

            value = "";
            return false;
        }

        /// <summary>
        /// Checks if the inteface is valid or not
        /// </summary>
        /// <returns>True if valid</returns>
        private bool CheckIfValid()
        {
            if (template == null || parameters == null)
                return false;

            if (parameters.Length != template.Length)
                return false;

            for(int i = 0; i < parameters.Length; i++)
            {
                bool valid = false;
                switch(template[i])
                {
                    case FunctionParameterType.INT:
                        valid = int.TryParse(parameters[i], out int _);
                        break;
                    case FunctionParameterType.UINT:
                        valid = uint.TryParse(parameters[i], out uint _);
                        break;
                    case FunctionParameterType.FLOAT:
                        valid = float.TryParse(parameters[i], out float _);
                        break;
                    case FunctionParameterType.BOOL:
                        valid = bool.TryParse(parameters[i], out bool _);
                        break;
                }

                if (!valid)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clears the interface
        /// </summary>
        public void Clear()
        {
            parameters = null;
            template = null;
            isValid = false;
        }

    }

    /// <summary>
    /// Base types for enums
    /// </summary>
    public enum FunctionParameterType
    {
        UNKNOWN,
        INT,
        UINT,
        FLOAT,
        BOOL,
        STRING
    }
}

