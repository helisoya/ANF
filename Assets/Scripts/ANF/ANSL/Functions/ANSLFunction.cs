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

        public ANSLFunction()
        {
            isProcessing = false;
        }

        public abstract bool Compile();

        /// <summary>
        /// Starts the function's process
        /// </summary>
        /// <param name="parameters">The function's parameters</param>
        /// <param name="context">The ANSL Context</param>
        public void StartProcess(string[] parameters, ANSLContext context)
        {
            isProcessing = true;
            OnStartProcess(parameters, context);
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
            if(isProcessing)
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
        /// <param name="parameters">The function's parameters</param>
        /// <param name="context">The ANSL Context</param>
        protected abstract void OnStartProcess(string[] parameters, ANSLContext context);

        /// <summary>
        /// Called when ending the function's processing.
        /// Clean things here if needs be
        /// </summary>
        protected abstract void OnCleanup();
    }
}

