using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents the level up menu
/// </summary>
public class LevelUpMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject root;
    [SerializeField] private LineData[] lines;
    [SerializeField] private LocalizedTextAdditive levelUpTitleText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private LocalizedTextAdditive remainingPointsText;
    [SerializeField] private GameObject confirmButton;

    public bool open { get { return root.activeInHierarchy; } }
    private RPGCharacter currentCharacter;

    [System.Serializable]
    public struct LineData
    {
        public BattleBarFill bar;
        public TextMeshProUGUI valueText;
        public GameObject addButton;
    }

    /// <summary>
    /// Opens the level up menu
    /// </summary>
    /// <param name="character">The character to level up</param>
    public void Open(RPGCharacter character)
    {
        currentCharacter = character;
        SetVisible(true);

        characterNameText.text = currentCharacter.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(currentCharacter.GetData().ID + "_name");

        RefreshValues(true);
        while (currentCharacter.canLevelUp)
        {
            currentCharacter.LevelUp();
        }
        RefreshValues(false);
    }

    /// <summary>
    /// Closes the level up menu
    /// </summary>
    public void Close()
    {
        currentCharacter.UpdateComputedStats();
        currentCharacter.SetHealthToMax();
        currentCharacter.SetSPToMax();
        SetVisible(false);
    }

    /// <summary>
    /// Adds a points to a category
    /// </summary>
    /// <param name="type">The category's type</param>
    public void AddPointTo(int type)
    {
        currentCharacter.availableFreePoints--;
        currentCharacter.GetData().stats[(RPGCharacterData.StatType)type]++;
        RefreshValues(false);
    }

    /// <summary>
    /// Refreshes the menu's values
    /// </summary>
    /// <param name="immediate">True if the bar's fill must be immediate</param>
    public void RefreshValues(bool immediate)
    {
        int rawStat;
        float ratio;
        for (int i = 0; i < 6; i++)
        {
            rawStat = currentCharacter.GetData().GetRawStat((RPGCharacterData.StatType)i);
            ratio = rawStat / 99.0f;
            lines[i].valueText.text = rawStat.ToString();
            lines[i].valueText.GetComponent<RectTransform>().anchoredPosition = new Vector2(400.0f * ratio, lines[i].valueText.GetComponent<RectTransform>().anchoredPosition.y);
            lines[i].bar.SetValue(ratio, immediate);

            lines[i].addButton.SetActive(rawStat < 99 && currentCharacter.availableFreePoints > 0);
        }

        if (currentCharacter.availableFreePoints == 0)
        {
            remainingPointsText.gameObject.SetActive(false);
            confirmButton.SetActive(true);
        }
        else
        {
            remainingPointsText.gameObject.SetActive(true);
            confirmButton.SetActive(false);
            remainingPointsText.SetValue(null, currentCharacter.availableFreePoints, true);
        }

        levelUpTitleText.SetValue(null, currentCharacter.GetData().level, true);
    }

    /// <summary>
    /// Changes if the menu is visible or not
    /// </summary>
    /// <param name="visible">True if visible</param>
    public void SetVisible(bool visible)
    {
        root.SetActive(visible);
    }



}
