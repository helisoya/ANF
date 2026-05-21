using ANF.GUI;
using ANF.Scene;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the graphics part of the settings (Resolution, fullscreen, ...)
    /// </summary>
    [System.Serializable]
    public class SettingsGraphicsTab : SettingsTab
    {
        private bool fullscreen;
        private Resolution currentResolution;

        public string GetName()
        {
            return "SettingsMenu_Graphics";
        }

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            fullscreen = Screen.fullScreen;
            currentResolution = Screen.currentResolution;
        }

        public void PopulateTab(ANFManager manager, SettingsMenuUI menu, RectTransform root)
        {
            Toggle fullscreenToggle = menu.CreateToggle("SettingsMenu_Graphics_Fullscreen", root);
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullScreenChange);

            Resolution[] resolutions = Screen.resolutions;
            List<string> resolutionsLabels = new List<string>();
            int currentIdx = -1;

            for (int i = 0; i < resolutions.Length; i++)
            {
                resolutionsLabels.Add($"{resolutions[i].width}/{resolutions[i].height}({Math.Round(resolutions[i].refreshRateRatio.value, 2)})");
                if (currentIdx == -1 && resolutions[i].width == currentResolution.width &&
                    resolutions[i].height == currentResolution.height && resolutions[i].refreshRateRatio.value == currentResolution.refreshRateRatio.value)
                {
                    currentIdx = i;
                }
            }

            if (currentIdx == -1)
                currentIdx = 0;

            TMP_Dropdown dropdown = menu.CreateDropdown("SettingsMenu_Graphics_Resolution", root);
            dropdown.ClearOptions();
            dropdown.AddOptions(resolutionsLabels);
            dropdown.SetValueWithoutNotify(currentIdx);
            dropdown.onValueChanged.AddListener(OnResolutionChange);
        }

        public void RedrawLocalizedEntries(ANFManager manager, SettingsMenuUI menu, RectTransform root)
        {

        }

        public void ApplySettings(ANFManager manager)
        {
            // Graphics settings don't need to be reapplied when loading a scene
        }

        public void Save(JSON json)
        {
            json.Add("fullscreen", fullscreen);
            json.Add("width", currentResolution.width);
            json.Add("height", currentResolution.height);
            json.Add("refreshRateDenomiator", currentResolution.refreshRateRatio.denominator);
            json.Add("refreshRateNumerator", currentResolution.refreshRateRatio.numerator);
        }

        public void Load(JSON json)
        {
            int width = currentResolution.width;
            int height = currentResolution.height;
            uint denominator = currentResolution.refreshRateRatio.denominator;
            uint numerator = currentResolution.refreshRateRatio.numerator;

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

            OnResolutionChange(new Resolution()
            {
                height = height,
                width = width,
                refreshRateRatio = new() { denominator = denominator, numerator = numerator }
            });
            OnFullScreenChange(fullscreen);
        }



        public void OnFullScreenChange(bool fullscreen)
        {
            this.fullscreen = fullscreen;
            Screen.SetResolution(currentResolution.width, currentResolution.height,
                fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
                currentResolution.refreshRateRatio);
        }

        public void OnResolutionChange(int idx)
        {
            OnResolutionChange(Screen.resolutions[idx]);
        }

        public void OnResolutionChange(Resolution resolution)
        {
            currentResolution = resolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height,
                fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
                currentResolution.refreshRateRatio);
        }
    }
}

