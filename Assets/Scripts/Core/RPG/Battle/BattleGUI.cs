using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the GUI in a battle
/// </summary>
public class BattleGUI : MonoBehaviour
{
    [Header("Data")]
    public BattleManager manager;

    [Header("General")]
    [SerializeField] private LocalizedTextAdditive actionText;
    [SerializeField] private Fade fade;
    [SerializeField] private LevelUpMenu levelUp;

    public bool fading { get { return fade.fading; } }
    public bool levelingUp { get { return levelUp.open; } }

    [Header("Player Icons")]
    [SerializeField] private BattlePlayerIcon prefabIcon;
    [SerializeField] private Transform iconsRoot;
    private List<BattlePlayerIcon> playerIcons;

    [Header("Skills")]
    [SerializeField] private BattleSkillButton prefabSkillButton;
    [SerializeField] private Transform skillButtonsRoot;
    [SerializeField] private GameObject skillDescRoot;
    [SerializeField] private LocalizedText skillDescText;
    [SerializeField] private LocalizedText skillNameText;

    [Header("Items")]
    [SerializeField] private BattleItemButton prefabItemButton;
    [SerializeField] private Transform itemsButtonsRoot;
    [SerializeField] private GameObject itemDescRoot;
    [SerializeField] private LocalizedText itemDescText;
    [SerializeField] private LocalizedTextAdditive itemNameText;

    [Header("Targets")]
    [SerializeField] private BattleTargetButton prefabTargetButton;
    [SerializeField] private Transform targetButtonsRoot;

    [Header("Screens")]
    [SerializeField] private GameObject playerScreen;
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject targetScreen;
    [SerializeField] private GameObject skillsScreen;
    [SerializeField] private GameObject itemsScreen;

    private RPGItem currentItem;
    private BattleManager.ItemData currentItemData;
    private bool usingSkill;

    void Awake()
    {
        playerIcons = new List<BattlePlayerIcon>();
        fade.ForceAlphaTo(1);
        fade.FadeTo(0);
    }

    /// <summary>
    /// Fades to a set alpha
    /// </summary>
    /// <param name="alpha">The alpha target</param>
    public void FadeTo(float alpha)
    {
        fade.FadeTo(alpha);
    }

    /// <summary>
    /// Opens the level up menu for a character
    /// </summary>
    /// <param name="character">The character to level up</param>
    public void OpenLevelUpMenu(RPGCharacter character)
    {
        levelUp.Open(character);
    }

    /// <summary>
    /// Gets the action text
    /// </summary>
    /// <returns>The action text</returns>
    public LocalizedTextAdditive GetActionText()
    {
        return actionText;
    }

    /// <summary>
    /// Changes if the action text is visible or not
    /// </summary>
    /// <param name="visible">True if visible</param>
    public void SetActionTextVisible(bool visible)
    {
        actionText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Set the player icons
    /// </summary>
    /// <param name="datas">The data linked to the icons</param>
    public void SetPlayerIcons(List<BattleManager.CharacterData> datas)
    {
        playerIcons.Clear();
        foreach (BattleManager.CharacterData character in datas)
        {
            BattlePlayerIcon icon = Instantiate(prefabIcon, iconsRoot);
            icon.Init(character, manager.statusSprites);
            playerIcons.Add(icon);
        }
    }

    /// <summary>
    /// Sets the currently active icon
    /// </summary>
    /// <param name="index">The icon's index</param>
    public void SetPlayerIconActive(int index)
    {
        for (int i = 0; i < playerIcons.Count; i++)
        {
            playerIcons[i].SetFocus(index == i);
        }
    }

    /// <summary>
    /// Gets a player icon
    /// </summary>
    /// <param name="ID">The player's ID</param>
    /// <returns>The player's icon</returns>
    public BattlePlayerIcon GetPlayerIcon(string ID)
    {
        for (int i = 0; i < playerIcons.Count; i++)
        {
            if (playerIcons[i].GetID().Equals(ID)) return playerIcons[i];
        }
        return null;
    }


    /// <summary>
    /// Updates all player icons
    /// </summary>
    public void UpdateAllPlayerIcons()
    {
        for (int i = 0; i < playerIcons.Count; i++)
        {
            playerIcons[i].UpdateIcon();
        }
    }

    /// <summary>
    /// Sets if the player menu is visible or not
    /// </summary>
    /// <param name="value">True if the menu is visible</param>
    public void SetPlayerScreenActive(bool value)
    {
        playerScreen.SetActive(value);
    }

    /// <summary>
    /// Opens the player's main screen
    /// </summary>
    public void OpenMainScreen()
    {
        mainScreen.SetActive(true);
        skillsScreen.SetActive(false);
        itemsScreen.SetActive(false);
        targetScreen.SetActive(false);
    }

    /// <summary>
    /// Opens the inventory UI
    /// </summary>
    public void OpenInventory()
    {
        mainScreen.SetActive(false);

        // TODO

        itemsScreen.SetActive(true);
    }

    /// <summary>
    /// Callback for blocking incoming attacks
    /// </summary>
    public void Block()
    {
        SetPlayerScreenActive(false);
        manager.BlockForTurn();
    }

    /// <summary>
    /// Callback for opening the skills menu
    /// </summary>
    public void OpenSkills()
    {
        mainScreen.SetActive(false);
        skillsScreen.SetActive(true);
    }

    /// <summary>
    /// Callback for selecting a skill
    /// </summary>
    /// <param name="skill">The selected skill</param>
    public void SelectSkill(RPGItem skill)
    {
        SetSkillDescription(null);
        currentItem = skill;
        usingSkill = true;

        skillsScreen.SetActive(false);
        OpenTargetScreen();
    }

    /// <summary>
    /// Sets the current skill's description
    /// </summary>
    /// <param name="item">The skill</param>
    public void SetSkillDescription(RPGItem item)
    {
        if (item == null)
        {
            skillDescRoot.SetActive(false);
            return;
        }
        skillDescRoot.SetActive(true);
        skillDescText.SetNewKey(item.ID + "_desc");
        skillNameText.SetNewKey(item.ID + "_name");
    }

    /// <summary>
    /// Sets the current item's desciption
    /// </summary>
    /// <param name="item">The item</param>
    public void SetItemDescription(BattleManager.ItemData item)
    {
        if (item == null)
        {
            itemDescRoot.SetActive(false);
            return;
        }
        itemDescRoot.SetActive(true);
        itemNameText.SetValue(null, item.amountInInventory, false);
        itemDescText.SetNewKey(item.item.ID + "_desc");
        itemNameText.SetNewKey(item.item.ID + "_name");
    }

    /// <summary>
    /// Callback for selecting an item
    /// </summary>
    /// <param name="skill">The selected item</param>
    public void SelectItem(BattleManager.ItemData item)
    {
        SetItemDescription(null);
        currentItem = item.item;
        currentItemData = item;
        usingSkill = false;

        itemsScreen.SetActive(false);
        OpenTargetScreen();
    }

    /// <summary>
    /// Opens the target screen
    /// </summary>
    public void OpenTargetScreen()
    {
        targetScreen.SetActive(true);

        foreach (Transform child in targetButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        List<List<BattleManager.CharacterData>> targets = manager.GetAvailableTargets(currentItem);

        foreach (List<BattleManager.CharacterData> target in targets)
        {
            Instantiate(prefabTargetButton, targetButtonsRoot).Init(target, this);
        }

        targetButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            targetButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabTargetButton.GetComponent<RectTransform>().sizeDelta.y + 5) * targets.Count
        );
    }

    /// <summary>
    /// Callback for selecting a target
    /// </summary>
    /// <param name="target">The target</param>
    public void SelectTarget(List<BattleManager.CharacterData> target)
    {
        SetPlayerScreenActive(false);

        if (!usingSkill)
        {
            currentItemData.amountInInventory--;
            BattleItemButton button;
            foreach (Transform child in itemsButtonsRoot)
            {
                button = child.GetComponent<BattleItemButton>();
                if (button != null && button.GetLinkedItem() == currentItemData)
                {
                    if (currentItemData.amountInInventory == 0)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        button.Refresh();
                    }
                    break;
                }
            }
        }

        manager.UseItemOn(currentItem, target, !usingSkill);
    }

    /// <summary>
    /// Callback for closing the target screen
    /// </summary>
    public void CloseTargetScreen()
    {
        manager.SetCameraTargetToCurrentPlayer();
        targetScreen.SetActive(false);
        if (usingSkill) skillsScreen.SetActive(true);
        else itemsScreen.SetActive(true);
    }

    /// <summary>
    /// Sets the currently displayed skills
    /// </summary>
    /// <param name="skills">The skills</param>
    public void SetCurrentSkills(List<RPGItem> skills)
    {
        foreach (Transform child in skillButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (RPGItem skill in skills)
        {
            Instantiate(prefabSkillButton, skillButtonsRoot).Init(skill, this);
        }

        skillButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            skillButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabSkillButton.GetComponent<RectTransform>().sizeDelta.y + 5) * skills.Count
        );
    }

    /// <summary>
    /// Sets the currently displayed items
    /// </summary>
    /// <param name="items">The items</param>
    public void SetCurrentItems(List<BattleManager.ItemData> items)
    {
        foreach (Transform child in itemsButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (BattleManager.ItemData item in items)
        {
            Instantiate(prefabItemButton, itemsButtonsRoot).Init(item, this);
        }

        itemsButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            itemsButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabItemButton.GetComponent<RectTransform>().sizeDelta.y + 5) * items.Count
        );
    }


}
