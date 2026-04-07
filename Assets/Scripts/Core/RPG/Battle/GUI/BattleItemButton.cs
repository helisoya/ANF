using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Represents an item button in the battle GUI
/// </summary>
public class BattleItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private LocalizedTextAdditive itemNameText;
    private BattleManager.ItemData linkedItem;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The linked item</param>
    /// <param name="gui">The gui</param>
    public void Init(BattleManager.ItemData item, BattleGUI gui)
    {
        this.linkedItem = item;
        this.gui = gui;

        Refresh();
    }

    /// <summary>
    /// Refreshs the component
    /// </summary>
    public void Refresh()
    {
        itemNameText.SetValue(null, linkedItem.amountInInventory, false);
        itemNameText.SetNewKey(linkedItem.item.ID + "_name");
    }

    /// <summary>
    /// Gets the button's linked item
    /// </summary>
    /// <returns>Its linked item</returns>
    public BattleManager.ItemData GetLinkedItem()
    {
        return linkedItem;
    }

    public void Click()
    {
        gui.SelectItem(linkedItem);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gui.SetItemDescription(linkedItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gui.SetItemDescription(null);
    }
}
