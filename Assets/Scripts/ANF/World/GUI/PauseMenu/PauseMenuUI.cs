using ANF.GUI;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the save menu
    /// </summary>
    [System.Serializable]
    public class PauseMenuUI : GUIComponent
    {
        [Header("Infos")]
        [SerializeField] private string[] guiComponentsToPause = {"fadeBg","fadeFg","dialog"};
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Base UI")]
        [SerializeField] private Image bgImg;
        [SerializeField] private Color bgTargetColor = new Color(0, 0, 0, 0.5f);
        [SerializeField] private RectTransform buttonsRoot;

        [Header("Buttons")]
        [SerializeReference, SubclassSelector(AllowNull = false)] private PauseMenuButtonData[] buttonDatas;
        [SerializeField] private PauseMenuButton buttonPrefab;

        public override void OnInitialize()
        {
            bgImg.color = Color.clear;
            buttonsRoot.anchoredPosition = new Vector2(-buttonsRoot.sizeDelta.x / 2f, 0);

            foreach(PauseMenuButtonData buttonData in buttonDatas)
            {
                Instantiate(buttonPrefab, buttonsRoot).Initialize(buttonData, this, manager);
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Pause").performed += OnPauseInput;
        }

        public override void OnUpdate()
        {

        }

        protected override void OnClose()
        {
            bgImg.DOComplete();
            bgImg.DOColor(Color.clear, transitionDuration).SetEase(Ease.OutQuad);

            buttonsRoot.DOComplete();
            float halfSizeButtonsRoot = buttonsRoot.sizeDelta.x / 2f;
            buttonsRoot.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsEnabled(guiComponentsToPause, true);
            manager.GetWorld().EnableWorldComponents(true);
            if (gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if (dialog.isOpen)
                    dialog.SetIsEnabled(true);
            }
        }

        protected override void OnOpen()
        {
            bgImg.DOComplete();
            bgImg.DOColor(bgTargetColor, transitionDuration).SetEase(Ease.OutQuad);

            buttonsRoot.DOComplete();
            float halfSizeButtonsRoot = buttonsRoot.sizeDelta.x / 2f;
            buttonsRoot.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsEnabled(guiComponentsToPause, false);
            manager.GetWorld().EnableWorldComponents(false);
            if(gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if(dialog.isOpen)
                    dialog.SetIsEnabled(false);
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if(context.ReadValueAsButton())
            {
                if (!isOpen)
                    Open();
                else
                    Close();
            }
        }

        protected override void OnSave(JSON json)
        {
        }
        protected override void OnLoad(JSON json)
        {
        }
    }
}
