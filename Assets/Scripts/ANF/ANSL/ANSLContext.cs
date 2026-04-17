using ANF.ANSL;
using ANF.Persistent;
using ANF.Utils;
using ANF.World;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the flow of a specific script.
/// A context can change scripts dynamically without having to recreate a new one
/// </summary>
public class ANSLContext : Jsonable
{
    public bool isRunning { get; private set; }
    public bool isPaused { get; private set; }

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

    private ANFManager manager;

    public ANSLContext(Dictionary<uint, ANSLFunction> functions, uint contextStackLength, uint maxFunctionsPerFrame, ANFManager manager)
    {
        this.contextStackLength = contextStackLength;
        this.maxFunctionsPerFrame = maxFunctionsPerFrame;
        this.functions = functions;
        this.manager = manager;
        isRunning = false;
        isPaused = false;
        scriptStack = new List<ANSLContextStackEntry>();
    }

    /// <summary>
    /// Loads a new script file
    /// </summary>
    /// <param name="scriptFilePath">The script's filepath (Local path inside the designated resource folder) </param>
    /// <param name="startLine">The starting line. 0 by default.</param>
    /// <param name="canAddPreviousToStack">True if the previous script can be added to the stack</param>
    public void LoadScript(string scriptFilePath, uint startLine = 0, bool canAddPreviousToStack = true)
    {
        string fullPath = PersistentDataManager.instance.GetANFSettings().anslDestinationFolder + scriptFilePath;
        if (System.IO.File.Exists("Assets/Resources/" + fullPath + ".txt"))
        {
            // Add current script to stack if it isn't finished
            if (currentScript != null && canAddPreviousToStack)
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

            isRunning = true;
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
        if (isPaused)
            return;

        currentFunctionDepth++;
        if (currentFunctionDepth > maxFunctionsPerFrame)
            return;

        if (!lastFunctionModifiedLine)
            currentLine++;

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
                return;
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
                    return;
                }
            }
        }
        else
        {
            // No more lines in script

            if (scriptStack.Count == 0)
                StopContext();
            else
            {
                ANSLContextStackEntry entry = scriptStack[scriptStack.Count - 1];
                scriptStack.RemoveAt(scriptStack.Count - 1);
                LoadScript(entry.filePath, entry.lineCounter, false);
            }
        }
    }

    /// <summary>
    /// Updates the context
    /// </summary>
    public void Update()
    {
        if (isPaused)
            return;

        // Updates the current function if it isn't finished yet
        if (waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            functions[currentFunctionId].Update();
            if(!functions[currentFunctionId].isProcessing)
                waitingForFunction = false;
        }

        if(!waitingForFunction)
        {
            // Start next batch of lines 
            currentFunctionDepth = 0;
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
        isRunning = false;
        currentScript = null;
    }
    
    /// <summary>
    /// Pauses the context
    /// </summary>
    /// <param name="paused">True if paused</param>
    public void PauseContext(bool paused)
    {
        isPaused = paused;
    }

    public string GetJSONName()
    {
        return "anslContext";
    }

    public void Save(JSON json)
    {
        json.Add("isRunning", isRunning);
        json.Add("isPaused", isPaused);

        json.Add("currentLine", currentLine);
        json.Add("currentFilePath", currentFilePath);

        json.Add("lastFunctionModifiedLine", lastFunctionModifiedLine);
        json.Add("waitingForFunction", waitingForFunction);
        json.Add("currentFunctionId", currentFunctionId);
        json.Add("currentFunctionDepth", currentFunctionDepth);

        JArray stackArray = new JArray();
        JSON stackEntry;
        foreach (ANSLContextStackEntry entry in scriptStack)
        {
            stackEntry = new JSON();
            stackEntry.Add("filePath", entry.filePath);
            stackEntry.Add("lineCounter", entry.lineCounter);
        }
        json.Add("scriptStack", stackArray);

        if (isRunning && waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            JSON currentFunctionParameters = new JSON();
            functions[currentFunctionId].Save(currentFunctionParameters);
            json.Add("currentFunctionParameters", currentFunctionParameters);
        }
    }

    public void Load(JSON json)
    {
        scriptStack.Clear();
        currentScript = null;

        if (json.ContainsKey("isRunning"))
            isRunning = json.GetBool("isRunning");
        if (json.ContainsKey("isPaused"))
            isPaused = json.GetBool("isPaused");
        if (json.ContainsKey("lastFunctionModifiedLine"))
            lastFunctionModifiedLine = json.GetBool("lastFunctionModifiedLine");
        if (json.ContainsKey("waitingForFunction"))
            waitingForFunction = json.GetBool("waitingForFunction");
        if (json.ContainsKey("currentFunctionId"))
            currentFunctionId = json.GetJNumber("currentFunctionId").AsUInt();
        if (json.ContainsKey("currentFunctionDepth"))
            currentFunctionDepth = json.GetJNumber("currentFunctionDepth").AsUInt();

        string currentFilepath = null;
        uint currentLineIndex = 0;

        if (json.ContainsKey("currentLine"))
            currentLineIndex = json.GetJNumber("currentLine").AsUInt();
        if (json.ContainsKey("currentFilePath"))
            currentFilepath = json.GetString("currentFilePath");

        if (json.ContainsKey("scriptStack"))
        {
            JSON jsonEntry;
            JArray stack = json.GetJArray("scriptStack");
            foreach (JValue entry in stack.Values)
            {
                if (entry is JSON)
                {
                    jsonEntry = entry as JSON;
                    if (jsonEntry.ContainsKey("filePath") && jsonEntry.ContainsKey("lineCounter"))
                        scriptStack.Add(new ANSLContextStackEntry() { filePath = jsonEntry.GetString("filepath"), lineCounter = jsonEntry.GetJNumber("lineCounter").AsUInt() });
                }
            }
        }

        if (isRunning && currentScript != null)
        {
            currentLine = currentLineIndex;
            currentScript = FileManager.ReadTextAsset(Resources.Load<TextAsset>(currentFilepath)).ToArray();

            if (waitingForFunction && json.ContainsKey("currentFunctionParameters"))
            {
                JSON parameters = json.GetJSON("currentFunctionParameters");
                functions[currentFunctionId].Load(parameters);
            }
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
