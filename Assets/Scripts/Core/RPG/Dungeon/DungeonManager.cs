using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles dungeons
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float playerSpeed;

    [Header("Encounters")]
    [SerializeField] private float averageMeterstoEncounter = 15f;
    public float currentMetersRemainingToEncounter;

    [Header("Components")]
    [SerializeField] private Behaviour[] disableOnBattle;
    [SerializeField] private Generator2D generator;
    [SerializeField] private DungeonGUI gui;
    private DungeonStairs stairs;
    private int currentFloor;

    public static DungeonManager instance;
    private bool inBattle = false;
    private DungeonData data;
    private Coroutine routineNextFloor;
    private Vector3 moveVector;
    private Vector2Int playerPosition;
    private float metersToNextRegen;
    public bool changingFloor { get { return routineNextFloor != null; } }




    [Header("Debug")]
    [SerializeField] private bool useDebug = true;
    [SerializeField] private DungeonData debugData;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetupGUI();
        if (useDebug)
        {
            GameManager.GetRPGManager().AddItemToInventory("ITEM_POTION", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_REVIVE", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_POISON", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_CURE", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_SLEEP", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_ATTACK_UP", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_DEFENSE_UP", 4);
            GameManager.GetRPGManager().AddItemToInventory("ITEM_CONFUSE", 4);
            LoadDungeon(debugData);
        }
        else LoadDungeon(GameManager.GetRPGManager().dungeonData, GameManager.GetRPGManager().dungeonFloorStart);
    }

    public void SaveGame(string slot)
    {
        GAMEFILE activeGameFile = GameManager.GetSaveManager().saveFile;

        activeGameFile.rpgCharacters = GameManager.GetRPGManager().GetCharacters();
        activeGameFile.inventory = GameManager.GetRPGManager().GetInventory();
        activeGameFile.followers = GameManager.GetRPGManager().GetFollowers();
        activeGameFile.money = GameManager.GetRPGManager().money;
        activeGameFile.locationName = GameManager.instance.currentLocation;

        activeGameFile.localFiles = Locals.currentFiles;


        activeGameFile.inDungeon = true;
        activeGameFile.dungeonID = data.ID;
        activeGameFile.dungeonFloor = currentFloor;

        GameManager.GetSaveManager().Save(slot);
    }

    public void LoadGame(string slot)
    {
        GAMEFILE activeGameFile = GameManager.GetSaveManager().Load(slot);

        if (!activeGameFile.inDungeon)
        {
            GameManager.instance.SetSaveToLoad(slot);
            GameManager.instance.SetNextChapter("");
            gui.ChangeScene("VN");
            return;
        }

        gui.ClosePauseMenu();
        GameManager.instance.SetSaveToLoad(null);
        GameManager.GetRPGManager().LoadCharactersFromList(activeGameFile.rpgCharacters);
        GameManager.GetRPGManager().SetFollowers(activeGameFile.followers);
        GameManager.GetRPGManager().SetInventory(activeGameFile.inventory);
        GameManager.GetRPGManager().SetMoney(activeGameFile.money);
        GameManager.instance.SetCurrentLocation(activeGameFile.locationName);
        Locals.SetFiles(activeGameFile.localFiles);

        gui.RefreshPlayerIcons();
        LoadDungeon(Resources.Load<DungeonData>("RPG/Dungeons/" + activeGameFile.dungeonID), activeGameFile.dungeonFloor);
    }

    /// <summary>
    /// Setups the GUI
    /// </summary>
    private void SetupGUI()
    {
        List<int> followers = GameManager.GetRPGManager().GetFollowers();
        List<RPGCharacter> players = new List<RPGCharacter>();
        foreach (int follower in followers)
        {
            players.Add(GameManager.GetRPGManager().GetCharacter(follower));
        }

        gui.SetPlayerIcons(players);
    }

    /// <summary>
    /// Loads a new dungeon
    /// </summary>
    /// <param name="data">The dungeon's data</param>
    public void LoadDungeon(DungeonData data, int startFloor = 0)
    {
        this.data = data;
        if (stairs) Destroy(stairs.gameObject);
        GameManager.instance.SetCurrentLocation(data.locationID);
        stairs = Instantiate(data.stairsPrefab);
        generator.ResetEntitiesToPlace();
        generator.AddEntityToPlace(stairs.transform);
        stairs.active = false;
        foreach (GameObject prop in data.propsToPlace) generator.AddEntityToPlace(Instantiate(prop).transform);
        currentFloor = startFloor - 1;
        AudioManager.instance.PlaySong(data.musicName);
        gui.SetCanExitDungeon(data.canExitEarly, data.earlyExitChapter);
        NextFloor();
    }

    /// <summary>
    /// Loads the next floor
    /// </summary>
    public void NextFloor()
    {
        if (routineNextFloor != null)
        {
            StopCoroutine(routineNextFloor);
        }
        routineNextFloor = StartCoroutine(Routine_TransitionToNextFloor());
    }

    private Vector2Int ComputePlayerPosition()
    {
        return new Vector2Int(
            Mathf.FloorToInt(playerRigidbody.position.x / 8.0f),
            Mathf.FloorToInt(playerRigidbody.position.z / 8.0f));
    }

    /// <summary>
    /// Routine for loading the next floor
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator Routine_TransitionToNextFloor()
    {
        gui.FadeTo(1f);
        yield return new WaitForEndOfFrame();
        while (gui.fading) yield return new WaitForEndOfFrame();

        currentFloor++;
        gui.SetFloor(currentFloor + 1);
        if (currentFloor == data.floorsAmount)
        {
            AudioManager.instance.PlaySong(null);
            GameManager.instance.SetNextChapter(data.endChapter);
            SceneManager.LoadScene("VN");
        }
        else
        {
            generator.Generate(data);
            stairs.active = true;
            playerPosition = ComputePlayerPosition();
            gui.UpdateMiniMap(generator.grid, playerPosition);
            ComputeMetersToNextEncounter();
            yield return new WaitForEndOfFrame();
            gui.FadeTo(0f);
        }
        routineNextFloor = null;
    }

    /// <summary>
    /// Routine for starting a battle
    /// </summary>
    /// <returns></returns>
    private IEnumerator Routine_TransitionToBattle()
    {
        gui.ShaderFadeTo(1.0f);
        yield return new WaitForEndOfFrame();
        while (gui.fadingShader) yield return new WaitForEndOfFrame();

        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        EnableBattleRequirements(false);
    }

    /// <summary>
    /// Computes the meters to the next encounter
    /// </summary>
    private void ComputeMetersToNextEncounter()
    {
        currentMetersRemainingToEncounter = Random.Range(averageMeterstoEncounter - 5f, averageMeterstoEncounter + 5f);
    }

    /// <summary>
    /// Starts a random encounter
    /// </summary>
    public void StartRandomEncounter()
    {
        ComputeMetersToNextEncounter();
        if (data.encounters.Length == 0) return;

        BattleData chosenData = data.encounters[Random.Range(0, data.encounters.Length)];
        AudioManager.instance.PlaySong(chosenData.music);
        inBattle = true;
        GameManager.GetRPGManager().SetNextBattleEncounter(
            chosenData,
            data.battleBackground,
            BattleData.CloseType.UNLOAD,
            null
        );
        StartCoroutine(Routine_TransitionToBattle());
    }

    /// <summary>
    /// Ends the current battle
    /// </summary>
    public void EndBattle()
    {
        gui.RefreshPlayerIcons();
        SceneManager.UnloadSceneAsync("Battle").completed += _ =>
        {
            AudioManager.instance.PlaySong(data.musicName);
            gui.ShaderFadeTo(0.0f);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Dungeon"));
            inBattle = false;
            EnableBattleRequirements(true);
        };
    }

    /// <summary>
    /// Enables the battle requirements
    /// </summary>
    /// <param name="active">True if they must be active</param>
    public void EnableBattleRequirements(bool active)
    {
        foreach (Behaviour requirement in disableOnBattle)
        {
            requirement.enabled = active;
        }
    }

    void Update()
    {
        if (inBattle || changingFloor || gui.isChangingScene) return;

        cameraTransform.position = playerRigidbody.position + cameraOffset;
        moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        bool moving = moveVector != Vector3.zero;
        if (moving)
        {
            playerModel.forward = moveVector;
            float value = (Mathf.Abs(moveVector.x) + Mathf.Abs(moveVector.z)) * Time.deltaTime;
            currentMetersRemainingToEncounter -= value;
            metersToNextRegen -= value;

            if (metersToNextRegen <= 0)
            {
                metersToNextRegen = 1.0f;

                foreach (int follower in GameManager.GetRPGManager().GetFollowers())
                {
                    GameManager.GetRPGManager().GetCharacter(follower).AddSP(1);
                    GameManager.GetRPGManager().GetCharacter(follower).AddHealth(1);
                }

                gui.RefreshPlayerIcons();
            }

            if (currentMetersRemainingToEncounter <= 0) StartRandomEncounter();
        }
        playerAnimator.SetBool("Move", moving);

        Vector2Int newPosition = ComputePlayerPosition();
        if (newPosition != playerPosition)
        {
            playerPosition = newPosition;
            gui.UpdateMiniMap(generator.grid, playerPosition);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gui.OpenPauseMenu();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartRandomEncounter();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            NextFloor();
        }
#endif
    }

    void FixedUpdate()
    {
        if (inBattle || changingFloor || gui.isChangingScene)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        playerRigidbody.linearVelocity = moveVector * playerSpeed;

    }
}
