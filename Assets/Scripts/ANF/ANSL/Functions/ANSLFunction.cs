using ANF.Manager;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// Represents an ANSL Function
    /// Ex:
    /// setBackground(Mountain)
    /// </summary>
    public abstract class ANSLFunction
    {
        public bool isProcessing { get; private set; }

        protected ANSLContext context;
        protected ANFManager manager;
        protected FunctionParameters parameters;

        public ANSLFunction()
        {
            isProcessing = false;
        }

        public abstract bool Compile();

        /// <summary>
        /// Generates the parameters templates for this function.
        /// You can have multiple templates for one function.
        /// Ex : setPosition(MARKER), setPosition(X;Y;Z) 
        /// </summary>
        /// <returns>The templates</returns>
        public abstract FunctionParameterType[][] GetParametersTemplates();

        /// <summary>
        /// Starts the function's process
        /// </summary>
        /// <param name="parameters">The function's parameters</param>
        /// <param name="context">The ANSL Context</param>
        /// <param name="manager">The ANF Manager</param>
        public void StartProcess(FunctionParameters parameters, ANSLContext context, ANFManager manager)
        {
            isProcessing = true;
            this.context = context;
            this.manager = manager;
            this.parameters = parameters;
            OnStartProcess();
        }

        /// <summary>
        /// Marks the function as ended.
        /// Script flow will then resume
        /// </summary>
        protected void EndProcess()
        {
            isProcessing = false;
            OnCleanup();
        }

        /// <summary>
        /// Updates the function
        /// </summary>
        public void Update()
        {
            if (isProcessing)
            {
                OnUpdate();
            }
        }

        /// <summary>
        /// Called when updating the function
        /// Use this is the function does things in more than one frame
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// Called when the function starts processing.
        /// If the function doesn't last over time (Ex: Edit a variable), call EndProcess() here
        /// </summary>
        protected abstract void OnStartProcess();

        /// <summary>
        /// Called when ending the function's processing.
        /// Clean things here if needs be
        /// </summary>
        protected abstract void OnCleanup();
    }
}

