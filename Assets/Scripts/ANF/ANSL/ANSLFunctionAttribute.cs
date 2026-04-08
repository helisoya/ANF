using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// Use this attribute to mark an ANSL Function
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ANSLFunctionAttribute : System.Attribute
    {
        public uint functionId { get; private set; }
        public string functionBody { get; private set; }
        public string functionAutoComplete { get; private set; }
        public string functionDesc { get; private set; }

        /// <summary>
        /// Marks the class as an ANSL Function
        /// </summary>
        /// <param name="functionId">The Function's ID must be unique</param>
        /// <param name="functionBody">The body of the function is the part before any (). Ex : setBackground is the body of setBackground(BCKGND)</param>
        /// <param name="functionAutoComplete">The function's script template (For VS Code autocomplete feature). Ex : setBackground(BACKGROUND)</param>
        /// <param name="functionDesc">The function's script description (For VS Code autocomplete feature)</param>
        public ANSLFunctionAttribute(uint functionId = 0, string functionBody = "", string functionAutoComplete = "", string functionDesc = "")
        {
            this.functionId = functionId;
            this.functionBody = functionBody;
            this.functionAutoComplete = functionAutoComplete;
            this.functionDesc = functionDesc;
        }
    }
}
