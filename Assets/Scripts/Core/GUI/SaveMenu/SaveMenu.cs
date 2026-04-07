using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents the save menu
/// </summary>
public class SaveMenu : MonoBehaviour
{
    [Header("Save Menu")]
    [SerializeField] private GameObject root;
    [SerializeField] private LocalizedText titleText;
    [SerializeField] private SaveButton prefabButton;
    [SerializeField] private Transform buttonsRoot;
    [SerializeField] private SaveButton[] buttons;
    [SerializeField] private CurrentScene currentScene;
    [SerializeField] private Scrollbar scrollbar;

    [Header("Confirm choice")]
    [SerializeField] private GameObject confirmRoot;
    [SerializeField] private SaveButton confirmWidget;

    public enum CurrentScene
    {
        MAINMENU,
        VN,
        DUNGEON
    }


    private int currentSlot;
    private SaveManager.SaveInfo currentInfo;

    private bool isInSavingMode;
    public bool isOpen { get { return root.activeInHierarchy; } }

    void Awake()
    {
        int amount = 11;
        buttons = new SaveButton[amount];
        for (int i = 0; i < amount; i++)
        {
            buttons[i] = Instantiate(prefabButton, buttonsRoot);
        }
        buttonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            buttonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            prefabButton.GetComponent<RectTransform>().sizeDelta.y * amount + 10 * amount
        );
    }

    /// <summary>
    /// Closes the save menu
    /// </summary>
    /// <param name="isLoading">Is the menu loading a new save ?</param>
    public void Close(bool isLoading = false)
    {
        root.SetActive(false);
    }

    /// <summary>
    /// Opens the save menu
    /// </summary>
    /// <param name="isInSavingMode">Is the menu in save mode ? False for load mode</param>
    /// <param name="resetEventSystem">Reset the event system ?</param>
    public void Open(bool isInSavingMode, bool resetEventSystem = true)
    {
        this.isInSavingMode = isInSavingMode;

        root.SetActive(true);
        titleText.SetNewKey(isInSavingMode ? "saves_title_save" : "saves_title_load");

        SaveManager.SaveInfo[] infos = GameManager.GetSaveManager().GetAvailableSaves();

        for (int i = 0; i < infos.Length; i++)
        {
            buttons[i].Init(infos[i], i, this);
        }

        if (resetEventSystem)
        {
            scrollbar.value = 1;
        }
    }

    /// <summary>
    /// Chooses a slot to save / load
    /// </summary>
    /// <param name="slot">The slot index</param>
    /// <param name="info">The slot's indo</param>
    public void ChooseSlot(int slot, SaveManager.SaveInfo info)
    {
        if ((!isInSavingMode && info == null) || (isInSavingMode && slot == 0))
        {
            // Can't choose this
            // Put Animation here
        }
        else
        {
            // Can choose

            if (isInSavingMode && info == null)
            {
                if (currentScene == CurrentScene.VN) NovelController.instance.SaveGameFile(slot.ToString());
                else if (currentScene == CurrentScene.DUNGEON) DungeonManager.instance.SaveGame(slot.ToString());
                Open(isInSavingMode, false);
            }
            else
            {
                confirmRoot.SetActive(true);
                confirmWidget.Init(info, slot, this);
                currentInfo = info;
                currentSlot = slot;
            }

        }
    }


    public void Click_Confirm()
    {
        confirmRoot.SetActive(false);
        if (isInSavingMode)
        {
            if (currentScene == CurrentScene.VN) NovelController.instance.SaveGameFile(currentSlot.ToString());
            else if (currentScene == CurrentScene.DUNGEON) DungeonManager.instance.SaveGame(currentSlot.ToString());
            Open(isInSavingMode, false);
        }
        else
        {
            Time.timeScale = 1;
            if (currentScene == CurrentScene.VN)
            {
                NovelController.instance.LoadGameFile(currentInfo.slot);
                Close(true);
            }
            else if (currentScene == CurrentScene.MAINMENU)
            {
                GameManager.instance.SetSaveToLoad(currentInfo.slot);
                GameManager.instance.SetNextChapter("");
                MainMenuManager.instance.StartTransitionToVN();
                Close(true);
            }
            else if (currentScene == CurrentScene.DUNGEON)
            {
                DungeonManager.instance.LoadGame(currentInfo.slot);
                Close(true);
            }
        }
    }

    public void Click_Back()
    {
        confirmRoot.SetActive(false);
    }

}
