using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Represents an item's button in the shop menu
/// </summary>
public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private LocalizedText label;
    private RPGItem linkedItem;
    private ShopGUI root;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The component's item</param>
    /// <param name="root">The component's root</param>
    public void Init(RPGItem item, ShopGUI root)
    {
        this.linkedItem = item;
        this.root = root;
        label.SetNewKey(item.ID + "_name");
    }

    /// <summary>
    /// Click event
    /// </summary>
    public void Click()
    {
        root.DescribeItem(linkedItem);
    }
}
