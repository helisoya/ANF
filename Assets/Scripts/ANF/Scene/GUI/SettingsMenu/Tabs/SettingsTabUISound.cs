using ANF.Persistent;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the sound tab in the settings
    /// </summary>
    public class SettingsTabUISound : SettingsTabUI
    {
        [SerializeField] private float sliderMaxValue = 20;
        [SerializeField] private float sliderMinValue = -80;

        private Slider musicSlider;
        private Slider ambientSlider;
        private Slider sfxSlider;
        private Slider voiceSlider;

        public override string GetLabelKey()
        {
            return "SettingsMenu_Sound";
        }

        public override void PopulateTab()
        {
            if(PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
            {
                musicSlider = menu.CreateSlider("SettingsMenu_Sound_Music", root);
                musicSlider.minValue = sliderMinValue;
                musicSlider.maxValue = sliderMaxValue;
                musicSlider.SetValueWithoutNotify(audioManager.GetMusicVolume());
                musicSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetMusicVolume(newVolume);
                });

                ambientSlider = menu.CreateSlider("SettingsMenu_Sound_Ambient", root);
                ambientSlider.minValue = sliderMinValue;
                ambientSlider.maxValue = sliderMaxValue;
                ambientSlider.SetValueWithoutNotify(audioManager.GetAmbientVolume());
                ambientSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetAmbientVolume(newVolume);
                });

                sfxSlider = menu.CreateSlider("SettingsMenu_Sound_Sfx", root);
                sfxSlider.minValue = sliderMinValue;
                sfxSlider.maxValue = sliderMaxValue;
                sfxSlider.SetValueWithoutNotify(audioManager.GetSfxVolume());
                sfxSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetSfxVolume(newVolume);
                });

                voiceSlider = menu.CreateSlider("SettingsMenu_Sound_Voice", root);
                voiceSlider.minValue = sliderMinValue;
                voiceSlider.maxValue = sliderMaxValue;
                voiceSlider.SetValueWithoutNotify(audioManager.GetVoiceVolume());
                voiceSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetVoiceVolume(newVolume);
                });
            }


        }

        public override void RedrawLocalizedElements()
        {

        }
    }

}
