using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TestStatsManager : MonoBehaviour
{
    [SerializeField] private RPGCharacterDataInterface data;
    [SerializeField] private RPGCharacterStatsConstants constants;

    [Header("Inputs")]
    [SerializeField] private TestStatsSlider levelSlider;
    [SerializeField] private TestStatsSlider forceSlider;
    [SerializeField] private TestStatsSlider agilitySlider;
    [SerializeField] private TestStatsSlider charismaSlider;
    [SerializeField] private TestStatsSlider resilienceSlider;
    [SerializeField] private TestStatsSlider strategySlider;
    [SerializeField] private TestStatsSlider bonusHPSlider;
    [SerializeField] private RPGItem weapon;
    [SerializeField] private RPGItem armor;

    [Header("Output")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI maxHPText;
    [SerializeField] private TextMeshProUGUI maxSPText;
    [SerializeField] private TextMeshProUGUI evasionText;
    [SerializeField] private TextMeshProUGUI priceText;
    private RPGCharacter character;

    void Start()
    {
        character = new RPGCharacter(data.data.Clone(), constants);
        levelSlider.value = character.GetData().level;
        forceSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.FORCE);
        agilitySlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.AGILITY);
        charismaSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.CHARISMA);
        resilienceSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.RESILIENCE);
        strategySlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.STRATEGY);
        bonusHPSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.BONUSHP);
        UpdateAll();
    }

    public void LevelUp()
    {
        character.GetData().exp += character.nextExpCap;
        character.LevelUp();

        levelSlider.value = character.GetData().level;
        forceSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.FORCE);
        agilitySlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.AGILITY);
        charismaSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.CHARISMA);
        resilienceSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.RESILIENCE);
        strategySlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.STRATEGY);
        bonusHPSlider.value = character.GetData().GetRawStat(RPGCharacterData.StatType.BONUSHP);

        UpdateAll();
    }

    public void UpdateAll()
    {
        character.GetData().weapon = weapon == null ? null : weapon.ID;
        character.GetData().armor = armor == null ? null : armor.ID;
        character.GetData().stats[RPGCharacterData.StatType.FORCE] = forceSlider.value;
        character.GetData().stats[RPGCharacterData.StatType.AGILITY] = agilitySlider.value;
        character.GetData().stats[RPGCharacterData.StatType.CHARISMA] = charismaSlider.value;
        character.GetData().stats[RPGCharacterData.StatType.RESILIENCE] = resilienceSlider.value;
        character.GetData().stats[RPGCharacterData.StatType.STRATEGY] = strategySlider.value;
        character.GetData().stats[RPGCharacterData.StatType.BONUSHP] = bonusHPSlider.value;
        character.GetData().level = levelSlider.value;

        character.UpdateComputedStats();

        levelSlider.UpdateText();
        forceSlider.UpdateText();
        agilitySlider.UpdateText();
        charismaSlider.UpdateText();
        resilienceSlider.UpdateText();
        strategySlider.UpdateText();
        bonusHPSlider.UpdateText();

        attackText.text = "Attack : " + character.attack;
        defenseText.text = "Defense : " + character.defense;
        maxHPText.text = "Max HP : " + character.maxHealth;
        maxSPText.text = "Max SP : " + character.maxSP;
        evasionText.text = "Evasion : " + character.evasion;
        priceText.text = "Price reduction : " + character.priceMultiplier;

    }


}
