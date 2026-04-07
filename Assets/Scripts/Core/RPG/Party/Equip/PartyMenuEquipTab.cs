using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the equip tab in the party menu
/// </summary>
public class PartyMenuEquipTab : PartyMenuTab
{
    [Header("Equip tab")]
    [Header("Characters")]
    [SerializeField] private PartyMenuEquipCharacterButton prefabCharacterButton;
    [SerializeField] private Transform charactersRoot;

    [Header("Current Weapon/Armor")]
    [SerializeField] private LocalizedTextAdditive currentArmorText;
    [SerializeField] private LocalizedTextAdditive currentWeaponText;

    [Header("Available Items")]
    [SerializeField] private Transform availableItemsRoot;
    [SerializeField] private PartyMenuEquipItemButton prefabAvailableItem;
    private RPGCharacter currentCharacter;
    private RPGItem.ItemType currentSelectionType;

    protected override void OnOpen()
    {
        // Characters

        foreach (Transform child in charactersRoot) Destroy(child.gameObject);
        foreach (Transform child in availableItemsRoot) Destroy(child.gameObject);
        List<int> inParty = GameManager.GetRPGManager().GetFollowers();
        foreach (int idx in inParty)
        {
            Instantiate(prefabCharacterButton, charactersRoot).Init(GameManager.GetRPGManager().GetCharacter(idx), this);
        }
        SelectCharacter(GameManager.GetRPGManager().GetCharacter(inParty[0]));
    }

    protected override void OnClose()
    {
        currentCharacter = null;

        foreach (Transform child in charactersRoot) Destroy(child.gameObject);
        foreach (Transform child in availableItemsRoot) Destroy(child.gameObject);
    }

    /// <summary>
    /// Selects a new character to edit
    /// </summary>
    /// <param name="character">The character to edit</param>
    public void SelectCharacter(RPGCharacter character)
    {
        foreach (Transform child in availableItemsRoot) Destroy(child.gameObject);

        currentCharacter = character;

        bool hasWeapon = !string.IsNullOrEmpty(character.GetData().weapon);
        bool hasArmor = !string.IsNullOrEmpty(character.GetData().armor);
        currentWeaponText.SetValue(hasWeapon ? Locals.GetLocal(character.GetData().weapon + "_name") : Locals.GetLocal("party_status_none"),
            hasWeapon ? GameManager.GetRPGManager().GetItem(character.GetData().weapon).statsValue : 0,
            true);

        currentArmorText.SetValue(hasArmor ? Locals.GetLocal(character.GetData().armor + "_name") : Locals.GetLocal("party_status_none"),
            hasArmor ? GameManager.GetRPGManager().GetItem(character.GetData().armor).statsValue : 0,
            true);
    }


    /// <summary>
    /// Selects a specific item type to edit
    /// </summary>
    /// <param name="weapon">True for a weapon, false for armor</param>
    public void SelectItemOfType(bool weapon)
    {
        SelectItemOfType(weapon ? RPGItem.ItemType.WEAPON : RPGItem.ItemType.ARMOR);
    }

    /// <summary>
    /// Selects a specific item type to edit
    /// </summary>
    /// <param name="type">The type</param>
    public void SelectItemOfType(RPGItem.ItemType type)
    {
        if (!currentCharacter.GetData().canChangeEquipement) return;

        currentSelectionType = type;
        foreach (Transform child in availableItemsRoot) Destroy(child.gameObject);

        int amount = 0;
        RPGItem item;
        List<InventorySlot> inventory = GameManager.GetRPGManager().GetInventory();
        if ((type == RPGItem.ItemType.WEAPON && !string.IsNullOrEmpty(currentCharacter.GetData().weapon)) ||
            (type == RPGItem.ItemType.ARMOR && !string.IsNullOrEmpty(currentCharacter.GetData().armor)))
        {
            amount++;
            Instantiate(prefabAvailableItem, availableItemsRoot).Init(null, this);
        }

        foreach (InventorySlot slot in inventory)
        {
            item = GameManager.GetRPGManager().GetItem(slot.itemID);
            if (item.type == type &&
                ((type == RPGItem.ItemType.WEAPON && item.ID != currentCharacter.GetData().weapon) ||
                (type == RPGItem.ItemType.ARMOR && item.ID != currentCharacter.GetData().armor)))
            {
                amount++;
                Instantiate(prefabAvailableItem, availableItemsRoot).Init(item, this);
            }
        }
        availableItemsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            availableItemsRoot.GetComponent<RectTransform>().sizeDelta.x,
            prefabAvailableItem.GetComponent<RectTransform>().sizeDelta.y * amount + 15 * amount
        );
    }

    /// <summary>
    /// Selects an item
    /// </summary>
    /// <param name="item">The item to select</param>
    public void SelectItem(RPGItem item)
    {
        if (currentSelectionType == RPGItem.ItemType.WEAPON)
        {
            if (!string.IsNullOrEmpty(currentCharacter.GetData().weapon))
            {
                GameManager.GetRPGManager().AddItemToInventory(currentCharacter.GetData().weapon, 1);
            }

            if (item)
            {
                GameManager.GetRPGManager().AddItemToInventory(item.ID, -1);
                currentCharacter.GetData().weapon = item.ID;
            }
            else
            {
                currentCharacter.GetData().weapon = null;
            }

            bool hasWeapon = !string.IsNullOrEmpty(currentCharacter.GetData().weapon);
            currentWeaponText.SetValue(hasWeapon ? Locals.GetLocal(currentCharacter.GetData().weapon + "_name") : Locals.GetLocal("party_status_none"),
                hasWeapon ? GameManager.GetRPGManager().GetItem(currentCharacter.GetData().weapon).statsValue : 0,
                true);
        }
        else if (currentSelectionType == RPGItem.ItemType.ARMOR)
        {
            if (!string.IsNullOrEmpty(currentCharacter.GetData().armor))
            {
                GameManager.GetRPGManager().AddItemToInventory(currentCharacter.GetData().armor, 1);
            }

            if (item)
            {
                GameManager.GetRPGManager().AddItemToInventory(item.ID, -1);
                currentCharacter.GetData().armor = item.ID;
            }
            else
            {
                currentCharacter.GetData().armor = null;
            }

            bool hasArmor = !string.IsNullOrEmpty(currentCharacter.GetData().armor);
            currentArmorText.SetValue(hasArmor ? Locals.GetLocal(currentCharacter.GetData().armor + "_name") : Locals.GetLocal("party_status_none"),
                hasArmor ? GameManager.GetRPGManager().GetItem(currentCharacter.GetData().armor).statsValue : 0,
                true);
        }
        currentCharacter.UpdateComputedStats();
        SelectItemOfType(currentSelectionType);
    }
}
