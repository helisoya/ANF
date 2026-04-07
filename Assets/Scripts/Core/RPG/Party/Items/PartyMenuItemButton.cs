using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a button in the items tab of the party menu
/// </summary>
public class PartyMenuItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private LocalizedTextAdditive itemText;
    [SerializeField] private Button button;
    private RPGItem linkedItem;
    private PartyMenuItemsTab menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="amount">The amount in inventory</param>
    /// <param name="menu">The root tab</param>
    public void Init(RPGItem item, int amount, PartyMenuItemsTab menu)
    {
        this.linkedItem = item;
        this.menu = menu;
        button.interactable = item.type == RPGItem.ItemType.USABLE_ALL;

        itemText.SetValue(Locals.GetLocal(item.ID + "_name"), amount, true);
    }

    /// <summary>
    /// On Click Event
    /// </summary>
    public void Click()
    {
        menu.SelectItem(linkedItem);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menu.SetTooltip(linkedItem.ID + "_desc");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menu.SetTooltip(null);
    }
}
