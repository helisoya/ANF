using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// Represents a RPG Character
/// </summary>
public class RPGCharacter
{
    private RPGCharacterData baseData;
    private RPGCharacterStatsConstants constants;

    public int currentHealth { get { return baseData.currentHP; } }
    public int currentSP { get { return baseData.currentSP; } }
    public int maxHealth { get; private set; }
    public int attack { get; private set; }
    public float evasion { get; private set; }
    public int maxSP { get; private set; }
    public float priceMultiplier { get; private set; }
    public int defense { get; private set; }
    public int nextExpCap { get; private set; }
    public bool canLevelUp { get { return baseData.exp >= nextExpCap; } }

    public int availableFreePoints = 0;

    public RPGCharacter(RPGCharacterData data, RPGCharacterStatsConstants constants)
    {
        this.constants = constants;
        SetData(data);
    }

    /// <summary>
    /// Gets the character's savable data
    /// </summary>
    /// <returns>The savable data</returns>
    public RPGCharacterData GetData()
    {
        return baseData;
    }

    /// <summary>
    /// Gets the character's constants
    /// </summary>
    /// <returns>The constants</returns>
    public RPGCharacterStatsConstants GetConstants()
    {
        return constants;
    }

    /// <summary>
    /// Sets the character's data
    /// </summary>
    /// <param name="data">The data</param>
    public void SetData(RPGCharacterData data)
    {
        baseData = data;
        UpdateComputedStats();
    }

    /// <summary>
    /// Update the computed stats
    /// </summary>
    public void UpdateComputedStats()
    {
        // Heath = BASE + LVL * MULT_LVL + BONUS * MULT_BONUS
        // Attack = BASE + (1.0f + LVL / MULT_LVL + FORCE / MULT_FORCE) * WEAPON_ATTACK
        // Evasion = BASE + AGILITY / MULT_AGILITY
        // SP = BASE + LVL * MULT_LEVEL + STRATEGY * MULT_STRATEGY
        // DEFENSE = BASE + (1.0f + LVL / MULT_LVL + RESILIENCE / MULT_RESILIENCE) * ARMOR_VALUE
        // Price = BASE - CHARISMA * MULT_CHARISMA
        // Next EXP = BASE + (LVL*2)**2

        maxHealth = constants.HEALTH_BASE + Mathf.FloorToInt(constants.HEALTH_MULT_LEVEL * baseData.level + constants.HEALTH_MULT_BONUS * baseData.GetRawStat(RPGCharacterData.StatType.BONUSHP));
        attack = Mathf.FloorToInt(
            (1.0f + baseData.GetRawStat(RPGCharacterData.StatType.FORCE) / constants.ATTACK_MULT_FORCE + baseData.level / constants.ATTACK_MULT_LEVEL)
            * (constants.ATTACK_BASE + (string.IsNullOrEmpty(baseData.weapon) ? 0.0f : GameManager.GetRPGManager().GetItem(baseData.weapon).statsValue)));
        evasion = constants.EVASION_BASE + baseData.GetRawStat(RPGCharacterData.StatType.AGILITY) / constants.EVASION_MULT_AGILITY;
        maxSP = constants.SP_BASE + Mathf.FloorToInt(baseData.level * constants.SP_MULT_LEVEL + baseData.GetRawStat(RPGCharacterData.StatType.STRATEGY) * constants.SP_MULT_STRATEGY);
        defense = Mathf.FloorToInt(
            (1.0f + baseData.GetRawStat(RPGCharacterData.StatType.RESILIENCE) / constants.DEFENSE_MULT_RESILIENCE + baseData.level / constants.DEFENSE_MULT_LEVEL)
            * (constants.DEFENSE_BASE + (string.IsNullOrEmpty(baseData.armor) ? 0.0f : GameManager.GetRPGManager().GetItem(baseData.armor).statsValue)));
        priceMultiplier = constants.PRICE_BASE - baseData.GetRawStat(RPGCharacterData.StatType.CHARISMA) / constants.PRICE_MULT_CHARISMA;
        nextExpCap = constants.EXP_BASE + Mathf.FloorToInt(Mathf.Pow(baseData.level * 2, 2));

        attack = Mathf.Clamp(attack, constants.ATTACK_BASE, 999);
        defense = Mathf.Clamp(defense, constants.DEFENSE_BASE, 999);
        maxHealth = Mathf.Clamp(maxHealth, constants.HEALTH_BASE, 999);
        evasion = Mathf.Clamp(evasion, constants.EVASION_BASE, 0.8f);
        maxSP = Mathf.Clamp(maxSP, constants.SP_BASE, 999);
        priceMultiplier = Mathf.Clamp(priceMultiplier, 0.1f, constants.PRICE_BASE);
        nextExpCap = Mathf.Clamp(nextExpCap, constants.EXP_BASE, 999999);
    }

    /// <summary>
    /// Sets the character's health to max
    /// </summary>
    public void SetHealthToMax()
    {
        baseData.currentHP = maxHealth;
    }

    /// <summary>
    /// Set the character's SP to max
    /// </summary>
    public void SetSPToMax()
    {
        baseData.currentSP = maxSP;
    }

    /// <summary>
    /// Adds health
    /// </summary>
    /// <param name="amount">The amount to add</param>
    public void AddHealth(int amount)
    {
        baseData.currentHP = Mathf.Clamp(baseData.currentHP + amount, 0, maxHealth);
    }

    /// <summary>
    /// Adds SP
    /// </summary>
    /// <param name="amount">The amount of SP to add</param>
    public void AddSP(int amount)
    {
        baseData.currentSP = Mathf.Clamp(baseData.currentSP + amount, 0, maxSP);
    }

    /// <summary>
    /// Level up the character
    /// </summary>
    /// <param name="awardFreePointsNow">Should the free points be awarded now (random distribution) ?</param>
    public void LevelUp(bool awardFreePointsNow = false)
    {
        if (baseData.level >= 99) return;

        baseData.level++;
        baseData.exp -= nextExpCap;



        List<int> possibilities = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (baseData.stats[(RPGCharacterData.StatType)i] < 99) possibilities.Add(i);
        }


        int awarded = 0;
        bool canContinue = possibilities.Count > 0;
        int maxToAward = 4 + (awardFreePointsNow ? 2 : 0);

        while (awarded < maxToAward && canContinue)
        {
            int selectedIdx = Random.Range(0, possibilities.Count);

            int selectedStat = possibilities[selectedIdx];
            baseData.stats[(RPGCharacterData.StatType)selectedStat]++;
            if (baseData.stats[(RPGCharacterData.StatType)selectedStat] >= 99)
            {
                possibilities.RemoveAt(selectedIdx);
                if (possibilities.Count == 0) canContinue = false;
            }

            awarded++;
        }

        if (!awardFreePointsNow) availableFreePoints += 2;

        UpdateComputedStats();
        SetHealthToMax();
        SetSPToMax();
    }

    /// <summary>
    /// Level up the character to a target level
    /// </summary>
    /// <param name="targetLevel">The target level</param>
    public void LevelUpTo(int targetLevel)
    {
        while (baseData.level < targetLevel)
        {
            baseData.exp = nextExpCap;
            LevelUp(true);
        }
    }

    /// <summary>
    /// Increase a stat by a set amount (cannot go over 99)
    /// </summary>
    /// <param name="stat">The stat/param>
    /// <param name="amount">The increase amount</param>
    public void IncreaseStat(RPGCharacterData.StatType stat, int amount)
    {
        baseData.stats[stat] = Mathf.Clamp(baseData.stats[stat] + amount, 0, 99);
    }
}
