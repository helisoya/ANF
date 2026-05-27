using ANF.Locals;
using ANF.Persistent;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ANF.Locals.Locals;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the text tab in the settings
    /// </summary>
    public class SettingsTabUIText : SettingsTabUI
    {
        [SerializeField] private SerializedDictionary<Locals.Locals.Channel, bool> channelsToShow = new SerializedDictionary<Locals.Locals.Channel, bool>()
        {
            { Locals.Locals.Channel.CHANNEL0, true },
            { Locals.Locals.Channel.CHANNEL1, true },
            { Locals.Locals.Channel.CHANNEL2, false },
            { Locals.Locals.Channel.CHANNEL3, false },
            { Locals.Locals.Channel.CHANNEL4, false },
            { Locals.Locals.Channel.CHANNEL5, false },
            { Locals.Locals.Channel.CHANNEL6, false },
            { Locals.Locals.Channel.CHANNEL7, false },
            { Locals.Locals.Channel.CHANNEL8, false },
            { Locals.Locals.Channel.CHANNEL9, false }
        };

        private TMP_Dropdown dropdownLanguage;
        private TMP_Dropdown[] dropdownsFont;
        private TMP_Dropdown[] dropdownsSize;
        private Button[] buttonsColor;
        private Button resetButton;

        public override string GetLabelKey()
        {
            return "SettingsMenu_Text";
        }

        public override void PopulateTab()
        {
            if(PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals))
            {
                int channelCount = Enum.GetValues(typeof(Locals.Locals.Channel)).Length;

                List<string> languages = GetLanguageLabels(locals);
                int currentIdx = locals.GetCurrentLanguageIndex();

                dropdownLanguage = menu.CreateDropdown("SettingsMenu_Text_Language", root);
                dropdownLanguage.ClearOptions();
                dropdownLanguage.AddOptions(languages);
                dropdownLanguage.SetValueWithoutNotify(currentIdx);
                dropdownLanguage.onValueChanged.AddListener((int newIndex) =>
                {
                    locals.ChangeLanguage(locals.GetLanguages()[newIndex]);

                    if (manager.GetGUIManager().GetComponent<DialogUI>(out DialogUI dialogUI))
                        dialogUI.RefreshDialogLocals();

                    menu.RedrawLocalizedElements();
                });

                List<string> sizeLabels = new List<string>();
                for (int i = 0; i < locals.GetSizes().Length;i++)
                    sizeLabels.Add(locals.GetLocal($"SettingsMenu_Text_Sizes_{i}"));

                List<string> fontLabels = new List<string>();
                foreach (TMP_FontAsset font in locals.GetFonts())
                    fontLabels.Add(font.name);

                dropdownsFont = new TMP_Dropdown[channelCount];
                dropdownsSize = new TMP_Dropdown[channelCount];
                buttonsColor = new Button[channelCount];

                using (var it = Enumerable.Range(0, channelCount).GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        int value = it.Current;
                        Locals.Locals.Channel channel = (Locals.Locals.Channel)value;

                        if (channelsToShow.TryGetValue(channel, out bool shouldShow) && shouldShow)
                        {
                            dropdownsFont[value] = menu.CreateDropdown($"SettingsMenu_Text_Channel_{value}_Font", root);
                            dropdownsFont[value].ClearOptions();
                            dropdownsFont[value].AddOptions(fontLabels);
                            dropdownsFont[value].SetValueWithoutNotify(locals.GetFontIndex(channel));
                            dropdownsFont[value].onValueChanged.AddListener((int newIndex) =>
                            {
                                locals.ChangeFont(channel, newIndex);
                            });

                            dropdownsSize[value] = menu.CreateDropdown($"SettingsMenu_Text_Channel_{value}_Size", root);
                            dropdownsSize[value].ClearOptions();
                            dropdownsSize[value].AddOptions(sizeLabels);
                            dropdownsSize[value].SetValueWithoutNotify(locals.GetFontSizeIndex(channel));
                            dropdownsSize[value].onValueChanged.AddListener((int newIndex) =>
                            {
                                locals.ChangeSize(channel, newIndex);
                            });

                            buttonsColor[value] = menu.CreateColorPicker($"SettingsMenu_Text_Channel_{value}_Color", root);
                            buttonsColor[value].GetComponent<Image>().color = locals.GetColor(channel);
                            buttonsColor[value].onClick.AddListener(() =>
                            {
                                menu.OpenColorPicker(locals.GetColor(channel), buttonsColor[value], (Color selectedColor) =>
                                {
                                    buttonsColor[value].GetComponent<Image>().color = selectedColor;
                                    locals.ChangeColor(channel, selectedColor);
                                });
                            });
                        }
                    }
                }

                resetButton = menu.CreateButton($"SettingsMenu_Reset", root);
                resetButton.onClick.AddListener(() =>
                {
                    Reset();
                });
            }
        }

        /// <summary>
        /// Resets the parameters to their default values
        /// </summary>
        private void Reset()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals))
            {
                locals.Reset();

                RedrawLocalizedElements();

                int channelCount = Enum.GetValues(typeof(Locals.Locals.Channel)).Length;

                using (var it = Enumerable.Range(0, channelCount).GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        int value = it.Current;
                        Locals.Locals.Channel channel = (Locals.Locals.Channel)value;

                        if (channelsToShow.TryGetValue(channel, out bool shouldShow) && shouldShow)
                        {
                            dropdownsFont[value].SetValueWithoutNotify(locals.GetFontIndex(channel));
                            dropdownsSize[value].SetValueWithoutNotify(locals.GetFontSizeIndex(channel));
                            buttonsColor[value].GetComponent<Image>().color = locals.GetColor(channel);
                        }
                    }
                }
            }
        }

        public override void RedrawLocalizedElements()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals))
            {
                List<string> languages = GetLanguageLabels(locals);
                int currentIdx = locals.GetCurrentLanguageIndex();
                dropdownLanguage.ClearOptions();
                dropdownLanguage.AddOptions(languages);
                dropdownLanguage.SetValueWithoutNotify(currentIdx);
            }
        }

        /// <summary>
        /// Computes the list of labels for the available languages
        /// </summary>
        /// <param name="locals">The locals</param>
        /// <returns>The list of labels</returns>
        private List<string> GetLanguageLabels(Locals.Locals locals)
        {
            List<string> result = new List<string>();

            string[] languages = locals.GetLanguages();

            for (int i = 0; i < languages.Length; i++)
                result.Add(locals.GetLocal($"SettingsMenu_Text_Language_{languages[i]}"));

            return result;
        }
    }

}
