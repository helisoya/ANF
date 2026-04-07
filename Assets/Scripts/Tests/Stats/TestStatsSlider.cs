using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestStatsSlider : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Slider slider;

    public int value
    {
        get
        {
            return (int)slider.value;
        }
        set
        {
            slider.SetValueWithoutNotify(value);
            UpdateText();
        }
    }

    public void UpdateText()
    {
        valueText.text = value.ToString();
    }
}
