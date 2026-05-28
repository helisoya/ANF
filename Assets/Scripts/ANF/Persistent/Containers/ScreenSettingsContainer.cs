
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the Screen Settings
    /// </summary>
    [System.Serializable]
    public class ScreenSettingsContainer : DataContainer
    {
        [Tooltip("If true, Screen.currentResolution will be used as a default")]
        [SerializeField] private bool useScreenSettings = true;
        [SerializeField] private bool defaultFullscreen = true;
        [SerializeField] private int defaultWidth = 800;
        [SerializeField] private int defaultHeight = 600;
        private Resolution resolution;
        private bool fullscreen;

        public DataContainer CloneContainer()
        {
            return new ScreenSettingsContainer()
            {
                defaultFullscreen = defaultFullscreen,
                useScreenSettings = useScreenSettings,
                defaultWidth = defaultWidth,
                defaultHeight = defaultHeight
            };
        }

        public void Initialize(ANFSettings settings)
        {
            Reset();
        }

        public void Reset()
        {
            if (useScreenSettings)
            {
                resolution = Screen.currentResolution;
                fullscreen = Screen.fullScreen;
            }
            else
            {
                resolution = new Resolution() { height = defaultHeight, width = defaultWidth, refreshRateRatio = Screen.currentResolution.refreshRateRatio };

                SetFullscreen(defaultFullscreen);
                SetResolution(resolution);
            }
        }

        public void Load(JSON json)
        {
            int width = resolution.width;
            int height = resolution.height;
            uint denominator = resolution.refreshRateRatio.denominator;
            uint numerator = resolution.refreshRateRatio.numerator;

            if (json.ContainsKey("fullscreen"))
                fullscreen = json.GetBool("fullscreen");

            if (json.ContainsKey("width"))
                width = json.GetInt("width");
            if (json.ContainsKey("height"))
                height = json.GetInt("height");
            if (json.ContainsKey("refreshRateDenomiator"))
                denominator = json.GetJNumber("refreshRateDenomiator").AsUInt();
            if (json.ContainsKey("refreshRateNumerator"))
                numerator = json.GetJNumber("refreshRateNumerator").AsUInt();

            SetResolution(new Resolution()
            {
                height = height,
                width = width,
                refreshRateRatio = new() { denominator = denominator, numerator = numerator }
            });
            SetFullscreen(fullscreen);
        }

        public void Save(JSON json)
        {
            json.Add("fullscreen", fullscreen);
            json.Add("width", resolution.width);
            json.Add("height", resolution.height);
            json.Add("refreshRateDenomiator", resolution.refreshRateRatio.denominator);
            json.Add("refreshRateNumerator", resolution.refreshRateRatio.numerator);
        }

        /// <summary>
        /// Sets the game's resolution
        /// </summary>
        /// <param name="resolution">The new resolution</param>
        public void SetResolution(Resolution resolution)
        {
            this.resolution = resolution;
            Screen.SetResolution(resolution.width, resolution.height,
                fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
                resolution.refreshRateRatio);
        }

        /// <summary>
        /// Sets if the game is in fullscreen or not
        /// </summary>
        /// <param name="fullscreen">True if fullscreen</param>
        public void SetFullscreen(bool fullscreen)
        {
            this.fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// Gets if the game is in fullscreen or not
        /// </summary>
        /// <returns>True if in fullscreen</returns>
        public bool IsFullscreen()
        {
            return fullscreen;
        }

        /// <summary>
        /// Gets the current gam's resolution
        /// </summary>
        /// <returns>The current resolution</returns>
        public Resolution GetResolution()
        {
            return resolution;
        }
    }

}
