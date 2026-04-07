using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents the shop's GUI
/// </summary>
public class ShopGUI : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;
    [SerializeField] private LocalizedTextAdditive goldText;
    [SerializeField] private RectTransform buttonsRoot;
    [SerializeField] private ShopItemButton prefabButton;


    [Header("Description")]
    [SerializeField] private GameObject descriptionRoot;
    [SerializeField] private LocalizedText itemName;
    [SerializeField] private LocalizedTextAdditive itemInInventory;
    [SerializeField] private LocalizedTextAdditive itemPrice;
    [SerializeField] private LocalizedText itemDescription;
    [SerializeField] private Transform valuesRoot;
    [SerializeField] private ShopValueEntry prefabValue;

    public bool open { get { return root.activeInHierarchy; } }

    private RPGItem[] items;
    private RPGItem currentItem;

    /// <summary>
    /// Opens the shop
    /// </summary>
    /// <param name="data">The shop's data</param>
    public void Open(ShopData data)
    {
        items = new RPGItem[data.itemsToSell.Length];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = GameManager.GetRPGManager().GetItem(data.itemsToSell[i]);
        }
        ShowItemsOfType(RPGItem.ItemType.WEAPON);
        UpdateGoldUI();
        root.SetActive(true);
    }

    /// <summary>
    /// Closes the shop
    /// </summary>
    public void Close()
    {
        currentItem = null;
        root.SetActive(false);
    }

    /// <summary>
    /// Updates the gold UI
    /// </summary>
    private void UpdateGoldUI()
    {
        goldText.SetValue(null, GameManager.GetRPGManager().money, true);
    }

    /// <summary>
    /// Show the buyable items of a certain type
    /// </summary>
    /// <param name="type">The item's type</param>
    public void ShowItemsOfType(int type)
    {
        ShowItemsOfType((RPGItem.ItemType)type);
    }

    /// <summary>
    /// Show the buyable items of a certain type
    /// </summary>
    /// <param name="type">The item's type</param>
    public void ShowItemsOfType(RPGItem.ItemType type)
    {
        int amount = 0;
        foreach (Transform child in buttonsRoot) Destroy(child.gameObject);

        foreach (RPGItem item in items)
        {
            if (item.type == type || (type == RPGItem.ItemType.NO_USE && item.type != RPGItem.ItemType.WEAPON && item.type != RPGItem.ItemType.ARMOR))
            {
                Instantiate(prefabButton, buttonsRoot).Init(item, this);
                amount++;
                if (amount == 1) DescribeItem(item);
            }
        }

        if (amount == 0)
        {
            descriptionRoot.SetActive(false);
        }

        buttonsRoot.sizeDelta = new Vector2(
        buttonsRoot.sizeDelta.x,
        amount * prefabButton.GetComponent<RectTransform>().sizeDelta.y + 5 * amount
        );

    }

    /// <summary>
    /// Describes an item on GUI
    /// </summary>
    /// <param name="item">The item</param>
    public void DescribeItem(RPGItem item)
    {
        descriptionRoot.SetActive(true);
        currentItem = item;
        itemName.SetNewKey(item.ID + "_name");
        itemDescription.SetNewKey(item.ID + "_desc");
        itemInInventory.SetValue(null, GameManager.GetRPGManager().GetAmountInInventory(item.ID), true);

        itemPrice.SetValue(null, item.sellValue, true);
        itemPrice.GetText().color = (GameManager.GetRPGManager().money >= currentItem.sellValue) ? Color.white : Color.red;

        foreach (Transform child in valuesRoot) Destroy(child.gameObject);
        if (item.type == RPGItem.ItemType.WEAPON || item.type == RPGItem.ItemType.ARMOR)
        {
            List<int> inParty = GameManager.GetRPGManager().GetFollowers();
            RPGCharacter character;
            foreach (int idx in inParty)
            {
                character = GameManager.GetRPGManager().GetCharacter(inParty[idx]);

                string itemToGet = item.type == RPGItem.ItemType.WEAPON ? character.GetData().weapon : character.GetData().armor;
                int currentValue = 0;
                if (!string.IsNullOrEmpty(itemToGet))
                {
                    currentValue = GameManager.GetRPGManager().GetItem(itemToGet).statsValue;
                }

                Instantiate(prefabValue, valuesRoot).Init(
                    character.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(character.GetData().ID + "_name"),
                    currentValue,
                    item.statsValue);
            }
        }
    }

    /// <summary>
    /// Buys the current item
    /// </summary>
    public void BuyCurrentItem()
    {
        if (currentItem != null && GameManager.GetRPGManager().money >= currentItem.sellValue)
        {
            GameManager.GetRPGManager().AddMoney(-currentItem.sellValue);
            GameManager.GetRPGManager().AddItemToInventory(currentItem.ID, 1);

            itemInInventory.SetValue(null, GameManager.GetRPGManager().GetAmountInInventory(currentItem.ID), true);
            UpdateGoldUI();
        }
    }
}
