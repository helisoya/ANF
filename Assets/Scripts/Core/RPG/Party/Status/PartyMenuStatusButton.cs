using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a button in the status tab of the party menu
/// </summary>
public class PartyMenuStatusButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    private RPGCharacter linkedCharacter;
    private PartyMenuStatusTab menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="character">The character</param>
    /// <param name="menu">The root tab</param>
    public void Init(RPGCharacter character, PartyMenuStatusTab menu)
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
