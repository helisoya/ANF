using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the save menu located within the pause menu
    /// </summary>
    public class SaveMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Save Menu")]
        [SerializeField] private Locals.LocalizedText titleText;
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private SaveMenuButton buttonPrefab;

        private bool inSaveMode;

        public override void OnInitialize()
        {
            inSaveMode = false;
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {

        }

        /// <summary>
        /// Changes if the pause menu is enabled or not
        /// </summary>
        /// <param name="enabled">True if the pause menu is now enabled</param>
        /// <param name="inSaveMode">True if the pause menu should be in save mode</param>
        public void SetEnabled(bool enabled, bool inSaveMode)
        {
            this.inSaveMode = inSaveMode;
            SetEnabled(enabled);
        }

        /// <summary>
        /// Changes if the menu is in save mode
        /// </summary>
        /// <param name="inSaveMode">True if in save mode</param>
        public void SetIsInSaveMode(bool inSaveMode)
        {
            this.inSaveMode = inSaveMode;
        }

        /// <summary>
		/// Generates a save button
		/// </summary>
		/// <param name="settings">The ANF settings</param>
		/// <param name="saveName">The save's name</param>
		/// <param name="saveIcon">The save's icon</param>
        private void GenerateSaveButton(ANFSettings settings, string saveName, string saveIcon)
        {
            string savePath = SaveUtils.GetSavePath(saveName, settings.saveFolder);
            string label = "";

            JSON saveFile = SaveUtils.LoadJSON(savePath);
            if (saveFile != null)
            {
                try
                {
                    label = saveFile.GetJSON("playerData").GetJSON("playerVariableContainer").GetString("playerName");
                }
                catch
                {
                }
            }

            Instantiate(buttonPrefab, buttonsRoot).Initialize(0, this,
            new SaveMenuButtonData()
            {
                saveFileIcon = saveIcon,
                saveFileName = savePath,
                label = label,
                bgSprite = null,
            });
        }

        public override void OnEnabled()
        {
            ANFSettings settings = PersistentDataManager.instance.GetANFSettings();
            titleText.SetNewKey(inSaveMode ? "SaveMenu_Title_Save" : "SaveMenu_Title_Load");

            foreach (Transform child in buttonsRoot)
                Destroy(child.gameObject);

            GenerateSaveButton(settings, "autosave", "A");

            for (int i = 0; i < settings.saveSlotsAmount; i++)
            {
                GenerateSaveButton(settings, i.ToString(), i.ToString());
            }

            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);
        }

        public override void OnDisabled()
        {
            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }
    }
}

