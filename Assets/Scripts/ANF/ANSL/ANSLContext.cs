using ANF.ANSL;
using ANF.Persistent;
using ANF.Utils;
using ANF.World;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the flow of a specific script.
/// A context can change scripts dynamically without having to recreate a new one
/// </summary>
public class ANSLContext
{
    public bool isRunning { get; private set; }

    private Dictionary<uint, ANSLFunction> functions;
    private List<ANSLContextStackEntry> scriptStack;

    private uint contextStackLength;
    private uint maxFunctionsPerFrame;

    private string[] currentScript;
    private uint currentLine;
    private string currentFilePath;

    private bool lastFunctionModifiedLine;
    private bool waitingForFunction;
    private uint currentFunctionId;

    private uint currentFunctionDepth;
    private bool waitingForNextFrame;

    private ANFManager manager;


    public ANSLContext(Dictionary<uint, ANSLFunction> functions, uint contextStackLength, uint maxFunctionsPerFrame, ANFManager manager)
    {
        this.contextStackLength = contextStackLength;
        this.maxFunctionsPerFrame = maxFunctionsPerFrame;
        this.functions = functions;
        this.manager = manager;
        isRunning = false;
        scriptStack = new List<ANSLContextStackEntry>();
    }

    /// <summary>
    /// Loads a new script file
    /// </summary>
    /// <param name="scriptFilePath">The script's filepath (Local path inside the designated resource folder) </param>
    /// <param name="startLine">The starting line. 0 by default.</param>
    /// <param name="startImmediately">True if the script should start immediatly. False will wait for the next Update</param>
    public void LoadScript(string scriptFilePath, uint startLine = 0, bool startImmediately = true)
    {
        string fullPath = PersistentDataManager.instance.GetANFSettings().anslDestinationFolder + scriptFilePath;
        if (System.IO.File.Exists("Assets/Resources/" + fullPath + ".txt"))
        {
            // Add current script to stack if it isn't finished
            if (currentScript != null && currentLine < currentScript.Length - 1)
            {
                while (scriptStack.Count >= contextStackLength) // Clear excess entries
                {
                    scriptStack.RemoveAt(0);
                }
                scriptStack.Add(new ANSLContextStackEntry() { filePath = currentFilePath, lineCounter = currentLine + 1 });
            }

            currentFilePath = scriptFilePath;
            currentScript = FileManager.ReadTextAsset(Resources.Load<TextAsset>(fullPath)).ToArray();
            lastFunctionModifiedLine = true;
            currentLine = startLine;

            if (!waitingForNextFrame)
                waitingForNextFrame = !startImmediately;

            isRunning = true;

            NextLine();
        }
        else
        {
            // No file, do something ?
            StopContext();
        }
    }

    /// <summary>
    /// Process the next line
    /// </summary>
    public void NextLine()
    {
        currentFunctionDepth++;
        if (waitingForNextFrame || currentFunctionDepth > maxFunctionsPerFrame)
        {
            waitingForNextFrame = true;
            return;
        }


        if (!lastFunctionModifiedLine)
            currentLine++;

        waitingForFunction = false;
        lastFunctionModifiedLine = false;

        if (currentLine < currentScript.Length)
        {
            string[] split = currentScript[currentLine].Split('|', 2);
            uint functionId;

            if (split.Length == 0 || string.IsNullOrEmpty(currentScript[currentLine]) ||
                !uint.TryParse(split[0], out functionId) || !functions.ContainsKey(functionId))
            {
                // Could not parse/find function
                NextLine();
            }
            else
            {
                FunctionParameters parameters = ANSLUtils.CreateParametersInterface(split.Length > 1 ? split[1].Split('|') : null, functions[functionId].GetParametersTemplates());

                if (parameters == null)
                {
                    // Parameters couldn't be parsed
                    NextLine();
                    return;
                }

                // Start the new function
                functions[functionId].StartProcess(parameters, this, manager);

                if (functions[functionId].isProcessing)
                {
                    // Needs to wait until the function is finished
                    currentFunctionId = functionId;
                    waitingForFunction = true;
                }
                else
                {
                    // Function is already over
                    NextLine();
                }
            }
        }
        else
        {
            // No more lines in script
            StopContext();
        }
    }

    /// <summary>
    /// Updates the context
    /// </summary>
    public void Update()
    {
        // If the context is in cooldown, 
        if (waitingForNextFrame)
        {
            currentFunctionDepth = 0;
            waitingForNextFrame = false;
            NextLine();
        }

        // Updates the current function if it isn't finished yet
        if (waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            functions[currentFunctionId].Update();
            if (!functions[currentFunctionId].isProcessing)
                NextLine();
        }
    }

    /// <summary>
    /// Changes the current line counter.
    /// This will disable the auto increment at the end of the next function
    /// </summary>
    /// <param name="value">The new line counter</param>
    public void SetLineCounter(uint value)
    {
        currentLine = value;
        lastFunctionModifiedLine = true;
    }

    /// <summary>
	/// Clears the script stack
	/// </summary>
    public void ClearStack()
    {
        scriptStack.Clear();
    }

    /// <summary>
    /// Stops the context
    /// </summary>
    public void StopContext()
    {
        if (scriptStack.Count == 0)
        {
            isRunning = false;
            currentScript = null;
        }
        else
        {
            ANSLContextStackEntry entry = scriptStack[scriptStack.Count - 1];
            scriptStack.RemoveAt(scriptStack.Count - 1);
            LoadScript(entry.filePath, entry.lineCounter, true);
        }

    }

    /// <summary>
	/// Represents an entry in the context's stack
	/// </summary>
    public struct ANSLContextStackEntry
    {
        public string filePath;
        public uint lineCounter;
    }
}
