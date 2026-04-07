using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a status icon in battle
/// </summary>
public class BattleStatusIcon : MonoBehaviour
{
    [SerializeField] private Image imgBG;
    [SerializeField] private Image imgFill;
    [SerializeField] private float fillSpeed = 2;
    private float targetFillAmount;
    private bool filling = false;
    private BattleStatusIconHandler handler;
    private RPGCharacterData.StatusType status;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="handler">Its handler</param>
    public void Init(BattleStatusIconHandler handler)
    {
        this.handler = handler;
    }

    /// <summary>
    /// Initialize the icon
    /// </summary>
    /// <param name="status">The status linked to the icon</param>
    /// <param name="img">The icon's sprite</param>
    public void Enable(RPGCharacterData.StatusType status, Sprite img)
    {
        gameObject.SetActive(true);
        imgBG.sprite = img;
        imgFill.sprite = img;
        this.status = status;
        Refresh(1, true);
    }

    /// <summary>
    /// Refreshs the icon's fill amount
    /// </summary>
    /// <param name="target">The fill target</param>
    /// <param name="immediate">True if the fill is immediate</param>
    public void Refresh(float target, bool immediate)
    {
        if (target == targetFillAmount) return;

        targetFillAmount = target;

        if (immediate)
        {
            imgFill.fillAmount = target;
            filling = false;
        }
        else
        {
            filling = true;
        }
    }

    /// <summary>
    /// Disable the icon
    /// </summary>
    public void Disable()
    {
        gameObject.SetActive(false);
        filling = false;
    }

    void Update()
    {
        if (filling)
        {
            float side = imgFill.fillAmount < targetFillAmount ? 1 : -1;

            imgFill.fillAmount = Mathf.Clamp(imgFill.fillAmount + fillSpeed * Time.deltaTime * side, side == -1.0f ? targetFillAmount : 0, side == -1.0f ? 1 : targetFillAmount);

            if (imgFill.fillAmount == targetFillAmount)
            {
                filling = false;
                if (targetFillAmount == 0.0f)
                {
                    handler.RemoveIcon(status);
                    Disable();
                }
            }
        }
    }
}
