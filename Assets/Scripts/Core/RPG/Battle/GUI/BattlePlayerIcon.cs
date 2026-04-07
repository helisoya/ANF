using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a player icon in battle
/// </summary>
public class BattlePlayerIcon : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private BattleBarFill healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private BattleBarFill spFill;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private Animator animator;
    [SerializeField] private BattleStatusIconHandler statusIcons;

    public bool fillingSP { get { return spFill.filling; } }
    public bool fillingHealth { get { return healthFill.filling; } }

    private RPGCharacter data;

    /// <summary>
    /// Gets the component's character ID
    /// </summary>
    /// <returns>Its character ID</returns>
    public string GetID()
    {
        return data.GetData().ID;
    }

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="data">Its linked data</param>
    public void Init(BattleManager.CharacterData data, SerializedDictionary<RPGCharacterData.StatusType, Sprite> sprites)
    {
        Init(data.characterData, sprites);
    }

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="data">Its linked data</param>
    public void Init(RPGCharacter data, SerializedDictionary<RPGCharacterData.StatusType, Sprite> sprites)
    {
        this.data = data;
        playerIcon.sprite = Resources.Load<Sprite>("RPG/Battles/Icons/" + data.GetData().modelID);
        statusIcons.Init(sprites);
        UpdateIcon(true);
    }

    /// <summary>
    /// Adds a new status icon
    /// </summary>
    /// <param name="status">The status</param>
    public void AddStatusIcon(RPGCharacterData.StatusType status)
    {
        statusIcons.AddIcon(status);
    }

    /// <summary>
    /// Updates a status icon
    /// </summary>
    /// <param name="status">The icon's status</param>
    /// <param name="target">The target</param>
    /// <param name="immediate">True </param>
    public void UpdateStatusIcon(RPGCharacterData.StatusType status, float target, bool immediate)
    {
        statusIcons.UpdateIcon(status, target, immediate);
    }

    /// <summary>
    /// Update the component
    /// </summary>
    /// <param name="immediateForBars">True if the health bar changes should be immediate</param>
    public void UpdateIcon(bool immediateForBars = false)
    {
        healthFill.SetValue((float)data.currentHealth / data.maxHealth, immediateForBars);
        spFill.SetValue((float)data.currentSP / data.maxSP, immediateForBars);
        healthText.text = data.currentHealth + "/" + data.maxHealth;
        spText.text = data.currentSP + "/" + data.maxSP;
    }

    /// <summary>
    /// Changes if the icon is focused on or not
    /// </summary>
    /// <param name="value">True if focused</param>
    public void SetFocus(bool value)
    {
        animator.SetBool("Selected", value);
    }
}
