using ANF.Locals;
using ANF.Persistent;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the game tab in the settings
    /// </summary>
    public class SettingsTabUIGame : SettingsTabUI
    {

        public override string GetLabelKey()
        {
            return "SettingsMenu_Game";
        }

        public override void PopulateTab()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                foreach(string key in settings.GetValues().Keys)
                {
                    SettingsContainer.SettingsObjectData data = settings.GetValues()[key];
                    switch (data.type)
                    {
                        case SettingsContainer.SettingsDataType.Bool:
                            {
                                Toggle toggle = menu.CreateToggle(data.drawParameters.label, root);
                                toggle.SetIsOnWithoutNotify((bool)data.value);
                                toggle.onValueChanged.AddListener((bool value) =>
                                {
                                    settings.SetValue(key, data.type, value);
                                });
                                break;
                            }
                        case SettingsContainer.SettingsDataType.UInt:
                        case SettingsContainer.SettingsDataType.Int:
                            {
                                if (data.drawParameters.dropdownLabels != null)
                                {
                                    TMP_Dropdown dropdown = menu.CreateDropdown(data.drawParameters.label, root);
                                    dropdown.ClearOptions();

                                    List<string> labels = new List<string>();
                                    foreach(string labelKey in data.drawParameters.dropdownLabels)
                                    {
                                        labels.Add(locals != null ? locals.GetLocal(labelKey) : labelKey);
                                    }

                                    dropdown.AddOptions(labels);
                                    dropdown.SetValueWithoutNotify((int)data.value);

                                    dropdown.onValueChanged.AddListener((int value) =>
                                    {
                                        if (data.type == SettingsContainer.SettingsDataType.Int)
                                            settings.SetValue(key, data.type, value);
                                        else if (data.type == SettingsContainer.SettingsDataType.UInt)
                                            settings.SetValue(key, data.type, (uint)value);
                                    });
                                    break;
                                }
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Float:
                            {
                                Slider slider = menu.CreateSlider(data.drawParameters.label, root);
                                slider.SetValueWithoutNotify((float)data.value);
                                slider.onValueChanged.AddListener((float value) =>
                                {
                                    if(data.type == SettingsContainer.SettingsDataType.Float)
                                        settings.SetValue(key, data.type, value);
                                });
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Color:
                            {
                                Button button = menu.CreateColorPicker(data.drawParameters.label, root);
                                button.GetComponent<Image>().color = (Color)data.value;
                                button.onClick.AddListener(() =>
                                {
                                    menu.OpenColorPicker((Color)data.value, button, (Color newColor) =>
                                    {
                                        button.GetComponent<Image>().color = newColor;
                                        data.value = newColor;
                                        data.onValueChange.Invoke(newColor);
                                    });
                                });
                                break;
                            }
                        case SettingsContainer.SettingsDataType.String:
                        case SettingsContainer.SettingsDataType.Vector2:
                        case SettingsContainer.SettingsDataType.Vector3:
                        case SettingsContainer.SettingsDataType.Vector4:
                            break;
                    }
                }
            }


        }

        public override void RedrawLocalizedElements()
        {
        }
    }

}
