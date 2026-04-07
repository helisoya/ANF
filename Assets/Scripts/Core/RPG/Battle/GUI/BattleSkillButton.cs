using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a skill button in the battle GUI
/// </summary>
public class BattleSkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private LocalizedTextAdditive skillNameText;
    [SerializeField] private Button button;
    private RPGItem linkedSkill;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The linked item</param>
    /// <param name="gui">The gui</param>
    public void Init(RPGItem item, BattleGUI gui)
    {
        this.linkedSkill = item;
        this.gui = gui;

        if (item.costSP == 0) skillNameText.ResetAdditive();
        else skillNameText.SetValue(null, (int)item.costSP, false);

        skillNameText.SetNewKey(item.ID + "_name");
        button.interactable = gui.manager.CurrentPlayerHasMoreSPThan((int)item.costSP);
    }

    public void Click()
    {
        gui.SelectSkill(linkedSkill);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gui.SetSkillDescription(linkedSkill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gui.SetSkillDescription(null);
    }
}
