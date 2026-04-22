using System.Collections.Generic;
using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents the component responsible for showing dialogs
	/// </summary>
    [System.Serializable]
    public class DialogUI : GUIComponent
    {
        [Header("Components")]
        [SerializeField] private GameObject speakerRoot;
        [SerializeField] private Locals.LocalizedText speakerText;
        [SerializeField] private Locals.LocalizedText dialogText;

        [Header("Infos")]
        [SerializeField] private float punctuationSpeedFactor = 3;

        private List<string> textIds = new List<string>();
        private bool canSkip;
        private int revealIndex;
        private float currentWaitTime;
        private float secondsBetweenCharacters;
        public bool showingDialog { get; private set; }

        private char[] punctuations = { '.', ',', '?', '!', ';', ':' };

        public override void OnInitialize()
        {
            textIds = new List<string>();

            speakerText.SetCanReload(true, false);
            dialogText.SetLocalizationEnabled(false, false);
            dialogText.SetCanReload(false, false);
        }

        public override void OnStart()
        {
        }

        /// <summary>
        /// Toggles the can skip flag (Skips the dialog typewriter, and )
        /// </summary>
        public void ToggleCanSkip()
        {
            canSkip = true;
        }

        /// <summary>
        /// Starts a new dialog
        /// </summary>
        /// <param name="speakerID">The speaker's ID. null to hide the speaker text</param>
        /// <param name="dialogID">The dialog's ID</param>
        /// <param name="additive">True if the text is additive</param>
        /// <param name="secondsBetweenCharacters">The number of seconds between characters</param>
        public void StartDialog(string speakerID, string dialogID, bool additive = false, float secondsBetweenCharacters = 0.05f)
        {
            canSkip = false;
            showingDialog = true;
            currentWaitTime = 0;
            this.secondsBetweenCharacters = secondsBetweenCharacters;

            speakerRoot.SetActive(speakerID != null);
            if (speakerID != null)
            {
                bool speakerIsMC = speakerText.Equals("[MC]");
                if (speakerIsMC &&
                PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer container))
                {
                    speakerID = container.GetPlayerName();
                }
                speakerText.SetLocalizationEnabled(!speakerIsMC, false);
                speakerText.SetNewKey(speakerID);
            }

            if (dialogID != null)
            {
                if (!additive)
                    textIds.Clear();

                textIds.Add(dialogID);

                RegenerateDialogFromStack(false);
            }
        }

        /// <summary>
		/// Regenerates the dialog from the text stack and recompute the reveal Index
		/// </summary>
        /// <param name="lastTextCompleted">True if the last text was completed</param>
        private void RegenerateDialogFromStack(bool lastTextCompleted)
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out Locals.Locals locals))
            {
                revealIndex = 0;
                string result = "";
                for (int i = 0; i < textIds.Count; i++)
                {
                    result += locals.GetLocal(textIds[i]);
                    if (i != textIds.Count - 1 || lastTextCompleted)
                        revealIndex = result.Length;
                }

                dialogText.SetNewKey(result);
                dialogText.GetText().ForceMeshUpdate(true);
                dialogText.GetText().maxVisibleCharacters = revealIndex;
            }
        }

        public override void OnUpdate()
        {
            if (showingDialog)
            {
                TMP_Text text = dialogText.GetText();
                bool stillCharactersToReveal = revealIndex >= text.maxVisibleCharacters;
                if (canSkip)
                {
                    if (stillCharactersToReveal)
                    {
                        // End of dialog
                        showingDialog = false;
                        return;
                    }
                    else
                    {
                        // Skip typewriter
                        revealIndex = text.textInfo.characterCount;
                        text.maxVisibleCharacters = revealIndex;
                    }
                }
                else if (stillCharactersToReveal)
                {
                    if (currentWaitTime <= 0)
                    {
                        revealIndex++;
                        currentWaitTime = secondsBetweenCharacters *
                            (IsPunctuation(text.textInfo.characterInfo[revealIndex].character) ? punctuationSpeedFactor : 1.0f);
                        text.maxVisibleCharacters = revealIndex;
                    }
                    else
                    {
                        currentWaitTime -= Time.deltaTime;
                    }
                }
            }
        }

        /// <summary>
		/// Checks if a character is punctuation or not
		/// </summary>
		/// <param name="character">The character</param>
		/// <returns>True if it is punctuation</returns>
        private bool IsPunctuation(char character)
        {
            foreach (char punctuation in punctuations)
                if (punctuation == character)
                    return true;
            return false;
        }

        protected override void OnClose()
        {
        }

        protected override void OnOpen()
        {
        }

        protected override void OnSave(JSON json)
        {
            json.Add("showingDialog", showingDialog);
            json.Add("secondsBetweenCharacters", secondsBetweenCharacters);
            json.Add("currentWaitTime", currentWaitTime);
            json.Add("revealIndex", revealIndex);
            json.Add("textIds", textIds);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingDialog"))
                showingDialog = json.GetBool("showingDialog");
            if (json.ContainsKey("secondsBetweenCharacters"))
                secondsBetweenCharacters = json.GetFloat("secondsBetweenCharacters");
            if (json.ContainsKey("currentWaitTime"))
                currentWaitTime = json.GetFloat("currentWaitTime");
            if (json.ContainsKey("revealIndex"))
                revealIndex = json.GetInt("revealIndex");
            if (json.ContainsKey("textIds"))
                textIds = new List<string>(json.GetJArray("textIds").AsStringArray());

            RegenerateDialogFromStack(!showingDialog);
        }
    }

}
