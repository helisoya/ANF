using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a dungeon's data
/// </summary>
[CreateAssetMenu(menuName = "LJDN/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    [Header("Informations")]
    public string ID;
    public string locationID;
    public int floorsAmount;
    public string endChapter;
    public string musicName;
    public bool canExitEarly;
    public string earlyExitChapter;

    [Header("Encounters")]
    public BattleData[] encounters;
    public string battleBackground;

    [Header("Generation")]
    public Vector2Int size;
    public int roomCount;
    public Vector2Int roomMaxSize;
    public DungeonCell cellPrefab;
    public DungeonStairs stairsPrefab;
    public GameObject[] propsToPlace;
}
