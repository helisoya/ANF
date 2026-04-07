using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a tab in the party menu
/// </summary>
public class PartyMenuTab : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltip;
    [SerializeField] private LocalizedText tooltipText;
    private RectTransform canvasTransform;
    public bool open { get { return root.activeInHierarchy; } }

    void Awake()
    {
        Transform current = transform;
        while (current.parent && !current.GetComponent<Canvas>())
        {
            current = current.parent;
        }

        if (current.GetComponent<Canvas>())
        {
            canvasTransform = current.GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Opens the tab
    /// </summary>
    public void Open()
    {
        root.SetActive(true);
        OnOpen();
    }

    /// <summary>
    /// Closes the tab
    /// </summary>
    public void Close()
    {
        root.SetActive(false);
        SetTooltip(null);
        OnClose();
    }

    /// <summary>
    /// Sets the tooltip's text
    /// </summary>
    /// <param name="textID">The tooltip's text</param>
    public void SetTooltip(string textID)
    {
        tooltip.gameObject.SetActive(!string.IsNullOrEmpty(textID));
        tooltipText.SetNewKey(textID);
        tooltipText.GetText().ForceMeshUpdate(true);
    }


    void Update()
    {
        if (open && tooltip.gameObject.activeInHierarchy)
        {
            Vector2 mousePosition = Input.mousePosition;

            if (mousePosition.x + tooltip.rect.width > canvasTransform.rect.width)
            {
                mousePosition.x = canvasTransform.rect.width - tooltip.rect.width;
            }

            if (mousePosition.y + tooltip.rect.height > canvasTransform.rect.height)
            {
                mousePosition.y = canvasTransform.rect.height - tooltip.rect.height;
            }

            tooltip.anchoredPosition = mousePosition;
        }
    }

    /// <summary>
    /// On Open Callback
    /// </summary>
    protected virtual void OnOpen()
    {

    }

    /// <summary>
    /// On Close Callback
    /// </summary>
    protected virtual void OnClose()
    {

    }
}
