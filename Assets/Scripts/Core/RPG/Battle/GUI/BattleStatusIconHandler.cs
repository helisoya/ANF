using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class BattleStatusIconHandler : MonoBehaviour
{
    [SerializeField] private Transform iconsRoot;
    [SerializeField] private BattleStatusIcon iconPrefab;
    private SerializedDictionary<RPGCharacterData.StatusType, Sprite> sprites;
    private List<BattleStatusIcon> unusedIcons;
    private Dictionary<RPGCharacterData.StatusType, BattleStatusIcon> iconsInUse;



    void Awake()
    {

    }

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="sprites">The sprites in use</param>
    public void Init(SerializedDictionary<RPGCharacterData.StatusType, Sprite> sprites)
    {
        this.sprites = sprites;

        unusedIcons = new List<BattleStatusIcon>();
        iconsInUse = new Dictionary<RPGCharacterData.StatusType, BattleStatusIcon>();

        foreach (RPGCharacterData.StatusType type in System.Enum.GetValues(typeof(RPGCharacterData.StatusType)))
        {
            BattleStatusIcon icon = Instantiate(iconPrefab, iconsRoot);
            icon.Disable();
            icon.Init(this);
            unusedIcons.Add(icon);
        }
    }

    /// <summary>
    /// Adds an icon
    /// </summary>
    /// <param name="type">The icon's status</param>
    public void AddIcon(RPGCharacterData.StatusType type)
    {
        if (iconsInUse.ContainsKey(type) || unusedIcons.Count == 0) return;
        BattleStatusIcon icon = unusedIcons[unusedIcons.Count - 1];
        unusedIcons.RemoveAt(unusedIcons.Count - 1);

        icon.Enable(type, sprites[type]);
        icon.transform.SetAsLastSibling();
        iconsInUse.Add(type, icon);
    }

    /// <summary>
    /// Updates an icon
    /// </summary>
    /// <param name="type">The icon's status</param>
    /// <param name="target">The icon's target</param>
    /// <param name="immediate">True if the change must be immediate</param>
    public void UpdateIcon(RPGCharacterData.StatusType type, float target, bool immediate)
    {
        if (iconsInUse.TryGetValue(type, out BattleStatusIcon found))
        {
            found.Refresh(target, immediate);
        }
    }

    /// <summary>
    /// Removes an icon
    /// </summary>
    /// <param name="type">The icon's status</param>
    public void RemoveIcon(RPGCharacterData.StatusType type)
    {
        if (iconsInUse.TryGetValue(type, out BattleStatusIcon found))
        {
            iconsInUse.Remove(type);
            found.Disable();
            unusedIcons.Add(found);
        }
    }
}
