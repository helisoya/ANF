using ANF.Utils;
using System.Collections.Generic;
using System.IO;
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
        public bool Compile(string sourceFile, string destinationFile, Dictionary<string,ANSLFunction> functions, List<ANSLUtils.ANSLError> errors)
        {
            this.errors = errors;
            this.functions = functions;
            currentLine = -1;

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
            ContinueCompiling();

            return true;
        }

        /// <summary>
        /// Continues the compiling process
        /// </summary>
        private void ContinueCompiling()
        {
            if(currentLine >= inLines.Length)
            {
                // End of File
                Clean();
            }
            else
            {
                if (!string.IsNullOrEmpty(cachedCurrentLineClean) && !string.IsNullOrWhiteSpace(cachedCurrentLineClean))
                {
                    foreach (string body in functions.Keys)
                    {
                        if (string.IsNullOrEmpty(body)) // Skip undefined functions
                            continue;

                        if (cachedCurrentLineClean.StartsWith(body))
                        {
                            // Compile with this function
                            outStream.WriteLine(cachedCurrentLineClean);
                        }
                    }
                }

                CheckNextLine();
                ContinueCompiling();
            }
        }

        /// <summary>
        /// Checks the next line and caches a version without 
        /// </summary>
        public void CheckNextLine()
        {
            currentLine++;
            if(currentLine >= inLines.Length)
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
        /// Cleans the compiler
        /// </summary>
        public void Clean()
        {
            if(outStream != null)
            {
                outStream.Close();
                outStream = null;
            }

            inLines = null;
        }
    }
}

