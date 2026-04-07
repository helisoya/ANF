using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a button that shows a log
/// </summary>
public class LogButton : MonoBehaviour
{
    [SerializeField] private LocalizedText label;
    [SerializeField] private Button button;

    private Log linkedLog;
    private LogsMenu menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="log">The linked log</param>
    /// <param name="unlocked">True if the log is unlocked</param>
    /// <param name="menu">The parent menu</param>
    public void Init(Log log, bool unlocked,LogsMenu menu)
    {
        linkedLog = log;
        label.SetNewKey(unlocked ? log.id + "_name" : "Log_Unknown");
        button.interactable = unlocked;
        this.menu = menu;
    }

    public void OnClick()
    {
        menu.ShowLog(linkedLog);
    }
}
