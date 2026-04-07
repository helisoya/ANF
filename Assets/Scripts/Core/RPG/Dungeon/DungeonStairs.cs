using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the stairs in the dungeon
/// </summary>
public class DungeonStairs : MonoBehaviour
{
    public bool active { get; set; }
    void OnTriggerEnter(Collider collider)
    {
        if (active && collider.tag == "Player")
        {
            active = false;
            DungeonManager.instance.NextFloor();
        }
    }
}
