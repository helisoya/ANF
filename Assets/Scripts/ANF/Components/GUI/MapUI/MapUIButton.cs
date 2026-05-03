using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a map's button
	/// </summary>
    public class MapUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private Image buttonImg;
        [SerializeField] private RectTransform labelRoot;
        [SerializeField] private Locals.LocalizedText label;
        private string linkedScript;
        private MapUI mapUI;
        private int id;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="labelKey">The label's key</param>
        /// <param name="linkedScript">The button's linked script</param>
        /// <param name="sprite">The button's sprite</param>
        /// <param name="mapUI">The Map UI</param>
        public void Initialize(int id, string labelKey, string linkedScript, Sprite sprite, MapUI mapUI)
        {
            this.id = id;
            this.mapUI = mapUI;
            this.linkedScript = linkedScript;

            buttonImg.sprite = sprite;

            label.SetNewKey(labelKey);
            labelRoot.localScale = new Vector2(0, 1);

            buttonRoot.localScale = Vector2.zero;
            buttonRoot.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).SetId(transform);
        }

        /// <summary>
		/// Gets the linked script for this button
		/// </summary>
		/// <returns>Its linked script</returns>
        public string GetLinkedScript()
        {
            return linkedScript;
        }

        /// <summary>
        /// Fades and destroy the button
        /// </summary>
        /// <param name="delay">The delay</param>
        /// <param name="actionOnDestroy">The action to perform afterwards (optional)</param>
        public void Fade(float delay, Action actionOnDestroy)
        {
            buttonRoot.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(delay).SetId(transform).OnComplete(() =>
            {
                if (actionOnDestroy != null)
                    actionOnDestroy.Invoke();
            });
        }

        public void OnEnter()
        {
            labelRoot.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonImg.DOColor(Color.softRed, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
        }

        public void OnExit()
        {
            labelRoot.DOScaleX(0, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonImg.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                mapUI.SelectButton(id);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mapUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}

