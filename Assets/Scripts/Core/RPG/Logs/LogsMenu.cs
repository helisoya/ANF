using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Represents the logs menu
/// </summary>
public class LogsMenu : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform buttonsParent;
    [SerializeField] private LogButton prefabButton;


    [Header("Log")]
    [SerializeField] private LocalizedText titleText;
    [SerializeField] private LocalizedText descriptionText;
    [SerializeField] private Image image;
    [SerializeField] private Sprite defaultSprite;

    public bool isOpen { get { return root.activeInHierarchy; } }

    /// <summary>
    /// Opens the tab
    /// </summary>
    public void Open()
    {
        ClearButtons();
        AddLogButtons();
        HideLog();
        root.SetActive(true);
    }

    /// <summary>
    /// Closes the tab 
    /// </summary>
    public void Close()
    {
        ClearButtons();
        root.SetActive(false);
    }

    /// <summary>
    /// Clear the existing buttons
    /// </summary>
    private void ClearButtons()
    {
        foreach(Transform child in buttonsParent)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Adds all the log buttons
    /// </summary>
    private void AddLogButtons()
    {
        Log[] logs = GameManager.instance.GetLogs();
        Array.Sort(logs, (Log a, Log b) => { return Locals.GetLocal(a.id + "_name").CompareTo(Locals.GetLocal(b.id + "_name")); });
        bool unlocked;

        foreach(Log log in logs)
        {
            unlocked = GameManager.GetSaveManager().HasUnlockedLog(log.id);
            Instantiate(prefabButton, buttonsParent).Init(log, unlocked, this);
        }
    }

    /// <summary>
    /// Shows a log on screen
    /// </summary>
    /// <param name="log">The log</param>
    public void ShowLog(Log log)
    {
        titleText.SetNewKey(log.id+"_name");
        descriptionText.SetNewKey(log.id+"_desc");
        image.sprite = log.sprite;
    }

    /// <summary>
    /// Hides the current log
    /// </summary>
    public void HideLog()
    {
        titleText.SetNewKey("Log_Unknown");
        descriptionText.SetNewKey("");
        image.sprite = defaultSprite;
    }
}
