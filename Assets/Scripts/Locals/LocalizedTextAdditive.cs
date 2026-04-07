using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

/// <summary>
/// A localized text that can append a value to the text
/// </summary>
public class LocalizedTextAdditive : LocalizedText
{
    [SerializeField] private string suffixPrefix;
    [SerializeField] private string suffixSuffix;
    [SerializeField] private string prefixPrefix;
    [SerializeField] private string prefixSuffix;
    private object prefixValue;
    private object suffixValue;

    /// <summary>
    /// Sets a new value
    /// </summary>
    /// <param name="prefix">The new prefix</param>
    /// <param name="suffix">The new suffix</param>
    /// <param name="reload">Should the text be reloaded ?</param>
    public void SetValue(object prefix, object suffix, bool reload)
    {
        prefixValue = prefix;
        suffixValue = suffix;
        if (reload) ReloadText();
    }

    public override void ReloadText()
    {
        base.ReloadText();

        if (suffixValue != null)
        {
            string value = suffixPrefix + suffixValue + suffixSuffix;
            text.text += value;
        }

        if (prefixValue != null)
        {
            string value = prefixPrefix + prefixValue + prefixSuffix;
            text.text = value + text.text;
        }
    }

    /// <summary>
    /// Changes the additive parameters
    /// </summary>
    /// <param name="prefix">The value's prefix</param>
    /// <param name="suffix">The value's suffix</param>
    public void SetParameters(string prefixPrefix, string prefixSuffix, string suffixPrefix, string suffixSuffix)
    {
        ;
        this.prefixPrefix = prefixPrefix;
        this.prefixSuffix = prefixSuffix;
        this.suffixPrefix = suffixPrefix;
        this.suffixSuffix = suffixSuffix;
    }

    /// <summary>
    /// Reset the additive component
    /// </summary>
    public void ResetAdditive()
    {
        prefixPrefix = "";
        prefixSuffix = "";
        suffixPrefix = "";
        suffixSuffix = "";
        suffixValue = null;
        prefixValue = null;
    }
}
