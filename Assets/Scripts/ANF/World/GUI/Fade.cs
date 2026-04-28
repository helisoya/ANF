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

        private float startAlpha;
        private float tAlpha;
        private float targetAlpha;
        private float currentAlphaTransitionDuration;

        private Color startColor;
        private float tColor;
        private Color targetColor;
        private float currentColorTransitionDuration;

        public bool fadingAlpha { get; private set; }
        public bool fadingColor { get; private set; }

        /// <summary>
		/// Starts an alpha transition
		/// </summary>
		/// <param name="target">The Alpha target</param>
		/// <param name="immediate">True if the transition should be immediate</param>
		/// <param name="transitionDuration">The transition's duration if not immediate</param>
        public void FadeAlphaTo(float target, bool immediate = false, float transitionDuration = 1.0f)
        {
            startAlpha = canvasGroup.alpha;
            tAlpha = 0.0f;
            targetAlpha = target;
            currentAlphaTransitionDuration = transitionDuration;
            if (immediate)
                canvasGroup.alpha = target;

            fadingAlpha = !immediate;
        }

        /// <summary>
        /// Starts a color transition
        /// </summary>
        /// <param name="target">The color target</param>
        /// <param name="immediate">True if the transition should be immediate</param>
        /// <param name="transitionDuration">The transition's duration if not immediate</param>
        public void FadeColorTo(Color target, bool immediate = false, float transitionDuration = 1.0f)
        {
            startColor = fadeImg.color;
            tColor = 0.0f;
            targetColor = target;
            currentColorTransitionDuration = transitionDuration;
            if (immediate)
                fadeImg.color = target;

            fadingColor = !immediate;
        }

        public override void OnUpdate()
        {
            if (fadingAlpha)
            {
                tAlpha += Time.deltaTime / currentAlphaTransitionDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, tAlpha);

                if (tAlpha >= 1.0f)
                    fadingAlpha = false;
            }

            if (fadingColor)
            {
                tColor += Time.deltaTime / currentColorTransitionDuration;
                fadeImg.color = Color.Lerp(startColor, targetColor, tColor);

                if (tColor >= 1.0f)
                    fadingColor = false;
            }
        }


        public override void OnInitialize()
        {
            fadingColor = false;
            fadingAlpha = false;
        }

        public override void OnStart()
        {
            // Unused
        }

        public override void OnDisabled()
        {
            // Unused
        }

        public override void OnEnabled()
        {
            // Unused
        }

        public override void OnPaused()
        {
            // Unused
        }

        public override void OnUnPaused()
        {
            // Unused
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("fadingAlpha"))
                fadingAlpha = json.GetBool("fadingAlpha");
            if (json.ContainsKey("fadingColor"))
                fadingColor = json.GetBool("fadingColor");

            if (json.ContainsKey("targetAlpha"))
                targetAlpha = json.GetFloat("targetAlpha");
            if (json.ContainsKey("startAlpha"))
                startAlpha = json.GetFloat("startAlpha");
            if (json.ContainsKey("tAlpha"))
                tAlpha = json.GetFloat("tAlpha");
            if (json.ContainsKey("currentAlphaTransitionDuration"))
                currentAlphaTransitionDuration = json.GetFloat("currentAlphaTransitionDuration");

            if (json.ContainsKey("targetColor"))
                targetColor = json.GetJArray("targetColor").AsColor();
            if (json.ContainsKey("startColor"))
                startColor = json.GetJArray("startColor").AsColor();
            if (json.ContainsKey("tColor"))
                tColor = json.GetFloat("tColor");
            if (json.ContainsKey("currentColorTransitionDuration"))
                currentColorTransitionDuration = json.GetFloat("currentColorTransitionDuration");

            if (json.ContainsKey("currentAlpha"))
                canvasGroup.alpha = json.GetFloat("currentAlpha");
            if (json.ContainsKey("currentColor"))
                fadeImg.color = json.GetJArray("currentColor").AsColor();
        }

        public override void OnSave(JSON json)
        {
            json.Add("fadingAlpha", fadingAlpha);
            json.Add("fadingColor", fadingColor);

            json.Add("startAlpha", startAlpha);
            json.Add("tAlpha", tAlpha);
            json.Add("targetAlpha", targetAlpha);
            json.Add("currentAlphaTransitionDuration", currentAlphaTransitionDuration);

            json.Add("startColor", startColor);
            json.Add("tColor", tColor);
            json.Add("currentColorTransitionDuration", currentColorTransitionDuration);
            json.Add("targetColor", targetColor);

            json.Add("currentAlpha", canvasGroup.alpha);
            json.Add("currentColor", fadeImg.color);
        }


    }
}

