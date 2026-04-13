using ANF.Manager;
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
        functionId: 8,
        functionBody: "if",
        functionAutoComplete: new string[] {"if()\n{\n\n}\nelse\n{\n\n}"},
        functionDesc: "Checks variables. You can use && and ||, as well as (). Ex : if((Var1 == Var2 && Var3 == 1) || Var3 == 0")]
    public class IfFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.UINT, FunctionParameterType.UINT, FunctionParameterType.STRING}
            };
        }

        public override bool Compile(out List<string> compiledLines, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string currentLine = compiler.GetCurrentLineClean();

            string ifContent = ExtractParenthesisContent(currentLine);

            if (ifContent == null)
            {
                // No if content
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No if content detected : {currentLine}."
                });
                return false;
            }

            if (!currentLine.EndsWith(')') && !currentLine.EndsWith('{'))
            {
                // Unknown character
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown character at the end of the line : {currentLine}."
                });
                return false;
            }

            if(!CheckIfContent(ifContent,out bool _, true))
            {
                // Check failed
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Could not parse if content : {currentLine}."
                });
                return false;
            }

            List<string> compiledTrue = new List<string>();
            List<string> compiledFalse = new List<string>();
            bool isMulti = currentLine.EndsWith('{');
            bool compilingTrues = true;
            bool canContinue = true;
            bool elseFound = false;
            compiler.CheckNextLine();
            string currentNextLine = compiler.GetCurrentLineClean();
            bool canTryCompile;
            bool mayCheckForLastFunction = false;

            while(canContinue && currentNextLine != null)
            {
                if (string.IsNullOrEmpty(currentNextLine) || string.IsNullOrWhiteSpace(currentNextLine))
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                }

                Debug.Log("--"+ currentNextLine + "--");

                canTryCompile = true;

                if (currentNextLine.StartsWith('{'))
                {
                    canTryCompile = false;
                    if ((compilingTrues && compiledTrue.Count != 0) ||
                        (!compilingTrues && compiledFalse.Count != 0))
                    {
                        // Already started as non multi
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Functions detected before this {{ : {currentNextLine}."
                        });
                        return false;
                    }
                    else if(isMulti)
                    {
                        // { was already put
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Too many {{ detected : {currentNextLine}."
                        });
                        return false;
                    }
                    else if(!compilingTrues && !elseFound)
                    {
                        // else is missing
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"else is missing : {currentNextLine}."
                        });
                        return false;
                    }
                    else
                    {
                        isMulti = true;
                        if(currentNextLine.Length > 1)
                        {
                            // Try parse a potential function on top of {
                            currentNextLine = currentNextLine.Substring(1);
                            canTryCompile = true;
                        }
                    }
                }
                
                if (currentNextLine.StartsWith('}'))
                {
                    canTryCompile = false;
                    if(!isMulti)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Too many }} detected : {currentNextLine}."
                        });
                        return false;
                    }
                    isMulti = false;
                    if (compilingTrues)
                        compilingTrues = false;
                    else
                        canContinue = false;

                    if(currentNextLine.Length > 1)
                    {
                        // Still things after the }
                        // Could be a else and/or a function

                        currentNextLine = currentNextLine.Substring(1);

                        if (!canContinue)
                            mayCheckForLastFunction = true;
                        else
                            canTryCompile = true;
                    }
                }

                if(currentNextLine.StartsWith("else"))
                {
                    canTryCompile = false;
                    if(elseFound)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Too many else detected : {currentNextLine}."
                        });
                        return false;
                    }

                    if(compilingTrues || !canContinue)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Else is not correctly placed : {currentNextLine}."
                        });
                        return false;
                    }

                    elseFound = true;

                    if(currentNextLine.Length > 4)
                    {
                        currentNextLine = currentNextLine.Substring(4);
                        continue;
                    }
                }
                
                if(canTryCompile)
                {
                    int outputLineForFunction;
                    if (compilingTrues)
                        outputLineForFunction = outputLine + compiledTrue.Count + 1;
                    else
                        outputLineForFunction = outputLine + compiledTrue.Count + 1 + compiledFalse.Count + 1;

                    while(currentNextLine.Length > 0 && currentNextLine[0] == ' ')
                        currentNextLine = currentNextLine.Substring(1);

                    // Potential function
                    if (compiler.CompileLine(currentNextLine, out List<string> compiled, outputLineForFunction))
                        {
                            if (!isMulti && !compilingTrues && !elseFound)
                            {
                                mayCheckForLastFunction = true;
                                canContinue = false;
                            }
                            else
                            {
                                foreach (string line in compiled)
                                {
                                    if (compilingTrues)
                                        compiledTrue.Add(line);
                                    else
                                        compiledFalse.Add(line);
                                }

                                if (!isMulti)
                                {
                                    if (compilingTrues)
                                        compilingTrues = false;
                                    else
                                        canContinue = false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                }

                if(canContinue)
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                }
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

            if(mayCheckForLastFunction)
            {
                while (currentNextLine.Length > 0 && currentNextLine[0] == ' ')
                    currentNextLine = currentNextLine.Substring(1);

                if (compiler.CompileLine(currentNextLine,out List<string> lastLines,endIndex))
                    compiledLines.AddRange(lastLines);
                else
                    return false;
            }

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

        /// <summary>
        /// Extracts the string inside the first encountered set of ()
        /// </summary>
        /// <param name="fullString">The string</param>
        /// <returns>The extracted string. Null if nothing was found</returns>
        protected string ExtractParenthesisContent(string fullString)
        {
            int startIdx = -1;
            int endIdx = -1;
            int parenthesisToClose = 0;
            for (int i = 0; i < fullString.Length;i++)
            {
                if (fullString[i] == '(')
                {
                    if(parenthesisToClose == 0)
                        startIdx = i + 1;

                    parenthesisToClose++;
                }
                if (fullString[i] == ')')
                {
                    parenthesisToClose--;

                    if (parenthesisToClose == 0)
                    {
                        endIdx = i - 1;

                        if(startIdx < endIdx && startIdx != -1 && endIdx != -1 && endIdx < fullString.Length)
                        {
                            return fullString.Substring(startIdx, endIdx - startIdx + 1);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Checks the result for the 
        /// </summary>
        /// <param name="result">True if the if was correct</param>
        /// <param name="debug">True if debug mode is enabled (Don't check variables, only the composition)</param>
        /// <returns>True if there was no errors</returns>
        protected bool CheckIfContent(string ifContent,out bool result, bool debug = false)
        {
            PlayerVariableContainer variableContainer = null;
            if(!debug)
            {
                if(!PersistentDataManager.instance.GetPlayerData().GetDataContainer(out variableContainer))
                {
                    result = false;
                    return false;
                }
            }

            if(CheckIfContentImpl(ifContent, variableContainer, out result))
                return true;

            result = false;
            return true;
        }

        /// <summary>
        /// Implementation for the if content check
        /// </summary>
        /// <param name="line">The current line to parse</param>
        /// <param name="container">The variable container</param>
        /// <param name="result">The out result</param>
        /// <returns>True if no problem was encountered</returns>
        protected bool CheckIfContentImpl(string line,PlayerVariableContainer container, out bool result)
        {
            result = true;
            return true;
        }
    }
}