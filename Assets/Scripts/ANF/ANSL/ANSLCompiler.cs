using ANF.Utils;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static ANF.Utils.ANSLUtils;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

namespace ANF.ANSL
{
    /// <summary>
    /// Handles the compiling of an ANSL file
    /// </summary>
    public class ANSLCompiler
    {
        private string sourceFilepath;
        private bool isfirstLine;

        private int currentLineInOutput;
        private int currentLine;
        private string cachedCurrentLine;
        private string cachedCurrentLineClean;

        private StreamWriter outStream;
        private string[] inLines;
        private Dictionary<string, ANSLFunction> functions;
        private List<ANSLUtils.ANSLError> errors;


        /// <summary>
        /// Starts compiling a new ANSL file
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="functions">The function list</param>
        /// <param name="errors">The global error list</param>
        public bool Compile(string sourceFile, string destinationFile, Dictionary<string, ANSLFunction> functions, List<ANSLUtils.ANSLError> errors)
        {
            sourceFilepath = sourceFile;
            this.errors = errors;
            this.functions = functions;
            currentLine = -1;
            currentLineInOutput = 0;
            isfirstLine = true;

            inLines = File.ReadAllLines(sourceFile);

            if (inLines == null)
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.ERROR,
                    filePath = sourceFile,
                    line = 0,
                    errorMessage = $"Could not open file."
                });
                return false;
            }

            new FileInfo(destinationFile).Directory.Create();

            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            outStream = new StreamWriter(destinationFile, false);

            CheckNextLine();

            return ContinueCompiling();
        }

        /// <summary>
        /// Continues the compiling process
        /// </summary>
		/// <returns>True if the compiling failed</returns>
        private bool ContinueCompiling()
        {
            if (currentLine >= inLines.Length)
            {
                // End of File
                Clean();
                return true;
            }
            else
            {
                if (CompileLine(cachedCurrentLineClean, out List<string> compiledLines, currentLineInOutput))
                {
                    foreach (string line in compiledLines)
                    {
                        if (isfirstLine)
                            isfirstLine = false;
                        else
                            outStream.Write("\n");

                        outStream.Write(line);
                    }
                    currentLineInOutput += compiledLines.Count;
                }
                else
                {
                    Clean();
                    return false;
                }

                CheckNextLine();
                return ContinueCompiling();
            }
        }

        /// <summary>
		/// Compiles a specific line
		/// </summary>
		/// <param name="line">The line counter</param>
		/// <param name="compiledLines">The compiled lines of code</param>
        /// <param name="outputLine">The outline line the function will start in</param>
		/// <returns>True if the compiling resulted in success</returns>
        public bool CompileLine(string line, out List<string> compiledLines, int outputLine)
        {
            compiledLines = new List<string>();
            if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
            {
                bool found = false;
                foreach (string body in functions.Keys)
                {
                    if (string.IsNullOrEmpty(body)) // Skip undefined functions
                        continue;

                    if (line.StartsWith(body))
                    {
                        found = true;

                        // Compile with this function
                        if (functions[body].Compile(out List<string> result, line, this, errors, outputLine))
                        {
                            foreach (string compiledLine in result)
                            {
                                compiledLines.Add(compiledLine);
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (!found)
                {
                    errors.Add(new ANSLError()
                    {
                        type = ANSLErrorType.WARNING,
                        filePath = sourceFilepath,
                        line = currentLine,
                        errorMessage = $"Unknown function : {cachedCurrentLineClean}."
                    });
                }
            }
            return true;
        }

        /// <summary>
        /// Checks the next line and caches a version without 
        /// </summary>
        public void CheckNextLine()
        {
            currentLine++;
            if (currentLine >= inLines.Length)
            {
                // End of file
                cachedCurrentLine = null;
                cachedCurrentLineClean = null;
            }
            else
            {
                cachedCurrentLine = inLines[currentLine];
                cachedCurrentLineClean = cachedCurrentLine.Replace("\t", "");
                while (cachedCurrentLineClean.StartsWith(" ") && cachedCurrentLineClean.Length > 0)
                    cachedCurrentLine = cachedCurrentLineClean.Substring(1);
            }
        }

        /// <summary>
        /// Removes the first character from the cleaned line
        /// </summary>
        public void RemoveFirstCharacterFromCleanedLine()
        {
            cachedCurrentLineClean = cachedCurrentLineClean.Substring(1);
        }

        /// <summary>
        /// Gets the current line counter in the output file
        /// </summary>
        /// <returns>The line counter</returns>
        public int GetCurrentOutputLineCounter()
        {
            return currentLineInOutput;
        }

        /// <summary>
		/// Returns the cached current line
		/// </summary>
		/// <returns>The current line</returns>
        public string GetCurrentLine()
        {
            return cachedCurrentLine;
        }

        /// <summary>
		/// Returns the cleaned cached current line (Without tabs and white spaces at the start)
		/// </summary>
		/// <returns>The cleaned cached current line</returns>
        public string GetCurrentLineClean()
        {
            return cachedCurrentLineClean;
        }

        /// <summary>
		/// Gets the source filepath
		/// </summary>
		/// <returns>The source filepath</returns>
        public string GetSourceFilepath()
        {
            return sourceFilepath;
        }

        /// <summary>
		/// Gets the current line counter
		/// </summary>
		/// <returns>The current line counter</returns>
        public int GetCurrentLineCounter()
        {
            return currentLine;
        }

        /// <summary>
        /// Cleans the compiler
        /// </summary>
        public void Clean()
        {
            if (outStream != null)
            {
                outStream.Close();
                outStream = null;
            }

            inLines = null;
        }
    }
}

