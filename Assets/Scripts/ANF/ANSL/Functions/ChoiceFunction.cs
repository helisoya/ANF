using ANF.ANSL;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Choice Function starts a choice sequence, allowing the player to chose between a few options
    /// </summary>
    [ANSLFunctionAttribute(
        functionId: 26,
        functionBody: "choice",
        functionAutoComplete: new string[] {
            "choice(Title)\n\tKey Script\nendchoice"
        },
        functionDesc: "Starts a choice")]
    public class ChoiceFunction : ANSLFunction
    {
        private bool waitingForChoice = false;
        private ChoiceUI choiceUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.LISTSTRING}
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string[] split = cleanedLine.Split(new char[] { '(', ')' });

            if (split.Length > 3 || split[2].Length != 0)
            {
                // More than one pair of () found
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Too many () detected : {cleanedLine}."
                });
                return false;
            }

            string titleKey = split[1];

            string fullLine = $"{GetAttribute().functionId}|{titleKey}";


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

                if (currentNextLine.StartsWith("endchoice"))
                {
                    if (currentNextLine.Length != "endchoice".Length)
                    {
                        // Unknown character
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unknown character at the end of the line : {currentNextLine}."
                        });
                        return false;
                    }

                    foundEnd = true;
                    canContinue = false;
                }
                else
                {
                    split = currentNextLine.Split(' ');

                    if (split.Length != 2)
                    {
                        // Unknown character
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Could not parse choice : {currentNextLine}."
                        });
                        return false;
                    }

                    fullLine += $"|{split[0]}|{split[1]}";
                }

                if (canContinue)
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                }
            }

            if (!foundEnd)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"endchoice not found : {currentNextLine}."
                });
                return false;
            }

            compiledLines.Add(fullLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string titleKey) &&
                parameters.GetParameter(1, out string[] choices) &&
                manager.GetGUIManager().GetComponent<ChoiceUI>(out choiceUI))
            {
                ChoiceData data = new ChoiceData();
                data.title = titleKey;
                data.entries = new ChoiceData.ChoiceDataEntry[choices.Length / 2];
                for (int i = 0; i < choices.Length; i += 2)
                {
                    data.entries[i / 2] = new ChoiceData.ChoiceDataEntry() { textKey = choices[i], linkedScript = choices[i + 1] };
                }


                choiceUI.SetEnabled(true, data);

                waitingForChoice = true;
            }
            else
            {
                EndProcess();
            }

        }

        protected override void OnUpdate()
        {
            if (choiceUI == null)
                manager.GetGUIManager().GetComponent<ChoiceUI>(out choiceUI);

            if (choiceUI != null && choiceUI.showingChoice)
                return;

            if (choiceUI)
            {
                if (choiceUI.showingChoice)
                    return;

                EndProcess();
                context.LoadScript(choiceUI.selectedScript);
            }
            else
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForChoice", waitingForChoice);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForChoice"))
                waitingForChoice = json.GetBool("waitingForChoice");
            else
                waitingForChoice = false;
        }
    }
}

