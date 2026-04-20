using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// A fade system that can fade in & out the screen
    /// </summary>
    [System.Serializable]
    public class Fade : GUIComponent
    {
        [Header("Fade")]
        [SerializeField] private Image fadeImg;
        [SerializeField] private CanvasGroup canvasGroup;
        private float targetAlpha;
        private float currentTransitionSpeed;
        public bool fading { get; private set; }

        /// <summary>
		/// Changes the component's color
		/// </summary>
		/// <param name="newColor">The new color</param>
        public void SetColor(Color newColor)
        {
            fadeImg.color = newColor;
        }

        /// <summary>
		/// Starts a fade
		/// </summary>
		/// <param name="target">The Alpha target</param>
		/// <param name="immediate">True if the transition should be immediate</param>
		/// <param name="transitionSpeed">The transition speed if not immediate</param>
        public void FadeTo(float target, bool immediate = false, float transitionSpeed = 1.0f)
        {
            targetAlpha = target;
            currentTransitionSpeed = transitionSpeed;
            if (immediate)
                canvasGroup.alpha = target;

            fading = !immediate;
        }

        public override void UpdateComponent()
        {
            if (fading)
            {
                float currentAlpha = canvasGroup.alpha;
                float side = targetAlpha > currentAlpha ? 1 : -1;
                float max = side == 1 ? targetAlpha : 1f;
                float min = side == 1 ? currentAlpha : targetAlpha;
                float nextValue = Mathf.Clamp(currentAlpha + currentTransitionSpeed * side * Time.deltaTime, min, max);

                canvasGroup.alpha = nextValue;
                if (nextValue == targetAlpha)
                    fading = false;
            }
        }


        protected override void OnInitialize()
        {
            fading = false;
            targetAlpha = canvasGroup.alpha;
        }

        protected override void OnOpen()
        {
            // Unused
        }

        protected override void OnClose()
        {
            // Unused
        }


        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("fading"))
                fading = json.GetBool("fading");
            if (json.ContainsKey("targetAlpha"))
                targetAlpha = json.GetFloat("targetAlpha");
            if (json.ContainsKey("currentTransitionSpeed"))
                currentTransitionSpeed = json.GetFloat("currentTransitionSpeed");
            if (json.ContainsKey("currentAlpha"))
                canvasGroup.alpha = json.GetFloat("currentAlpha");
            if (json.ContainsKey("currentColor"))
                fadeImg.color = json.GetJArray("currentColor").AsColor();
        }

        protected override void OnSave(JSON json)
        {
            json.Add("fading", fading);
            json.Add("targetAlpha", targetAlpha);
            json.Add("currentTransitionSpeed", currentTransitionSpeed);
            json.Add("currentAlpha", canvasGroup.alpha);
            json.Add("currentColor", fadeImg.color);
        }

    }
}

