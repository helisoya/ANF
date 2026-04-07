using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a dungeon cell
/// </summary>
public class DungeonCell : MonoBehaviour
{
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;

    /// <summary>
    /// Initialize the cell
    /// </summary>
    /// <param name="leftOpen">Is the left wall open ?</param>
    /// <param name="rightOpen">Is the right wall open ?</param>
    /// <param name="topOpen">Is the top wall open ?</param>
    /// <param name="bottomOpen">Is the bottom wall open ?</param>
    public void Init(bool leftOpen, bool rightOpen, bool topOpen, bool bottomOpen)
    {
        if (leftOpen) Destroy(leftWall.gameObject);
        if (rightOpen) Destroy(rightWall.gameObject);
        if (topOpen) Destroy(topWall.gameObject);
        if (bottomOpen) Destroy(bottomWall.gameObject);
    }
}
