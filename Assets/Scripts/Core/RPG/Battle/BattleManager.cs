using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using AYellowpaper.SerializedCollections;

/// <summary>
/// Handles the game's battles
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Battles")]
    [SerializeField] private RPGItem defaultAttack;
    [SerializeField] private BattleGUI gui;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform freeLookTransform;

    [Header("Lighting")]
    [SerializeField] private LightingManager lighting;

    [Header("Placements")]
    [SerializeField] private float posStart = 0;
    [SerializeField] private float posEnd = 40;
    [SerializeField] private float ennemyDistance = 10;
    [SerializeField] private float cameraUpDistance = 5;

    [Header("Status Sprites")]
    public SerializedDictionary<RPGCharacterData.StatusType, Sprite> statusSprites;

    [Header("Quests")]
    [SerializeField] private BattleQuestData[] quests;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioData audioData;

    [Header("DEBUG")]
    [SerializeField] private bool useDebug = true;
    [SerializeField] private BattleData data;
    [SerializeField] private string backgroundName;

    private BattleData currentData;
    private List<CharacterData> players;
    private List<CharacterData> ennemies;
    private List<CharacterData> order;
    private int currentOrderIdx;
    private Coroutine routineAttack = null;

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Battle"));

        players = new List<CharacterData>();
        ennemies = new List<CharacterData>();
        order = new List<CharacterData>();
        gui.SetPlayerScreenActive(false);

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
            SetBackground(backgroundName);
            LoadBattle(data);
        }
        else
        {
            SetBackground(GameManager.GetRPGManager().battleBackground);
            LoadBattle(GameManager.GetRPGManager().battleData);
        }

    }

    /// <summary>
    /// Sets the background
    /// </summary>
    /// <param name="backgroundName">The background's name</param>
    public void SetBackground(string backgroundName)
    {
        Instantiate(Resources.Load<GameObject>("RPG/Battles/Backgrounds/" + backgroundName), Vector3.zero, Quaternion.identity);
    }


    /// <summary>
    /// Routine for losing a battle
    /// </summary>   
    IEnumerator Routine_Lose()
    {
        gui.GetActionText().ResetAdditive();
        gui.GetActionText().SetNewKey("battle_lost");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        gui.FadeTo(1);
        yield return new WaitForEndOfFrame();
        while (gui.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        AudioManager.instance.PlaySong(null);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Routine for winning a battle
    /// </summary>
    IEnumerator Routine_Win()
    {
        gui.GetActionText().ResetAdditive();
        gui.GetActionText().SetNewKey("battle_won");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        GameManager.GetRPGManager().AddMoney(currentData.goldReward);

        gui.GetActionText().SetParameters("", "", " ", "");
        gui.GetActionText().SetValue(null, currentData.goldReward, false);
        gui.GetActionText().SetNewKey("battle_gold");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        foreach (CharacterData follower in players)
        {
            if (follower.dead) follower.characterData.AddHealth(1);
        }

        int totalExp = 0;
        foreach (CharacterData ennemy in ennemies)
        {
            totalExp += ennemy.characterData.GetData().exp;
            foreach (CharacterData follower in players)
            {
                follower.characterData.GetData().exp += ennemy.characterData.GetData().exp;
            }

            foreach (BattleQuestData data in quests)
            {
                if (ennemy.characterData.GetData().ID.Equals(data.entityToKill))
                {
                    int questValue = int.Parse(GameManager.GetSaveManager().GetItem(data.questID));
                    if (questValue >= data.from && questValue < data.to)
                    {
                        questValue++;
                        GameManager.GetSaveManager().EditVariable(data.questID, questValue.ToString());
                    }
                }

            }
        }

        gui.GetActionText().SetParameters("", "", " ", "");
        gui.GetActionText().SetValue(null, totalExp, false);
        gui.GetActionText().SetNewKey("battle_exp");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        foreach (CharacterData follower in players)
        {
            if (follower.characterData.canLevelUp)
            {

                SetCameraTarget(follower.characterVisual.GetCameraTarget());
                string playerName = follower.characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(follower.characterData.GetData().ID + "_name");
                gui.GetActionText().SetParameters("", " ", "", "");
                gui.GetActionText().SetValue(playerName, null, false);
                gui.GetActionText().SetNewKey("battle_levelUp");
                gui.SetActionTextVisible(true);
                yield return new WaitForSeconds(1.0f);

                gui.OpenLevelUpMenu(follower.characterData);
                yield return new WaitForEndOfFrame();
                while (gui.levelingUp) yield return new WaitForEndOfFrame();
            }
        }

        gui.SetActionTextVisible(false);
        gui.FadeTo(1);
        yield return new WaitForEndOfFrame();
        while (gui.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameManager.GetRPGManager().battleCloseType == BattleData.CloseType.VN)
        {
            AudioManager.instance.PlaySong(null);
            GameManager.instance.SetNextChapter(GameManager.GetRPGManager().battleNextChapter);
            SceneManager.LoadScene("VN");
        }
        else
        {
            DungeonManager.instance.EndBattle();
        }
    }

    /// <summary>
    /// Wins the battle
    /// </summary>
    public void WinBattle()
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Win());
    }

    /// <summary>
    /// Loses the battle
    /// </summary>
    public void LoseBattle()
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Lose());
    }

    /// <summary>
    /// Checks if the current player has more SP than the specified amount
    /// </summary>
    /// <param name="amount">The amount of SP to check</param>
    /// <returns>True if the current player has more SP than the amount given</returns>
    public bool CurrentPlayerHasMoreSPThan(int amount)
    {
        return order[currentOrderIdx].characterData.currentSP >= amount;
    }

    /// <summary>
    /// Sets the current camera target
    /// </summary>
    /// <param name="target">The new target</param>
    public void SetCameraTarget(Transform target)
    {
        virtualCamera.LookAt = target;
    }

    /// <summary>
    /// Sets the current camera target
    /// </summary>
    /// <param name="position">The new target's position</param>
    public void SetCameraTarget(Vector3 position)
    {
        freeLookTransform.position = position;
        SetCameraTarget(freeLookTransform);
    }

    /// <summary>
    /// Sets the current camera target to the current player
    /// </summary>
    public void SetCameraTargetToCurrentPlayer()
    {
        SetCameraTarget(order[currentOrderIdx].characterVisual.GetCameraTarget());
    }

    /// <summary>
    /// Loads a new battle
    /// </summary>
    /// <param name="data">The battle's data</param>
    public void LoadBattle(BattleData data)
    {
        this.currentData = data;
        players.Clear();
        ennemies.Clear();
        order.Clear();
        currentOrderIdx = 0;

        AudioManager.instance.PlaySong(data.music);
        lighting.ChangeData(GameManager.GetSaveManager().GetItem("night").Equals("0") ? data.daySky : data.nightSky);


        List<ItemData> items = new List<ItemData>();
        RPGItem item;
        foreach (InventorySlot slot in GameManager.GetRPGManager().GetInventory())
        {
            item = GameManager.GetRPGManager().GetItem(slot.itemID);
            if (item.type == RPGItem.ItemType.USABLE_COMBAT || item.type == RPGItem.ItemType.USABLE_ALL)
            {
                items.Add(new ItemData { item = item, amountInInventory = slot.itemAmount });
            }
        }

        gui.SetCurrentItems(items);

        GenerateCharacters();
        NextTurn();
    }

    /// <summary>
    /// Checks if all datas in a list are dead
    /// </summary>
    /// <param name="datas">The data list</param>
    /// <returns>True if they are all dead</returns>
    private bool AllDead(List<CharacterData> datas)
    {
        foreach (CharacterData data in datas)
        {
            if (!data.dead) return false;
        }
        return true;
    }

    /// <summary>
    /// Handles the next turn
    /// </summary>
    public void NextTurn()
    {
        if (AllDead(ennemies))
        {
            WinBattle();
            return;
        }
        else if (AllDead(players))
        {
            LoseBattle();
            return;
        }

        CharacterData data = order[currentOrderIdx];

        if (data.dead)
        {
            EndTurn();
            return;
        }

        if (data.blocking)
        {
            data.blocking = false;
            data.characterVisual.SetBlocking(false);
        }
        SetCameraTarget(data.characterVisual.GetCameraTarget());

        if (data.status.Find(entry => entry.status == RPGCharacterData.StatusType.SLEEP) != null)
        {
            StopAllCoroutines();
            routineAttack = StartCoroutine(Routine_Sleep());
            return;
        }
        else if (data.status.Find(entry => entry.status == RPGCharacterData.StatusType.CONFUSED) != null)
        {
            StopAllCoroutines();
            routineAttack = StartCoroutine(Routine_Confused());
            return;
        }

        if (data.isPlayer)
        {
            gui.SetPlayerIconActive(players.FindIndex(ch => ch == data));
            List<RPGItem> skills = new List<RPGItem>();
            skills.Add(defaultAttack);
            if (!string.IsNullOrEmpty(data.characterData.GetData().weapon))
            {
                foreach (string skillID in GameManager.GetRPGManager().GetItem(data.characterData.GetData().weapon).weaponSkills)
                {
                    skills.Add(GameManager.GetRPGManager().GetItem(skillID));
                }
            }

            gui.SetCurrentSkills(skills);

            gui.SetPlayerScreenActive(true);
            gui.OpenMainScreen();
        }
        else
        {
            HandleAI(data);
        }
    }

    /// <summary>
    /// Routine for blocking
    /// </summary>
    private IEnumerator Routine_Block()
    {
        // Play animation
        SetCameraTargetToCurrentPlayer();
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");
        gui.GetActionText().SetParameters("", " ", "", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_block");
        gui.SetActionTextVisible(true);
        order[currentOrderIdx].characterData.GetData().currentSP = Mathf.CeilToInt(Mathf.Clamp(
            order[currentOrderIdx].characterData.GetData().currentSP + order[currentOrderIdx].characterData.maxSP / 10.0f,
            0,
            order[currentOrderIdx].characterData.maxSP
        ));
        if (order[currentOrderIdx].isPlayer) gui.GetPlayerIcon(order[currentOrderIdx].characterData.GetData().ID).UpdateIcon();

        yield return new WaitForSeconds(1.0f);

        yield return Routine_Status(true);
        routineAttack = null;
        EndTurn();
    }

    /// <summary>
    /// Use an item on multiple targets
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="targets">The targets</param>
    /// <param name="isFromInventory">True if the item is from the inventory</param>
    public void UseItemOn(RPGItem item, List<CharacterData> targets, bool isFromInventory)
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_UseItemOn(item, targets, isFromInventory));
    }

    /// <summary>
    /// Use an item on multiple targets (Internal routine)
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="targets">The targets</param>
    /// <param name="isFromInventory">True if the item is from the inventory</param>
    private IEnumerator Routine_UseItemOn(RPGItem item, List<CharacterData> targets, bool isFromInventory)
    {
        if (isFromInventory)
        {
            GameManager.GetRPGManager().AddItemToInventory(item.ID, -1);
        }

        RPGItem currentWeapon = !string.IsNullOrEmpty(order[currentOrderIdx].characterData.GetData().weapon) ?
            GameManager.GetRPGManager().GetItem(order[currentOrderIdx].characterData.GetData().weapon) : null;


        bool isHealing = item.damageType == RPGItem.DamageType.HEAL || item.damageType == RPGItem.DamageType.HEAL_DEAD;
        bool canResurect = item.damageType == RPGItem.DamageType.HEAL_DEAD;
        order[currentOrderIdx].characterData.AddSP(-(int)item.costSP);


        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, Locals.GetLocal(item.ID + "_name"), false);
        gui.GetActionText().SetNewKey("battle_action_attack");
        gui.SetActionTextVisible(true);

        SetCameraTargetToCurrentPlayer();
        if (order[currentOrderIdx].isPlayer) gui.GetPlayerIcon(order[currentOrderIdx].characterData.GetData().ID).UpdateIcon();
        if (!isFromInventory && currentWeapon && !string.IsNullOrEmpty(currentWeapon.audioName) && audioData.attackSounds.ContainsKey(currentWeapon.audioName)) source.PlayOneShot(audioData.attackSounds[currentWeapon.audioName]);
        else if (!isFromInventory && !currentWeapon) source.PlayOneShot(audioData.attackSounds["Hand"]);
        order[currentOrderIdx].characterVisual.PlayAnimation(item.animationName);
        yield return new WaitForSeconds(1f);


        int damage = item.attackEquation == RPGItem.EquationType.REPLACE ?
            (int)item.attackValue // Attack is set
            : Mathf.RoundToInt(order[currentOrderIdx].characterData.attack * (item.attackValue + Random.Range(-0.1f, 0.1f))); // Defense is set

        int refValue = damage;
        if (order[currentOrderIdx].status.Find(status => status.status == RPGCharacterData.StatusType.ATTACK_UP) != null) damage += Mathf.RoundToInt(refValue * 0.5f);
        if (order[currentOrderIdx].status.Find(status => status.status == RPGCharacterData.StatusType.ATTACK_DOWN) != null) damage -= Mathf.RoundToInt(refValue * 0.5f);
        if (isHealing) damage = -damage;

        int defense;

        foreach (CharacterData data in targets)
        {
            // Evasion
            if (!isHealing && CanEvade(data) && Random.Range(0.0f, 1.0f) <= data.characterData.evasion)
            {
                SetCameraTarget(data.characterVisual.GetCameraTarget());
                yield return new WaitForSeconds(0.25f);
                data.characterVisual.TriggerEvassion();
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (isHealing) defense = 0; // No resistance on heal
            else if (item.defenseEquation == RPGItem.EquationType.REPLACE) defense = (int)item.defenseValue; // Defense is set
            else defense = Mathf.RoundToInt(data.characterData.defense * item.defenseValue) * (data.blocking ? 2 : 1); // Defense is normal

            refValue = defense;
            if (data.status.Find(status => status.status == RPGCharacterData.StatusType.DEFENSE_UP) != null) defense += Mathf.RoundToInt(refValue * 0.5f);
            if (data.status.Find(status => status.status == RPGCharacterData.StatusType.DEFENSE_DOWN) != null) defense -= Mathf.RoundToInt(refValue * 0.5f);

            int actualDamage = Mathf.Clamp(damage - defense, isHealing ? -999 : 2, 999);
            print(actualDamage + "(" + damage + "/" + defense + ")");

            SetCameraTarget(data.characterVisual.GetCameraTarget());
            yield return new WaitForSeconds(0.25f);

            if (data.dead && canResurect)
            {
                data.dead = false;
                data.characterVisual.PlayAnimation("Idle");
            }

            // Actual Attack
            data.characterData.AddHealth(-actualDamage);
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            if (!isHealing && data.status.Find(entry => entry.status == RPGCharacterData.StatusType.SLEEP) == null)
            {
                source.PlayOneShot(audioData.hitSound);
                data.characterVisual.TriggerDamage();
            }

            data.characterVisual.SetHealthBarVisible(true);
            data.characterVisual.setHealthBarFillAmount(data.characterData.currentHealth / (float)data.characterData.maxHealth, false);
            yield return new WaitForEndOfFrame();
            while (data.characterVisual.healthBarFilling)
            {
                yield return new WaitForEndOfFrame();
            }

            // Death
            if (data.characterData.currentHealth == 0)
            {
                data.dead = true;
                data.blocking = false;
                ClearStatus(data);
                data.characterVisual.SetBlocking(false);
                data.characterVisual.TriggerDeath();
                if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            }
            else
            {
                // Status 
                if (item.statusEffect != RPGItem.StatusEffect.NOTHING && Random.Range(0.0f, 1.0f) <= item.statusChance)
                {
                    int index = data.status.FindIndex(entry => entry.status == item.linkedStatus);
                    string otherName = data.characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(data.characterData.GetData().ID + "_name");

                    if (item.statusEffect == RPGItem.StatusEffect.CURE)
                    {
                        if (index != -1)
                        {
                            data.status.RemoveAt(index);
                            data.characterVisual.UpdateIcon(item.linkedStatus, 0f, false);
                            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateStatusIcon(item.linkedStatus, 0, false);

                            gui.GetActionText().SetParameters("", " ", " ", "");
                            gui.GetActionText().SetValue(otherName, Locals.GetLocal("ailement_" + item.linkedStatus.ToString().ToLower()), false);
                            gui.GetActionText().SetNewKey("battle_action_cured");
                            yield return new WaitForSeconds(1.0f);
                            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
                        }
                    }
                    else
                    {
                        if (index != -1)
                        {
                            data.status[index].remainingTurns += item.statusLength;
                            data.status[index].totalTurns += item.statusLength;
                            float fillAmount = data.status[index].remainingTurns / (float)data.status[index].totalTurns;
                            data.characterVisual.UpdateIcon(item.linkedStatus, fillAmount, false);
                            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateStatusIcon(item.linkedStatus, fillAmount, false);

                        }
                        else
                        {
                            if (item.linkedStatus == RPGCharacterData.StatusType.SLEEP) data.characterVisual.TriggerDeath();
                            data.status.Add(new StatusData { status = item.linkedStatus, remainingTurns = item.statusLength, totalTurns = item.statusLength });
                            data.characterVisual.AddIcon(item.linkedStatus);
                            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).AddStatusIcon(item.linkedStatus);
                        }

                        gui.GetActionText().SetParameters("", " ", " ", "");
                        gui.GetActionText().SetValue(otherName, Locals.GetLocal("ailement_" + item.linkedStatus.ToString().ToLower()), false);
                        gui.GetActionText().SetNewKey("battle_action_ailement");
                        yield return new WaitForSeconds(1.0f);
                        if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
                    }
                }
            }

            yield return new WaitForSeconds(1f);

            data.characterVisual.SetHealthBarVisible(false);
        }

        yield return Routine_Status(false);
        routineAttack = null;
        EndTurn();
    }

    /// <summary>
    /// Routine for confusion
    /// </summary>
    private IEnumerator Routine_Confused()
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");

        yield return new WaitForSeconds(0.25f);
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_confused");
        gui.SetActionTextVisible(true);

        yield return new WaitForSeconds(1f);

        gui.SetActionTextVisible(false);

        List<CharacterData> data = new List<CharacterData>();
        List<CharacterData> candidates = new List<CharacterData>();
        foreach (CharacterData entity in order)
        {
            if (!entity.dead && entity != order[currentOrderIdx]) candidates.Add(entity);
        }

        if (candidates.Count > 0)
        {
            data.Add(candidates[Random.Range(0, candidates.Count)]);
            UseItemOn(defaultAttack, data, false);
        }
        else
        {
            BlockForTurn();
        }
    }

    /// <summary>
    /// Routine for sleeping
    /// </summary>
    private IEnumerator Routine_Sleep()
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");

        yield return new WaitForSeconds(0.25f);
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_sleep");
        gui.SetActionTextVisible(true);

        yield return new WaitForSeconds(1f);

        gui.SetActionTextVisible(false);

        yield return Routine_Status(false);
        routineAttack = null;
        EndTurn();
    }

    /// <summary>
    /// Gets available targets using an item 
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The targets</returns>
    public List<List<CharacterData>> GetAvailableTargets(RPGItem item)
    {
        List<List<CharacterData>> result = new List<List<CharacterData>>();

        List<CharacterData> allies = order[currentOrderIdx].isPlayer ? players : ennemies;
        List<CharacterData> foes = order[currentOrderIdx].isPlayer ? ennemies : players;

        List<CharacterData> current;
        switch (item.targetType)
        {
            case RPGItem.TargetType.ALL:
                current = new List<CharacterData>();
                foreach (CharacterData character in allies) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                foreach (CharacterData character in foes) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.ONEALLY:
                foreach (CharacterData character in allies)
                {
                    if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLALLY:
                current = new List<CharacterData>();
                foreach (CharacterData character in allies) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.ONEFOE:
                foreach (CharacterData character in foes)
                {
                    if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLFOE:
                current = new List<CharacterData>();
                foreach (CharacterData character in foes) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.SELF:

                if ((!order[currentOrderIdx].dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (order[currentOrderIdx].dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                {
                    current = new List<CharacterData>();
                    current.Add(order[currentOrderIdx]);
                    result.Add(current);
                }

                break;
        }
        return result;
    }

    /// <summary>
    /// Routine for status effects at the end of a turn
    /// </summary>
    /// <param name="alreadyLookingAtTarget">True if the camera is already looking a the current player</param>
    private IEnumerator Routine_Status(bool alreadyLookingAtTarget)
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");


        CharacterData data = order[currentOrderIdx];
        int i = 0;
        bool poisoned = false;
        bool lookingAtTarget = alreadyLookingAtTarget;
        float fillAmount;
        while (i < data.status.Count)
        {
            data.status[i].remainingTurns--;
            fillAmount = data.status[i].remainingTurns / (float)data.status[i].totalTurns;
            data.characterVisual.UpdateIcon(data.status[i].status, fillAmount, false);
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateStatusIcon(data.status[i].status, fillAmount, false);
            if (data.status[i].remainingTurns <= 0)
            {

                if (!lookingAtTarget)
                {
                    lookingAtTarget = true;
                    SetCameraTargetToCurrentPlayer();
                    yield return new WaitForSeconds(0.25f);
                }

                if (data.status[i].status == RPGCharacterData.StatusType.SLEEP) data.characterVisual.PlayAnimation("Idle");

                gui.GetActionText().SetParameters("", " ", " ", "");
                gui.GetActionText().SetValue(playerName, Locals.GetLocal("ailement_" + data.status[i].status.ToString().ToLower()), false);
                gui.GetActionText().SetNewKey("battle_action_cured");
                gui.SetActionTextVisible(true);

                yield return new WaitForSeconds(1.0f);

                data.status.RemoveAt(i);
            }
            else
            {
                if (data.status[i].status == RPGCharacterData.StatusType.POISON)
                {
                    poisoned = true;
                }
                i++;
            }
        }

        if (poisoned)
        {
            if (!lookingAtTarget)
            {
                lookingAtTarget = true;
                SetCameraTargetToCurrentPlayer();
                yield return new WaitForSeconds(0.25f);
            }

            gui.GetActionText().SetParameters("", " ", " ", "");
            gui.GetActionText().SetValue(playerName, null, false);
            gui.GetActionText().SetNewKey("battle_action_poison");
            gui.SetActionTextVisible(true);

            data.characterData.AddHealth(-Mathf.RoundToInt(data.characterData.maxHealth * 0.1f));
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            data.characterVisual.SetHealthBarVisible(true);
            data.characterVisual.setHealthBarFillAmount(data.characterData.currentHealth / (float)data.characterData.maxHealth, false);
            yield return new WaitForEndOfFrame();
            while (data.characterVisual.healthBarFilling)
            {
                yield return new WaitForEndOfFrame();
            }

            if (data.characterData.currentHealth == 0)
            {
                data.dead = true;
                data.blocking = false;
                ClearStatus(data);
                data.characterVisual.SetBlocking(false);
                data.characterVisual.TriggerDeath();
                if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            }

            yield return new WaitForSeconds(1f);

            data.characterVisual.SetHealthBarVisible(false);
        }

        gui.SetActionTextVisible(false);
    }

    /// <summary>
    /// Checks if a character can evade
    /// </summary>
    /// <param name="data">The character's data</param>
    /// <returns>True if they can evade</returns>
    private bool CanEvade(CharacterData data)
    {
        foreach (StatusData status in data.status)
        {
            if (status.status == RPGCharacterData.StatusType.SLEEP) return false;
        }
        return true;
    }

    /// <summary>
    /// Clears the status for a character data
    /// </summary>
    /// <param name="data">The character data</param>
    private void ClearStatus(CharacterData data)
    {
        int size = data.status.Count;
        RPGCharacterData.StatusType status;
        while (size > 0)
        {
            status = data.status[size - 1].status;
            data.characterVisual.UpdateIcon(status, 0f, false);
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateStatusIcon(status, 0f, false);
            data.status.RemoveAt(size - 1);
            size--;
        }
    }

    /// <summary>
    /// Ends the current turn
    /// </summary>
    public void EndTurn()
    {


        gui.SetActionTextVisible(false);
        gui.SetPlayerIconActive(-1);
        gui.SetPlayerScreenActive(false);
        currentOrderIdx = (currentOrderIdx + 1) % order.Count;
        NextTurn();
    }


    /// <summary>
    /// Use the block action for this turn
    /// </summary>
    public void BlockForTurn()
    {
        order[currentOrderIdx].blocking = true;
        order[currentOrderIdx].characterVisual.SetBlocking(true);
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Block());
    }

    /// <summary>
    /// Handles an AI's turn
    /// </summary>
    /// <param name="data">The AI's data</param>
    private void HandleAI(CharacterData data)
    {
        if (Random.Range(0.0f, 1.0f) <= 0.1f)
        {
            BlockForTurn();
            return;
        }

        RPGItem selectedItem = null;
        List<CharacterData> targets = null;
        List<RPGItem> skills = new List<RPGItem>();
        skills.Add(defaultAttack);
        if (!string.IsNullOrEmpty(data.characterData.GetData().weapon))
        {
            RPGItem weapon = GameManager.GetRPGManager().GetItem(data.characterData.GetData().weapon);
            RPGItem skillInstance;
            foreach (string skill in weapon.weaponSkills)
            {
                skillInstance = GameManager.GetRPGManager().GetItem(skill);
                if (data.characterData.currentSP >= skillInstance.costSP) skills.Add(skillInstance);
            }
        }

        selectedItem = skills[Random.Range(0, skills.Count)];
        List<List<CharacterData>> allTargets = GetAvailableTargets(selectedItem);

        if (allTargets.Count == 1) targets = allTargets[0];
        else
        {
            int healthScore = int.MaxValue;
            int currentScore;

            foreach (List<CharacterData> possibleTarget in allTargets)
            {
                currentScore = 0;
                foreach (CharacterData ch in possibleTarget)
                {
                    currentScore += ch.characterData.currentHealth;
                }
                print(currentScore + " " + healthScore);
                if (currentScore < healthScore || (currentScore == healthScore && Random.Range(0, 2) == 0))
                {
                    healthScore = currentScore;
                    targets = possibleTarget;
                }
            }
        }

        print("AI Selected " + selectedItem.ID);
        UseItemOn(selectedItem, targets, false);
    }


    /// <summary>
    /// Generate the different characters
    /// </summary>
    private void GenerateCharacters()
    {
        virtualCamera.transform.position = new Vector3(posStart + posEnd / 2.0f, cameraUpDistance, ennemyDistance / 2.0f);

        List<int> playersIndex = GameManager.GetRPGManager().GetFollowers();
        RPGCharacter character;
        BattleCharacter visual;

        Vector3 from = new Vector3(posStart, 0, 0);
        Vector3 to = new Vector3(posEnd, 0, 0);
        int i = 0;
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        int avgLevel = 0;

        foreach (int index in playersIndex)
        {
            character = GameManager.GetRPGManager().GetCharacter(index);
            print(character.GetData().modelID + " " + character.GetData().ID);
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().modelID));
            visual.Init(statusSprites);
            visual.setHealthBarFillAmount(character.currentHealth / (float)character.maxHealth, true);
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / playersIndex.Count);
            visual.transform.rotation = rotation;

            avgLevel += character.GetData().level;

            i++;

            players.Add(new CharacterData
            {
                characterData = character,
                characterVisual = visual,
                dead = false,
                isPlayer = true,
                blocking = false
            });
        }

        avgLevel = Mathf.RoundToInt(avgLevel / (float)playersIndex.Count);

        from = new Vector3(posStart, 0, ennemyDistance);
        to = new Vector3(posEnd, 0, ennemyDistance);
        i = 0;
        rotation = Quaternion.Euler(0, 180, 0);

        foreach (RPGCharacterDataInterface dataInterface in currentData.ennemies)
        {
            character = new RPGCharacter(dataInterface.data.Clone(), dataInterface.constants);
            character.GetData().level = avgLevel;
            character.UpdateComputedStats();
            character.SetHealthToMax();
            character.SetSPToMax();
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().modelID));
            visual.Init(statusSprites);
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / currentData.ennemies.Length);
            visual.transform.rotation = rotation;

            i++;

            ennemies.Add(new CharacterData
            {
                characterData = character,
                characterVisual = visual,
                dead = false,
                isPlayer = false,
                blocking = false
            });
        }

        order.AddRange(players);
        order.AddRange(ennemies);
        order.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));
        players.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));
        ennemies.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));

        gui.SetPlayerIcons(players);
    }

    public class CharacterData
    {
        public RPGCharacter characterData;
        public BattleCharacter characterVisual;
        public bool dead;
        public bool isPlayer;
        public bool blocking;
        public List<StatusData> status;

        public CharacterData()
        {
            status = new List<StatusData>();
        }
    }

    public class StatusData
    {
        public RPGCharacterData.StatusType status;
        public int remainingTurns;
        public int totalTurns;
    }

    public class ItemData
    {
        public RPGItem item;
        public int amountInInventory;
    }

    [System.Serializable]
    public struct AudioData
    {
        public AudioClip hitSound;
        public SerializedDictionary<string, AudioClip> attackSounds;
    }
}
