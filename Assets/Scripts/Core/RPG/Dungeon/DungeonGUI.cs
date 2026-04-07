using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Represents the dungeon's GUI
/// </summary>
public class DungeonGUI : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private BattlePlayerIcon playerIconPrefab;
    [SerializeField] private Transform playerIconsRoot;
    private List<BattlePlayerIcon> playersIcons;

    [Header("MiniMap")]
    [SerializeField] private Transform miniMapRoot;

    [Header("Pause")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject exitDungeonButton;

    [Header("Floor")]
    [SerializeField] private LocalizedTextAdditive currentFloorText;


    [Header("Other")]
    [SerializeField] private Fade fade;
    [SerializeField] private FadeShader fadeShader;
    [SerializeField] private SaveMenu saveMenu;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private PartyMenu partyMenu;
    public bool fading { get { return fade.fading; } }
    public bool fadingShader { get { return fadeShader.fading; } }
    private Coroutine chaningScene;
    public bool isChangingScene { get { return chaningScene != null; } }
    private string nextChapterIfExiting;

    void Start()
    {
        fadeShader.ForceAlphaTo(0.0f);
        fade.ForceAlphaTo(1f);
        fade.FadeTo(0f);
    }

    /// <summary>
    /// Sets if the dungeon exit button is available or not
    /// </summary>
    /// <param name="canExit">True if the player can exit the dungeon</param>
    /// <param name="nextChapterIfExiting">The next chapter if the dungeon is exited early</param>
    public void SetCanExitDungeon(bool canExit, string nextChapterIfExiting)
    {
        exitDungeonButton.SetActive(canExit);
        this.nextChapterIfExiting = nextChapterIfExiting;
    }



    /// <summary>
    /// Sets the current floor on GUI
    /// </summary>
    /// <param name="floor">The current floor</param>
    public void SetFloor(int floor)
    {
        currentFloorText.SetValue(null, floor, true);
    }

    /// <summary>
    /// Updates the minimap
    /// </summary>
    /// <param name="grid">The grid</param>
    /// <param name="playerPos">The player position</param>
    public void UpdateMiniMap(Grid2D<Generator2D.CellType> grid, Vector2Int playerPos)
    {
        int gridSize = 7;
        int middlePos = Mathf.FloorToInt(gridSize / 2.0f);
        int x;
        int y;
        Color color;
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            y = i / gridSize;
            x = i % gridSize;
            if (x == middlePos && y == middlePos) continue;
            x = playerPos.x + (x - middlePos);
            y = playerPos.y - (y - middlePos);


            if (x < 0 || y < 0 || y > grid.Size.y - 1 || x > grid.Size.x - 1) color = Color.black;
            else color = grid[x, y] == Generator2D.CellType.None ? Color.black : Color.gray;
            miniMapRoot.GetChild(i).GetComponent<Image>().color = color;
        }
    }

    /// <summary>
    /// Sets the current players on GUI
    /// </summary>
    /// <param name="players">The list of players</param>
    public void SetPlayerIcons(List<RPGCharacter> players)
    {
        playersIcons = new List<BattlePlayerIcon>();
        foreach (RPGCharacter character in players)
        {
            BattlePlayerIcon icon = Instantiate(playerIconPrefab, playerIconsRoot);
            icon.Init(character, null);
            playersIcons.Add(icon);
        }
    }

    /// <summary>
    /// Refreshs the player icons
    /// </summary>
    public void RefreshPlayerIcons()
    {
        foreach (BattlePlayerIcon icon in playersIcons)
        {
            icon.UpdateIcon(true);
        }
    }

    /// <summary>
    /// Toggles the pause menu
    /// </summary>
    public void OpenPauseMenu()
    {
        Time.timeScale = 0;
        pauseRoot.SetActive(true);
    }

    /// <summary>
    /// Closes the pause menu
    /// </summary>
    public void ClosePauseMenu()
    {
        if (isChangingScene) return;

        Time.timeScale = 1;
        RefreshPlayerIcons();
        pauseRoot.SetActive(false);
    }

    /// <summary>
    /// Opens the settings
    /// </summary>
    public void OpenSettings()
    {
        pauseMenu.Show();
    }

    /// <summary>
    /// Opens the save menu
    /// </summary>
    public void OpenSave()
    {
        saveMenu.Open(true);
    }

    /// <summary>
    /// Opens the load menu
    /// </summary>
    public void OpenLoad()
    {
        saveMenu.Open(false);
    }

    /// <summary>
    /// Opens the party menu
    /// </summary>
    public void OpenParty()
    {
        partyMenu.Open();
    }

    /// <summary>
    /// Closes the game
    /// </summary>    
    public void QuitGame()
    {
        if (isChangingScene) return;
        ChangeScene("MainMenu");
    }

    /// <summary>
    /// Exits the dungeon early
    /// </summary>
    public void ExitDungeonEarly()
    {
        if (isChangingScene) return;

        GameManager.instance.SetSaveToLoad(null);
        GameManager.instance.SetNextChapter(nextChapterIfExiting);
        chaningScene = StartCoroutine(Routine_ChangeScene("VN"));
    }

    /// <summary>
    /// Changes the scene
    /// </summary>
    /// <param name="scene">The new scene</param>
    public void ChangeScene(string scene)
    {
        if (isChangingScene) return;
        chaningScene = StartCoroutine(Routine_ChangeScene(scene));
    }

    /// <summary>
    /// Routine for changing scenes
    /// </summary>
    /// <param name="scene">The new scene</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator Routine_ChangeScene(string scene)
    {
        Time.timeScale = 1f;
        AudioManager.instance.PlaySong(null);
        FadeTo(1);
        yield return new WaitForEndOfFrame();
        while (fading) yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }


    /// <summary>
    /// Starts fading the screen
    /// </summary>
    /// <param name="alpha">The alpha target</param>
    public void FadeTo(float alpha)
    {
        fade.FadeTo(alpha);
    }

    /// <summary>
    /// Starts fading the screen (shader version)
    /// </summary>
    /// <param name="alpha">The alpha target</param>
    public void ShaderFadeTo(float alpha)
    {
        fadeShader.FadeTo(alpha);
    }
}
