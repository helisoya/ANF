using ANF.Persistent;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the graphics tab in the settings
    /// </summary>
    public class SettingsTabUIGraphics : SettingsTabUI
    {
        private Toggle fullscreenToggle;
        private TMP_Dropdown resolutionDropdown;

        public override string GetLabelKey()
        {
            return "SettingsMenu_Graphics";
        }

        public override void PopulateTab()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out ScreenSettingsContainer screenSettings))
            {
                fullscreenToggle = menu.CreateToggle("SettingsMenu_Graphics_Fullscreen", root);
                fullscreenToggle.SetIsOnWithoutNotify(screenSettings.IsFullscreen());
                fullscreenToggle.onValueChanged.AddListener((bool fullscreen) =>
                {
                    screenSettings.SetFullscreen(fullscreen);
                });

                Resolution[] resolutions = Screen.resolutions;
                Resolution currentResolution = screenSettings.GetResolution();
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

                resolutionDropdown = menu.CreateDropdown("SettingsMenu_Graphics_Resolution", root);
                resolutionDropdown.ClearOptions();
                resolutionDropdown.AddOptions(resolutionsLabels);
                resolutionDropdown.SetValueWithoutNotify(currentIdx);
                resolutionDropdown.onValueChanged.AddListener((int idx) =>
                {
                    screenSettings.SetResolution(resolutions[idx]);
                });
            }


        }

        public override void RedrawLocalizedElements()
        {

        }
    }

}
