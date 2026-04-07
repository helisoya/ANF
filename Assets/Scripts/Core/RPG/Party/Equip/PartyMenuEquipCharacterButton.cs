using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a character button in the equip tab of the party menu
/// </summary>
public class PartyMenuEquipCharacterButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    private RPGCharacter linkedCharacter;
    private PartyMenuEquipTab menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="character">The character</param>
    /// <param name="menu">The root tab</param>
    public void Init(RPGCharacter character, PartyMenuEquipTab menu)
    {
        this.linkedCharacter = character;
        this.menu = menu;

        characterNameText.text = character.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(character.GetData().ID + "_name");
    }

    /// <summary>
    /// On Click Event
    /// </summary>
    public void Click()
    {
        menu.SelectCharacter(linkedCharacter);
    }
}
