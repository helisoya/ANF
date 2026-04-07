using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an animated health / SP bar in battle
/// </summary>
public class BattleBarFill : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Image additive;
    [SerializeField] private float fasterSpeed = 2.0f;
    [SerializeField] private float slowerSpeed = 1.0f;
    [SerializeField] private Color barColor;
    [SerializeField] private Color upwardColor;
    [SerializeField] private Color downwardColor;
    private float currentTarget = 1f;
    private bool upward;
    public bool filling { get; private set; }

    void Start()
    {
        fill.color = barColor;
    }

    /// <summary>
    /// Sets the value of the bar
    /// </summary>
    /// <param name="targetValue">The bar</param>
    /// <param name="immediate">True if the change should be immediate</param>
    public void SetValue(float targetValue, bool immediate)
    {
        if (immediate)
        {
            currentTarget = targetValue;
            filling = false;
            fill.fillAmount = currentTarget;
            additive.fillAmount = currentTarget;
            return;
        }

        filling = true;

        if (currentTarget > targetValue)
        {
            // downward
            upward = false;
            fill.fillAmount = targetValue;
            additive.color = downwardColor;
        }
        else
        {
            // upward
            upward = true;
            additive.fillAmount = targetValue;
            additive.color = upwardColor;
        }


        currentTarget = targetValue;
    }

    void Update()
    {
        if (filling)
        {
            if (upward)
            {
                if (additive.fillAmount != currentTarget)
                {
                    additive.fillAmount = Mathf.Clamp(additive.fillAmount + fasterSpeed * Time.deltaTime, 0f, currentTarget);
                }
                fill.fillAmount = Mathf.Clamp(fill.fillAmount + slowerSpeed * Time.deltaTime, 0f, currentTarget);
            }
            else
            {
                if (fill.fillAmount != currentTarget)
                {
                    fill.fillAmount = Mathf.Clamp(fill.fillAmount - fasterSpeed * Time.deltaTime, currentTarget, 1f);
                }
                additive.fillAmount = Mathf.Clamp(additive.fillAmount - slowerSpeed * Time.deltaTime, currentTarget, 1f);
            }

            if (fill.fillAmount == currentTarget && additive.fillAmount == currentTarget)
            {
                filling = false;
            }
        }
    }
}
