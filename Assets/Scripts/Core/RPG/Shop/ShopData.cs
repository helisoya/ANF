using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a shop's data
/// </summary>
[CreateAssetMenu(menuName = "LJDN/Shop Data")]
public class ShopData : ScriptableObject
{
    public string[] itemsToSell;
}
