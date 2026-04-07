using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// GameManager for the game
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private SaveManager saveManager;


    private string chapterToLoad;
    private string loadSaveName = null;

    public string currentLocation { get; private set; }
    public bool inMainMenu { get; set; }


    [Header("General Informations")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private RPGManager rpgManager;
    [SerializeField] private Log[] logs;

    [Header("Debug")]
    [SerializeField] private bool debug;
    [SerializeField] private string debug_nextChapter;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);

            instance = this;
            Locals.Init();
            saveManager = new SaveManager();
            rpgManager.Init();
            currentLocation = "NO_DATA";

            if (debug)
            {
                chapterToLoad = debug_nextChapter;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (instance == this)
        {
            Settings.Init();
        }
    }

    public void SetCurrentLocation(string locationName)
    {
        currentLocation = locationName;
    }

    /// <summary>
    /// Returns the save manager
    /// </summary>
    /// <returns>The save manager</returns>
    public static SaveManager GetSaveManager()
    {
        if (instance != null) return instance.saveManager;
        return null;
    }

    /// <summary>
    /// Returns the RPG Manager
    /// </summary>
    /// <returns>The RPG Manager</returns>
    public static RPGManager GetRPGManager()
    {
        if (instance != null) return instance.rpgManager;
        return null;
    }

    /// <summary>
    /// Gets the known logs
    /// </summary>
    /// <returns>The known logs</returns>
    public Log[] GetLogs()
    {
        return logs;
    }

    /// <summary>
    /// Sets the savefile to load
    /// </summary>
    /// <param name="value">The savefile's name</param>
    public void SetSaveToLoad(string value)
    {
        loadSaveName = value;
    }

    /// <summary>
    /// Gets the savefile to load
    /// </summary>
    /// <returns>The savefile to load</returns>
    public string GetSaveToLoad()
    {
        return loadSaveName;
    }

    /// <summary>
    /// Returns if the player is loading a save
    /// </summary>
    /// <returns>Is the player loading a save ?</returns>
    public bool IsLoadingSave()
    {
        return loadSaveName != null;
    }

    /// <summary>
    /// Sets the next chapter to be loaded when loading the VN mode
    /// </summary>
    /// <param name="value">The next chapter</param>
    public void SetNextChapter(string value)
    {
        chapterToLoad = value;
    }

    /// <summary>
    /// Returns the next chapter to be loaded
    /// </summary>
    /// <returns>The next chapter</returns>
    public string GetNextChapter()
    {
        return chapterToLoad;
    }

    /// <summary>
    /// Returns the game's audio mixer
    /// </summary>
    /// <returns>The audio mixer</returns>
    public AudioMixer GetAudioMixer()
    {
        return mixer;
    }
}

[System.Serializable]
public struct Log
{
    public string id;
    public Sprite sprite;
}
