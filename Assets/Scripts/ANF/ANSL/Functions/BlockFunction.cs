using ANF.Persistent;
using ANF.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// The if function is used to check variables.
    /// If the check is successful, the true functions will be loaded. If false, the else statement will be loaded
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 11,
        functionBody: "block",
        functionAutoComplete: new string[] { "block()\n\nendblock" },
        functionDesc: "All functions inside a block will be processed during the same frame, ignoring any depth check.")]
    public class BlockFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ }
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            if (cleanedLine.Length != "block".Length)
            {
                // Unknown character
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown character at the end of the line : {cleanedLine}."
                });
                return false;
            }

            List<string> compiledTrue = new List<string>();
            List<string> compiledFalse = new List<string>();
            bool compilingTrues = true;
            bool canContinue = true;
            bool foundEnd = false;
            compiler.CheckNextLine();
            string currentNextLine = compiler.GetCurrentLineClean();

            while (canContinue && currentNextLine != null)
            {
                if (string.IsNullOrEmpty(currentNextLine) || string.IsNullOrWhiteSpace(currentNextLine))
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                    continue;
                }

                if (currentNextLine.StartsWith("else"))
                {
                    if(!compilingTrues)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Two else detected : {currentNextLine}."
                        });
                        return false;
                    }

                    if (currentNextLine.Length != "else".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the else : {currentNextLine}."
                        });
                        return false;
                    }

                    compilingTrues = false;
                }
                else if (currentNextLine.StartsWith("endif"))
                {
                    if (currentNextLine.Length != "endif".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the endif : {currentNextLine}."
                        });
                        return false;
                    }

                    foundEnd = true;
                    canContinue = false;
                }
                else
                {
                    int outputLineForFunction;
                    if (compilingTrues)
                        outputLineForFunction = outputLine + compiledTrue.Count + 1;
                    else
                        outputLineForFunction = outputLine + compiledTrue.Count + 1 + compiledFalse.Count + 1;

                    // Potential function
                    if (compiler.CompileLine(currentNextLine, out List<string> compiled, outputLineForFunction))
                    {
                        if (compilingTrues)
                            compiledTrue.AddRange(compiled);
                        else
                            compiledFalse.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (canContinue)
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                }
            }

            if(!foundEnd)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"endif not found : {currentNextLine}."
                });
                return false;
            }

            int startTrue = outputLine + 1;
            int startFalse = startTrue + compiledTrue.Count + 1;
            int endIndex = startFalse + compiledFalse.Count + 1;

            uint idFunction = GetAttribute().functionId;

            compiledLines.Add($"{idFunction}|{startTrue}|{startFalse}|{ifContent}");
            compiledLines.AddRange(compiledTrue);
            compiledLines.Add($"0|{endIndex}");
            compiledLines.AddRange(compiledFalse);
            compiledLines.Add($"0|{endIndex}");

            return true;
        }

        protected override void OnStartProcess()
        {

            EndProcess();
        }

        protected override void OnUpdate()
        {
            // Unused
        }

        protected override void OnCleanup()
        {
            // Unused
        }
    }
}