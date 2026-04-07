using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Represents the quests menu
/// </summary>
public class QuestsMenu : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;
    [SerializeField] private string[] tags = { "mainQuest", "arcQuest", "factionQuest", "secondaryQuest" };

    [Header("In Progress")]
    [SerializeField] private RectTransform inProgressRoot;
    [SerializeField] private QuestInfo inProgressQuestPrefab;
    [SerializeField] private GameObject inProgressNoneText;

    [Header("Done")]
    [SerializeField] private RectTransform doneRoot;
    [SerializeField] private QuestInfo doneQuestPrefab;
    [SerializeField] private GameObject doneNoneText;

    private Dictionary<string, List<string>> quests;

    public bool open { get { return root != null; } }

    void Awake()
    {
        InitQuestIds();
    }

    /// <summary>
    /// Reads which variables are quests
    /// </summary>
    private void InitQuestIds()
    {
        quests = new Dictionary<string, List<string>>();

        List<string> currentList = null;
        List<string> lines = FileManager.ReadTextAsset(Resources.Load<TextAsset>("General/quests"));
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentList = new List<string>();
                    quests.Add(line.Substring(1, line.Length - 2), currentList);
                }
                else if (currentList != null)
                {
                    currentList.Add(line);
                }
            }
        }
    }

    /// <summary>
    /// Opens the menu
    /// </summary>
    public void Open()
    {
        root.SetActive(true);
        ShowQuestsForTag(0);
    }

    /// <summary>
    /// Closes the menu
    /// </summary>
    public void Close()
    {
        root.SetActive(false);
    }

    /// <summary>
    /// Shows the done and in progress quests for a tag
    /// </summary>
    /// <param name="tagIdx">The tag's index</param>
    public void ShowQuestsForTag(int tagIdx)
    {
        if (tagIdx < 0 || tagIdx >= tags.Length) return;

        foreach (Transform child in doneRoot) Destroy(child.gameObject);
        foreach (Transform child in inProgressRoot) Destroy(child.gameObject);

        int inProgressAmount = 0;
        int doneAmount = 0;

        string tempValue;
        int parsedValue;
        if (quests.TryGetValue(tags[tagIdx], out List<string> ids))
        {
            foreach (string id in ids)
            {
                tempValue = GameManager.GetSaveManager().GetItem(id);
                if (!int.TryParse(tempValue,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsedValue))
                {
                    parsedValue = -1;
                    Debug.LogWarning($"Quest Variable Invalid : {id} - {tempValue} ");
                }

                if (parsedValue == 100)
                {
                    Instantiate(doneQuestPrefab, doneRoot).Init(id, tempValue);
                    doneAmount++;
                }
                else if (parsedValue >= 0 && parsedValue < 100)
                {
                    Instantiate(inProgressQuestPrefab, inProgressRoot).Init(id, tempValue);
                    inProgressAmount++;
                }
            }
        }

        doneRoot.sizeDelta = new Vector2(
            doneRoot.sizeDelta.x,
            doneAmount * doneQuestPrefab.GetComponent<RectTransform>().sizeDelta.y + 5 * doneAmount
        );

        inProgressRoot.sizeDelta = new Vector2(
            inProgressRoot.sizeDelta.x,
            inProgressAmount * inProgressQuestPrefab.GetComponent<RectTransform>().sizeDelta.y + 5 * inProgressAmount
        );

        doneNoneText.SetActive(doneAmount == 0);
        inProgressNoneText.SetActive(inProgressAmount == 0);
    }
}
