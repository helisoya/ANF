using ANF.World;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a button in the pause menu
    /// </summary>
    public class PauseMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private Locals.LocalizedText label;
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform iconTransform;

        private PauseMenuUI pauseMenu;
        private ANFManager manager;
        private PauseMenuButtonData data;
        private int id;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="data">The button's data</param>
        /// <param name="pauseMenu">The pause menu</param>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(int id, PauseMenuButtonData data, PauseMenuUI pauseMenu, ANFManager manager)
        {
            this.id = id;
            this.data = data;
            this.pauseMenu = pauseMenu;
            this.manager = manager;

            iconTransform.sizeDelta = Vector2.zero;

            if (data != null)
            {
                label.SetNewKey(data.labelKey);
                icon.sprite = data.iconSprite;
            }
        }

        public void OnClick()
        {
            if (data != null)
                data.OnClick(pauseMenu, manager);
        }

        public void OnEnter()
        {
            iconTransform.DOSizeDelta(Vector2.one * 40f, 0.5f).SetEase(Ease.OutQuad);
            buttonRoot.DOAnchorPosX(25, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnExit()
        {
            iconTransform.DOSizeDelta(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
            buttonRoot.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pauseMenu.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }


    /// <summary>
    /// Represents the data linked to a pause menu button.
    /// Extend this to create a new button
    /// </summary>
    [System.Serializable]
    public abstract class PauseMenuButtonData
    {
        public Sprite iconSprite;
        public string labelKey;

        public abstract void OnClick(PauseMenuUI pauseMenu, ANFManager manager);
    }

    /// <summary>
    /// Subclass for the resume button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataResume : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            pauseMenu.Close();
        }
    }

    /// <summary>
    /// Subclass for the save button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataSave : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {

        }
    }

    /// <summary>
    /// Subclass for the load button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataLoad : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {

        }
    }

    /// <summary>
    /// Subclass for the options button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataOptions : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {

        }
    }

    /// <summary>
    /// Subclass for the Main Menu button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataMainMenu : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
        }
    }

    /// <summary>
    /// Subclass for the quit button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataQuit : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            Application.Quit();
        }
    }
}
