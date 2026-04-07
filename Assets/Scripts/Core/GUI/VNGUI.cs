using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Reprensents the VN's GUI
/// </summary>
public class VNGUI : MonoBehaviour
{
    public static VNGUI instance;

    [Header("Fades")]
    [SerializeField] private Fade fadeBg;
    [SerializeField] private Fade fadeFg;
    [SerializeField] private Fade flash;

    [Header("Buttons")]
    [SerializeField] private Button[] buttonsRequiringSaveReady;
    [SerializeField] private Button[] loadButtons;

    [Header("Pause")]
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private SaveMenu saveMenu;

    [Header("Interaction Mode")]
    [SerializeField] private GameObject interactionModeRoot;

    [Header("Level Up")]
    [SerializeField] private LevelUpMenu levelUpMenu;

    [Header("Party Menu")]
    [SerializeField] private PartyMenu partyMenu;

    [Header("Quests")]
    [SerializeField] private QuestsMenu questsMenu;

    [Header("Shop")]
    [SerializeField] private ShopGUI shop;

    [Header("Logs")]
    [SerializeField] private LogsMenu logsMenu;
    [SerializeField] private Animator logAnimation;

    private Coroutine routineExit;

    public bool fadingBg { get { return fadeBg.fading; } }
    public bool fadingFg { get { return fadeFg.fading; } }
    public float fadeBgAlpha { get { return fadeBg.currentAlpha; } }
    public float fadeFgAlpha { get { return fadeFg.currentAlpha; } }
    public Color fadeBgColor { get { return fadeBg.currenColor; } }
    public Color fadeFgColor { get { return fadeFg.currenColor; } }
    public bool fadingFlash { get { return flash.fading; } }

    public bool levelingUp { get { return levelUpMenu.open; } }
    public bool shopOpen { get { return shop.open; } }
    public bool logsMenuOpen { get { return logsMenu.isOpen; } }

    void Awake()
    {
        instance = this;

        fadeFg.ForceAlphaTo(1);
        fadeFg.FadeTo(0);
        fadeBg.ForceAlphaTo(1);
        fadeBg.FadeTo(0);

        flash.ForceAlphaTo(0);
    }

    /// <summary>
    /// Opens the logs menu
    /// </summary>
    public void OpenLogsMenu()
    {
        logsMenu.Open();
    }

    /// <summary>
    /// Plays the new log animation
    /// </summary>
    public void PlayNewLogAnimation()
    {
        logAnimation.SetTrigger("NewLog");
    }

    /// <summary>
    /// Opens the shop GUI
    /// </summary>
    /// <param name="data">The shop's data</param>
    public void OpenShop(ShopData data)
    {
        shop.Open(data);
    }

    /// <summary>
    /// Opens the party menu
    /// </summary>
    public void OpenPartyMenu()
    {
        partyMenu.Open();
    }

    /// <summary>
    /// Opens the quests menu
    /// </summary>
    public void OpenQuests()
    {
        questsMenu.Open();
    }

    /// <summary>
    /// Opens the level up menu for a character
    /// </summary>
    /// <param name="character">The character</param>
    public void OpenLevelUpMenu(RPGCharacter character)
    {
        levelUpMenu.Open(character);
    }

    /// <summary>
    /// Flashes the screen to a set alpha
    /// </summary>
    /// <param name="alpha">The target alpha</param>
    /// <param name="speed">The flash's speed</param>
    public void FlashTo(float alpha, float speed)
    {
        flash.FadeTo(alpha, speed);
    }

    /// <summary>
    /// Fades the Background
    /// </summary>
    /// <param name="alpha">Alpha target</param>
    /// <param name="speed">Fading speed</param>
    public void FadeBgTo(float alpha, float speed = 2)
    {
        fadeBg.FadeTo(alpha, speed);
    }

    /// <summary>
    /// Fades the Foreground
    /// </summary>
    /// <param name="alpha">Alpha target</param>
    /// <param name="speed">Fading speed</param>
    public void FadeFgTo(float alpha, float speed = 2)
    {
        fadeFg.FadeTo(alpha, speed);
    }

    /// <summary>
    /// Changes the Background Fading's color
    /// </summary>
    /// <param name="color">The new color</param>
    public void SetBgColor(Color color)
    {
        fadeBg.SetColor(color);
    }

    /// <summary>
    /// Changes the Foreground Fading's color
    /// </summary>
    /// <param name="color">The new color</param>
    public void SetFgColor(Color color)
    {
        fadeFg.SetColor(color);
    }

    /// <summary>
    /// Force the Background to a set alpha
    /// </summary>
    /// <param name="alpha">The new alpha</param>
    public void ForceBgTo(float alpha)
    {
        fadeBg.ForceAlphaTo(alpha);
    }

    /// <summary>
    /// Force the Foreground to a set alpha
    /// </summary>
    /// <param name="alpha">The new alpha</param>
    public void ForceFgTo(float alpha)
    {
        fadeFg.ForceAlphaTo(alpha);
    }


    /// <summary>
    /// Click event for saving
    /// </summary>
    public void Save()
    {
        if (!NovelController.instance.isReadyForSaving) return;

        saveMenu.Open(true);
    }

    /// <summary>
    /// Click event for loading
    /// </summary>
    public void Load()
    {
        saveMenu.Open(false);
    }

    /// <summary>
    /// Resets the cursor
    /// </summary>
    private void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Click event for loading to the main menu
    /// </summary>
    public void QuitToMainMenu()
    {
        if (routineExit != null) return;
        ResetCursor();

        routineExit = StartCoroutine(Routine_Exit());
    }

    IEnumerator Routine_Exit()
    {
        InteractionManager.instance.SetActive(false);
        AudioManager.instance.PlaySong(null);
        fadeFg.FadeTo(1);

        yield return new WaitForEndOfFrame();
        while (fadeFg.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Event for opening the settings
    /// </summary>
    public void OpenSettings()
    {
        ResetCursor();
        if (!pauseMenu.open) pauseMenu.Show();
    }

    /// <summary>
    /// Event for skipping the dialog
    /// </summary>
    public void SkipDialog()
    {
        NovelController.instance.Next();
    }

    /// <summary>
    /// Changes if the interaction mode is active or not
    /// </summary>
    /// <param name="active">Is the interaction mode active ?</param>
    public void SetInteractionMode(bool active)
    {
        interactionModeRoot.SetActive(active);
        if (active)
        {
            DialogSystem.instance.Close();
        }
    }

    void Update()
    {
        foreach (Button button in buttonsRequiringSaveReady)
        {
            button.interactable = NovelController.instance.isReadyForSaving;
        }

        bool saveFileExists = true;// GameManager.GetSaveManager().saveFileExistsOnDisk;

        foreach (Button button in loadButtons)
        {
            button.interactable = button.interactable && saveFileExists;
        }



        if (NovelController.instance.isReadyForSaving && !partyMenu.open && !questsMenu.open && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.open) pauseMenu.Close();
            else pauseMenu.Show();
        }

    }
}
