using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a value entry for a weapon / armor in the shop menu
/// </summary>
public class ShopValueEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private LocalizedTextAdditive valuesText;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="character">The character's name</param>
    /// <param name="from">The character's value</param>
    /// <param name="to">The character's value if using the item</param>
    public void Init(string character, int from, int to)
    {
        characterNameText.text = character;
        valuesText.SetValue(from, to, true);
        valuesText.GetText().color = from == to ? Color.black : (from < to ? new Color(0, 0.5f, 0) : new Color(0.5f, 0, 0));
    }
}
