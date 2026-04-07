
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents the items tab in the party menu
/// </summary>
public class PartyMenuItemsTab : PartyMenuTab
{
    [Header("Items")]
    [SerializeField] private Transform itemsRoot;
    [SerializeField] private PartyMenuItemButton prefabItemButton;

    [Header("Use")]
    [SerializeField] private GameObject useRoot;
    [SerializeField] private Transform useTextRoot;
    [SerializeField] private TextMeshProUGUI prefabUseText;
    [SerializeField] private Transform useTargetRoot;
    [SerializeField] private PartyMenuItemsTargetButton prefabUseTarget;

    private RPGItem currentItem;


    protected override void OnOpen()
    {
        SelectType(false);
    }

    protected override void OnClose()
    {
        foreach (Transform child in itemsRoot) Destroy(child.gameObject);
    }

    /// <summary>
    /// Selects a new type of items
    /// </summary>
    /// <param name="key">The new type</param>
    public void SelectType(bool key)
    {
        foreach (Transform child in itemsRoot) Destroy(child.gameObject);
        List<InventorySlot> inventory = GameManager.GetRPGManager().GetInventory();

        RPGItem item;
        int amount = 0;
        foreach (InventorySlot slot in inventory)
        {
            item = GameManager.GetRPGManager().GetItem(slot.itemID);
            if ((key && item.type == RPGItem.ItemType.NO_USE) ||
                (!key && (item.type == RPGItem.ItemType.USABLE_ALL || item.type == RPGItem.ItemType.USABLE_COMBAT)))
            {
                amount++;
                Instantiate(prefabItemButton, itemsRoot).Init(item, slot.itemAmount, this);
            }
        }

        itemsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            itemsRoot.GetComponent<RectTransform>().sizeDelta.x,
            prefabItemButton.GetComponent<RectTransform>().sizeDelta.y * amount + 15 * amount
        );
    }

    /// <summary>
    /// Select an item to use (Must be usable outside of combat)
    /// </summary>
    /// <param name="item">The item</param>
    public void SelectItem(RPGItem item)
    {
        currentItem = item;
        useRoot.SetActive(true);

        foreach (Transform child in useTextRoot) Destroy(child.gameObject);
        RPGCharacter character;
        foreach (int idx in GameManager.GetRPGManager().GetFollowers())
        {
            character = GameManager.GetRPGManager().GetCharacter(idx);
            Instantiate(prefabUseText, useTextRoot).text = character.currentHealth + "/" + character.maxHealth;
        }

        List<List<RPGCharacter>> targets = GetAvailableTargets(item);
        foreach (Transform child in useTargetRoot) Destroy(child.gameObject);
        foreach (List<RPGCharacter> target in targets)
        {
            Instantiate(prefabUseTarget, useTargetRoot).Init(target, this);
        }
    }

    /// <summary>
    /// Uses an item on targets
    /// </summary>
    /// <param name="characters">The targets</param>
    public void UseItemOnTarget(List<RPGCharacter> characters)
    {
        foreach (RPGCharacter target in characters)
        {
            target.AddHealth((int)currentItem.attackValue);
        }

        GameManager.GetRPGManager().AddItemToInventory(currentItem.ID, -1);
        CloseUseTab();
        SelectType(currentItem.type == RPGItem.ItemType.NO_USE);
        currentItem = null;
    }

    /// <summary>
    /// Closes the use tab
    /// </summary>
    public void CloseUseTab()
    {
        useRoot.SetActive(false);
    }


    /// <summary>
    /// Gets available targets using an item 
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The targets</returns>
    public List<List<RPGCharacter>> GetAvailableTargets(RPGItem item)
    {
        List<List<RPGCharacter>> result = new List<List<RPGCharacter>>();

        List<RPGCharacter> allies = new List<RPGCharacter>();
        foreach (int idx in GameManager.GetRPGManager().GetFollowers())
        {
            allies.Add(GameManager.GetRPGManager().GetCharacter(idx));
        }

        List<RPGCharacter> current;
        switch (item.targetType)
        {
            case RPGItem.TargetType.ALL:
                current = new List<RPGCharacter>();
                foreach (RPGCharacter character in allies) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.ONEALLY:
                foreach (RPGCharacter character in allies)
                {
                    current = new List<RPGCharacter>();
                    current.Add(character);
                    result.Add(current);
                }
                break;
            case RPGItem.TargetType.ALLALLY:
                current = new List<RPGCharacter>();
                foreach (RPGCharacter character in allies) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
        }
        return result;
    }
}
