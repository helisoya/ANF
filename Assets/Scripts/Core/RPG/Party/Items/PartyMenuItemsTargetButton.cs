using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a target button in the items tab of the party menu
/// </summary>
public class PartyMenuItemsTargetButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;
    private List<RPGCharacter> linkedData;
    private PartyMenuItemsTab menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="menu">The root tab</param>
    public void Init(List<RPGCharacter> data, PartyMenuItemsTab menu)
    {
        this.linkedData = data;
        this.menu = menu;

        string text = "";
        if (data.Count == 1)
        {
            text = data[0].GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(data[0].GetData().ID + "_name");
        }
        else
        {
            text = Locals.GetLocal("battle_target_multiple");
        }
        targetText.text = text;
    }

    /// <summary>
    /// On Click Event
    /// </summary>
    public void Click()
    {
        menu.UseItemOnTarget(linkedData);
    }
}
