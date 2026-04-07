using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an interface for a RPG Character's data
/// </summary>
[CreateAssetMenu(menuName = "LJDN/RPG Character's Data")]
public class RPGCharacterDataInterface : ScriptableObject
{
    public RPGCharacterData data;
    public RPGCharacterStatsConstants constants;
}
