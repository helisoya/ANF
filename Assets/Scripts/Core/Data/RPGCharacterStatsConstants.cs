using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an RPG Character's stats constants
/// </summary>
[CreateAssetMenu(fileName = "StatsConstants", menuName = "LJDN/Stats Constants")]
public class RPGCharacterStatsConstants : ScriptableObject
{
    public int HEALTH_BASE = 25;
    public float HEALTH_MULT_LEVEL = 5;
    public float HEALTH_MULT_BONUS = 2.5f;

    public int ATTACK_BASE = 5;
    public float ATTACK_MULT_LEVEL = 5.0f;
    public float ATTACK_MULT_FORCE = 10.0f;

    public float EVASION_BASE = 0.05f;
    public float EVASION_MULT_AGILITY = 105.0f;

    public int SP_BASE = 5;
    public float SP_MULT_LEVEL = 1.25f;
    public float SP_MULT_STRATEGY = 0.75f;

    public float PRICE_BASE = 1.0f;
    public float PRICE_MULT_CHARISMA = 175.0f;

    public int DEFENSE_BASE = 2;
    public float DEFENSE_MULT_LEVEL = 6f;
    public float DEFENSE_MULT_RESILIENCE = 12.0f;

    public int EXP_BASE = 20;
}
