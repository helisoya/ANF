using ANF.World;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a slot button in the save menu
	/// </summary>
    public class SaveMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Locals.LocalizedText iconText;
        [SerializeField] private Locals.LocalizedText labelText;
        [SerializeField] private Image backgroundImage;

        private SaveMenuUI saveMenuUI;
        private SaveMenuButtonData data;
        private int id;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="saveMenu">The save menu</param>
        /// <param name="data">The button's data</param>
        public void Initialize(int id, SaveMenuUI saveMenu, SaveMenuButtonData data)
        {
            this.id = id;
            this.data = data;
            this.saveMenuUI = saveMenu;

            if (data != null)
            {
                iconText.GetText().text = data.saveFileIcon;
                labelText.GetText().text = data.label;
                backgroundImage.sprite = data.bgSprite;
            }
        }

        public void OnEnter()
        {
            root.DOScale(Vector2.one * 0.9f, 0.5f).SetEase(Ease.OutQuad);
            root.DOShakeRotation(0.5f, new Vector3(0, 0, 5f)).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                root.DORotate(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
            });
        }

        public void OnExit()
        {
            root.DOScale(Vector2.one, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit();
        }
    }

    /// <summary>
	/// Represents a save menu button's data
	/// </summary>
    public class SaveMenuButtonData
    {
        public bool saveFileExists;
        public string saveFileName;
        public string saveFileIcon;
        public string label;
        public Sprite bgSprite;
    }
}
