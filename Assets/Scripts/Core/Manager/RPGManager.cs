using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles the game's RPG Elements
/// </summary>
public class RPGManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private RPGCharacterDataInterface[] defaultCharacters;
    private List<RPGCharacter> characters;
    private List<int> followers;

    [Header("Items")]
    [SerializeField] private string itemsPath = "RPG/Items/";
    private List<InventorySlot> inventory;
    private Dictionary<string, RPGItem> items;
    public int money { get; private set; }

    public BattleData battleData { get; private set; }
    public string battleBackground { get; private set; }
    public BattleData.CloseType battleCloseType { get; private set; }
    public string battleNextChapter { get; private set; }

    public DungeonData dungeonData { get; private set; }
    public int dungeonFloorStart { get; private set; }


    /// <summary>
    /// Initalize the manager
    /// </summary>
    public void Init()
    {
        followers = new List<int>();
        characters = new List<RPGCharacter>();
        items = new Dictionary<string, RPGItem>();
        inventory = new List<InventorySlot>();
        money = 0;
        Reset();
    }

    /// <summary>
    /// Resets the manager
    /// </summary>
    public void Reset()
    {
        money = 0;
        followers.Clear();
        followers.Add(0);
        characters.Clear();
        RPGCharacter ch;
        foreach (RPGCharacterDataInterface character in defaultCharacters)
        {
            ch = new RPGCharacter(character.data.Clone(), character.constants);
            ch.SetHealthToMax();
            ch.SetSPToMax();
            characters.Add(ch);
        }
    }

    /// <summary>
    /// Sets the current money count
    /// </summary>
    /// <param name="amount">The new money count</param>
    public void SetMoney(int amount)
    {
        money = amount;
    }

    /// <summary>
    /// Adds money
    /// </summary>
    /// <param name="amount">The amount to add</param>
    public void AddMoney(int amount)
    {
        money = Mathf.Clamp(money + amount, 0, 999999);
    }

    /// <summary>
    /// Sets the values for the next battle encounter
    /// </summary>
    /// <param name="data">The battle's data</param>
    /// <param name="backgroundName">The battle's background</param>
    /// <param name="closeType">The close type/param>
    /// <param name="nextChapter">The next chapter (VN Only)/param>
    public void SetNextBattleEncounter(BattleData data, string backgroundName, BattleData.CloseType closeType, string nextChapter)
    {
        battleData = data;
        battleBackground = backgroundName;
        battleCloseType = closeType;
        battleNextChapter = nextChapter;
    }

    /// <summary>
    /// Sets the values for the next dungeon
    /// </summary>
    /// <param name="data">The dungeon's data</param>
    /// <param name="startFloor">The dungeon's starting floor</param>
    public void SetNextDungeon(DungeonData data, int startFloor = 0)
    {
        dungeonFloorStart = startFloor;
        dungeonData = data;
    }

    /// <summary>
    /// Gets the current followers (player included)
    /// </summary>
    /// <returns>The current followers</returns>
    public List<int> GetFollowers()
    {
        return followers;
    }

    /// <summary>
    /// Sets the current followers
    /// </summary>
    /// <param name="followers">The new followers</param>
    public void SetFollowers(List<int> followers)
    {
        this.followers = followers;
    }

    /// <summary>
    /// Adds a new follower
    /// </summary>
    /// <param name="characterID">The follower's ID</param>
    public void AddFollower(string characterID)
    {
        int characterIndex = characters.FindIndex(character => character.GetData().ID.Equals(characterID));
        if (characterIndex == -1) return;

        if (!followers.Contains(characterIndex)) followers.Add(characterIndex);
    }

    /// <summary>
    /// Removes a follower
    /// </summary>
    /// <param name="characterID">The follower's ID</param>
    public void RemoveFollower(string characterID)
    {
        int characterIndex = characters.FindIndex(character => character.GetData().ID.Equals(characterID));
        if (characterIndex <= 0) return;

        if (followers.Contains(characterIndex)) followers.Remove(characterIndex);
    }

    /// <summary>
    /// Gets the game's characters
    /// </summary>
    /// <returns>The characters</returns>
    public List<RPGCharacterData> GetCharacters()
    {
        List<RPGCharacterData> datas = new List<RPGCharacterData>();
        foreach (RPGCharacter character in characters)
        {
            datas.Add(character.GetData());
        }
        return datas;
    }

    /// <summary>
    /// Gets an RPG Character 
    /// </summary>
    /// <param name="ID">The Character's ID</param>
    /// <returns>The character if it exists</returns>
    public RPGCharacter GetCharacter(string ID)
    {
        RPGCharacter character = characters.Find(c => c.GetData().ID.Equals(ID));
        if (character != null) return character;

        return null;
    }

    /// <summary>
    /// Gets an RPG Character 
    /// </summary>
    /// <param name="index">The Character's index</param>
    /// <returns>The character if it exists</returns>
    public RPGCharacter GetCharacter(int index)
    {
        if (characters.Count < index) return null;
        return characters[index];
    }

    /// <summary>
    /// Loads the game's character from a list
    /// </summary>
    /// <param name="list">The list</param>
    public void LoadCharactersFromList(List<RPGCharacterData> list)
    {
        RPGCharacter other;
        foreach (RPGCharacterData character in list)
        {
            other = characters.Find(other => other.GetData().ID.Equals(character.ID));
            if (other != null)
            {
                other.GetData().Copy(character);
                other.UpdateComputedStats();
            }
        }
    }

    /// <summary>
    /// Gets an RPG item, load to memory if needed
    /// </summary>
    /// <param name="ID">The Item's ID</param>
    /// <returns>The item</returns>
    public RPGItem GetItem(string ID)
    {
        RPGItem item;
        if (items.TryGetValue(ID, out item)) return item;

        item = Resources.Load<RPGItem>(itemsPath + ID);
        items.Add(ID, item);
        return item;
    }

    /// <summary>
    /// Gets the current inventory
    /// </summary>
    /// <returns>The inventory</returns>
    public List<InventorySlot> GetInventory()
    {
        return inventory;
    }

    /// <summary>
    /// Sets the inventory's value
    /// </summary>
    /// <param name="newInventory">The new inventory</param>
    public void SetInventory(List<InventorySlot> newInventory)
    {
        inventory = newInventory;
    }

    /// <summary>
    /// Adds an item to the inventory
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <param name="amount">The amount to add</param>
    public void AddItemToInventory(string ID, int amount)
    {
        if (amount == 0) return; // Nothing to add / remove

        int idx = inventory.FindIndex(c => c.itemID.Equals(ID));
        InventorySlot slot = idx == -1 ? null : inventory[idx];
        if (slot == null && amount < 0) return; // Not in inventory & Trying to remove
        if (slot == null)
        {
            // Not in inventory & trying to add
            slot = new InventorySlot
            {
                itemID = ID,
                itemAmount = amount
            };
            inventory.Add(slot);
        }
        else
        {
            // Adding / Removing from an existing slot
            slot.itemAmount = Mathf.Clamp(slot.itemAmount + amount, 0, 999);
            if (slot.itemAmount == 0 && amount < 0) inventory.RemoveAt(idx);
        }
    }

    /// <summary>
    /// Gets an item's amount in inventory
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <returns>The amount in inventory. 0 if not in inventory</returns>
    public int GetAmountInInventory(string ID)
    {
        InventorySlot slot = inventory.Find(c => c.itemID.Equals(ID));
        if (slot != null) return slot.itemAmount;
        return 0;
    }
}
