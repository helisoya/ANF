using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an RPG Item
/// </summary>
[CreateAssetMenu(menuName = "LJDN/RPG Item")]
public class RPGItem : ScriptableObject
{
    public string ID;
    public int sellValue;
    public ItemType type;
    public TargetType targetType;
    public DamageType damageType;
    public EquationType attackEquation;
    public EquationType defenseEquation;
    public float attackValue;
    public float defenseValue;
    public float costSP;
    public string animationName;
    public string audioName;
    public RPGCharacterData.StatusType linkedStatus;
    public StatusEffect statusEffect;
    public int statusLength;
    public float statusChance;
    public WeaponType weaponType;
    public int statsValue;
    public string[] weaponSkills;
    public string linkedAnimatior;

    public enum TargetType
    {
        ALL,
        SELF,
        ONEALLY,
        ALLALLY,
        ONEFOE,
        ALLFOE
    }

    public enum WeaponType
    {
        ONEHANDED,
        TWOHANDED,
        GUN
    }

    public enum EquationType
    {
        MULTIPLY,
        REPLACE
    }

    public enum DamageType
    {
        DAMAGE,
        HEAL,
        HEAL_DEAD
    }

    public enum ItemType
    {
        NO_USE,
        USABLE_ALL,
        USABLE_COMBAT,
        WEAPON,
        ARMOR
    }

    public enum StatusEffect
    {
        NOTHING,
        INFLICT,
        CURE
    }
}

/// <summary>
/// Represents an inventory slot
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public string itemID;
    public int itemAmount;
}
