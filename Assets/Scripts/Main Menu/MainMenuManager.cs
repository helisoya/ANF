using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles the Main Menu
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private string menuMusic;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private Fade fade;
    [SerializeField] private string defaultBackground;

    [Header("Main Screen")]
    [SerializeField] private GameObject mainScreenRoot;
    [SerializeField] private SaveMenu saveMenu;


    [Header("Name Input")]
    [SerializeField] private GameObject nameInputRoot;
    [SerializeField] private TMP_InputField nameInput;

    private Coroutine fading;

    public static MainMenuManager instance;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        GameManager.instance.inMainMenu = true;
        AudioManager.instance.PlaySong(menuMusic);
        fade.ForceAlphaTo(1);

        string selectedBackground = null;
        if (GameManager.GetSaveManager().SaveFileExists("auto"))
        {
            selectedBackground = GameManager.GetSaveManager().Load("auto").background;
        }

        if (selectedBackground == null || string.IsNullOrEmpty(selectedBackground)) selectedBackground = defaultBackground;
        Instantiate(Resources.Load<Background>("Backgrounds/" + selectedBackground)).ApplyLightRotation();

        fade.FadeTo(0);
    }

    /// <summary>
    /// Click event for starting a new game
    /// </summary>
    public void Event_NewGame()
    {
        if (fading != null) return;
        mainScreenRoot.SetActive(false);
        nameInputRoot.SetActive(true);
    }

    /// <summary>
    /// Click event for confirming the name input and starting the game 
    /// </summary>
    public void Event_ConfirmName()
    {
        if (!string.IsNullOrEmpty(nameInput.text) && nameInput.text.Length <= 12)
        {
            GameManager.instance.SetSaveToLoad(null);
            GameManager.instance.SetNextChapter("Intro/Intro");
            GameManager.GetRPGManager().Reset();
            GameManager.GetSaveManager().ResetVariables();
            GameManager.GetSaveManager().EditVariable("playerName", nameInput.text);
            AudioManager.instance.PlaySong(null);

            StartTransitionToVN();
        }
    }

    /// <summary>
    /// Click event for loading the saved game
    /// </summary>
    public void Event_Continue()
    {
        saveMenu.Open(false);
    }

    /// <summary>
    /// Click event for opening the settings
    /// </summary>
    public void Event_OpenSettings()
    {
        if (fading != null) return;
        pauseMenu.Show();
    }

    /// <summary>
    /// Click event for quitting the game
    /// </summary>
    public void Event_Quit()
    {
        if (fading != null) return;
        Application.Quit();
    }

    /// <summary>
    /// Starts the transition to the VN Scene
    /// </summary>
    public void StartTransitionToVN()
    {
        if (fading != null) return;

        fading = StartCoroutine(Transition());
    }

    /// <summary>
    /// Routine for the transition to the VN Scene
    /// </summary>
    /// <returns>IEnumerator</returns>
    IEnumerator Transition()
    {
        fade.FadeTo(1);

        yield return new WaitForEndOfFrame();
        while (fade.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        GameManager.instance.inMainMenu = false;
        SceneManager.LoadScene("VN");
    }
}
