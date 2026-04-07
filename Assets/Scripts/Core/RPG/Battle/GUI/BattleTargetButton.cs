using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleTargetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI targetNameText;
    private List<BattleManager.CharacterData> linkedTarget;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="target">The linked target</param>
    /// <param name="gui">The gui</param>
    public void Init(List<BattleManager.CharacterData> target, BattleGUI gui)
    {
        this.linkedTarget = target;
        this.gui = gui;

        string text = "";
        if (target.Count == 1)
        {
            text = target[0].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(target[0].characterData.GetData().ID + "_name");
        }
        else
        {
            text = Locals.GetLocal("battle_target_multiple");
        }
        targetNameText.text = text;


    }

    public void Click()
    {
        gui.SelectTarget(linkedTarget);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (BattleManager.CharacterData character in linkedTarget)
        {
            character.characterVisual.SetHealthBarVisible(true);
            character.characterVisual.setHealthBarFillAmount(character.characterData.currentHealth / (float)character.characterData.maxHealth, true);
        }

        if (linkedTarget.Count == 1) gui.manager.SetCameraTarget(linkedTarget[0].characterVisual.GetCameraTarget().transform);
        else
        {
            Vector3 position = Vector3.zero;
            foreach (BattleManager.CharacterData character in linkedTarget)
            {
                position += character.characterVisual.transform.position;
            }
            gui.manager.SetCameraTarget(position / linkedTarget.Count);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (BattleManager.CharacterData character in linkedTarget)
        {
            character.characterVisual.SetHealthBarVisible(false);
        }
    }
}
