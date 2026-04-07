using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the map's tooltip
/// </summary>
public class MapInfo : MonoBehaviour
{
    [SerializeField] private LocalizedText text;
    private Vector2 size;

    /// <summary>
    /// Updates the tooltip's informations
    /// </summary>
    /// <param name="show">Is the tooltip visible ?</param>
    /// <param name="key">The tooltip's key</param>
    public void UpdateInfo(bool show, string key)
    {
        gameObject.SetActive(show);
        text.SetNewKey(key);

        if (show == true) ComputePosition();
    }

    /// <summary>
    /// Computes the label's position
    /// </summary>
    void ComputePosition()
    {
        Vector2 position = Input.mousePosition;
        if (position.x >= Screen.width - size.x) position.x -= size.x;
        if (position.y < size.y) position.y += size.y;
        transform.position = position;
    }

    void Start()
    {
        size = GetComponent<RectTransform>().sizeDelta;
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            ComputePosition();
        }
    }
}
