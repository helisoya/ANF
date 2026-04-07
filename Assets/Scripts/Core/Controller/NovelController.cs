using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NovelController : MonoBehaviour
{

    public static NovelController instance;

    public bool isReadyForSaving
    {
        get
        {
            return waitingForUserToEndDialog || currentChoice != null || inInteractionMode;
        }
    }

    private List<string> data = new List<string>();
    private string activeChapterFile = "";
    private bool next = false;
    private bool handlingChapterFile = false;
    private int chapterProgress = 0;
    private Choice currentChoice;
    private bool waitingForUserToEndDialog;
    private bool inInteractionMode;
    private bool loadedAutoSave;

    private List<StackEntry> stack;

    /// <summary>
    /// Represents an entry in the chapters stack
    /// </summary>
    [System.Serializable]
    public class StackEntry
    {
        public string chapterName = "";
        public int currentChapterProgress = 0;
    }



    public void Next()
    {
        next = true;
    }

    public void EnableLock()
    {
        handlingChapterFile = false;
    }

    public void ClearStack()
    {
        while (stack.Count > 1) stack.RemoveAt(0);
    }

    void Start()
    {
        instance = this;
        stack = new List<StackEntry>();
        print("Is Loading Save : " + GameManager.instance.IsLoadingSave());
        if (GameManager.instance.IsLoadingSave())
        {
            LoadGameFile(GameManager.instance.GetSaveToLoad());
        }
        else
        {
            LoadChapterFile(GameManager.instance.GetNextChapter());
        }
    }

    public void LoadGameFile(string saveName = "save")
    {
        GAMEFILE activeGameFile = GameManager.GetSaveManager().Load(saveName);

        Locals.SetFiles(activeGameFile.localFiles);

        if (activeGameFile.inDungeon)
        {
            GameManager.instance.SetSaveToLoad(null);
            GameManager.instance.SetCurrentLocation(activeGameFile.locationName);
            GameManager.GetRPGManager().LoadCharactersFromList(activeGameFile.rpgCharacters);
            GameManager.GetRPGManager().SetFollowers(activeGameFile.followers);
            GameManager.GetRPGManager().SetInventory(activeGameFile.inventory);
            GameManager.GetRPGManager().SetMoney(activeGameFile.money);
            GameManager.GetRPGManager().SetNextDungeon(Resources.Load<DungeonData>("RPG/Dungeons/" + activeGameFile.dungeonID), activeGameFile.dungeonFloor);
            SceneManager.LoadScene("Dungeon");
            return;
        }


        GameManager.instance.SetSaveToLoad(saveName);
        GameManager.instance.SetCurrentLocation(activeGameFile.locationName);

        VNGUI.instance.ForceBgTo(activeGameFile.fadeBg);
        VNGUI.instance.ForceFgTo(activeGameFile.fadeFg);
        VNGUI.instance.SetBgColor(activeGameFile.colorBg);
        VNGUI.instance.SetFgColor(activeGameFile.colorFg);
        InteractionManager.instance.SetActive(false);

        CameraController.instance.SetPosition(activeGameFile.cameraPosition, true);
        CameraController.instance.SetRotation(activeGameFile.cameraRotation, true);

        LightingManager.instance.ChangeData(activeGameFile.skyData);

        stack = activeGameFile.stack;
        StackEntry currentChapter = stack[stack.Count - 1];

        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{currentChapter.chapterName}"));
        activeChapterFile = currentChapter.chapterName;

        CharacterManager.instance.RemoveAllCharacters();
        List<GAMEFILE.CHARACTERDATA> characters = activeGameFile.characterInScene;
        foreach (GAMEFILE.CHARACTERDATA character in characters)
        {
            CharacterManager.instance.AddCharacterFromData(character);
        }

        if (activeGameFile.background != null)
        {
            BackgroundManager.instance.ReplaceBackground(activeGameFile.background, false);
        }

        if (activeGameFile.music != null)
            AudioManager.instance.PlaySong(activeGameFile.music);

        currentChoice = activeGameFile.currentChoice;
        if (currentChoice.answers.Count == 0) currentChoice = null;

        inInteractionMode = activeGameFile.interactionMode;
        InteractionManager.instance.Load(activeGameFile.interactables);

        if (inInteractionMode)
        {
            DialogSystem.instance.Close();
        }
        else
        {
            DialogSystem.instance.Open(activeGameFile.currentTextSystemSpeakerDisplayText,
                activeGameFile.currentTextsIds);
        }

        GameManager.GetRPGManager().LoadCharactersFromList(activeGameFile.rpgCharacters);
        GameManager.GetRPGManager().SetFollowers(activeGameFile.followers);
        GameManager.GetRPGManager().SetInventory(activeGameFile.inventory);
        GameManager.GetRPGManager().SetMoney(activeGameFile.money);

        loadedAutoSave = saveName.Equals("auto");

        LoadChapterFile(currentChapter.chapterName, currentChapter.currentChapterProgress);
    }

    public void SaveGameFile(string saveName)
    {
        GAMEFILE activeGameFile = GameManager.GetSaveManager().saveFile;

        activeGameFile.stack = stack;

        activeGameFile.currentTextsIds = DialogSystem.instance.currentTextsIds;
        activeGameFile.currentTextSystemSpeakerDisplayText = DialogSystem.instance.speakerNameText.text;

        activeGameFile.characterInScene = CharacterManager.instance.SaveCharacters();

        activeGameFile.background = BackgroundManager.instance.currentBackground ?
            BackgroundManager.instance.currentBackground.backgroundName : null;

        activeGameFile.music = AudioManager.activeSong != null ? AudioManager.activeSong.clipName : "";

        activeGameFile.currentChoice = currentChoice;

        activeGameFile.fadeBg = VNGUI.instance.fadeBgAlpha;
        activeGameFile.fadeFg = VNGUI.instance.fadeFgAlpha;

        activeGameFile.colorBg = VNGUI.instance.fadeBgColor;
        activeGameFile.colorFg = VNGUI.instance.fadeFgColor;

        activeGameFile.interactionMode = inInteractionMode;
        activeGameFile.interactables = InteractionManager.instance.GetSaveData();

        activeGameFile.cameraPosition = CameraController.instance.targetPosition;
        activeGameFile.cameraRotation = CameraController.instance.targetRotation;

        activeGameFile.skyData = LightingManager.instance.GetCurrentDataName();

        activeGameFile.rpgCharacters = GameManager.GetRPGManager().GetCharacters();
        activeGameFile.inventory = GameManager.GetRPGManager().GetInventory();
        activeGameFile.followers = GameManager.GetRPGManager().GetFollowers();
        activeGameFile.money = GameManager.GetRPGManager().money;
        activeGameFile.locationName = GameManager.instance.currentLocation;

        activeGameFile.inDungeon = false;

        activeGameFile.localFiles = Locals.currentFiles;

        GameManager.GetSaveManager().Save(saveName);
    }

    public void LoadChapterFile(string filename, int chapterProgress = 0)
    {
        print("Loading chapter : " + $"Story/{filename}");
        StopAllCoroutines();
        handlingChapterFile = false;

        if (!GameManager.instance.IsLoadingSave())
        {
            inInteractionMode = false;
            stack.Add(new StackEntry { chapterName = filename, currentChapterProgress = chapterProgress });
            while (stack.Count > 10)
            {
                stack.RemoveAt(0);
            }
        }
        print("Current Stack :");
        foreach (StackEntry entry in stack)
        {
            print("- " + entry.chapterName + " : " + entry.currentChapterProgress);
        }
        activeChapterFile = filename;
        this.chapterProgress = chapterProgress;

        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{filename}"));

        StartCoroutine(HandlingChapterFile());
    }




    IEnumerator HandlingChapterFile()
    {
        yield return new WaitForEndOfFrame();
        handlingChapterFile = true;

        if (GameManager.instance.IsLoadingSave())
        {
            GameManager.instance.SetSaveToLoad(null);

            if (currentChoice != null)
            {
                yield return HandleChoice(currentChoice);
            }
            else if (inInteractionMode)
            {
                yield return HandleInteraction();
            }
            else if (!loadedAutoSave)
            {
                waitingForUserToEndDialog = true;
                next = false;
                while (!next)
                {
                    yield return new WaitForEndOfFrame();
                }
                next = false;
                waitingForUserToEndDialog = false;
            }
            chapterProgress++;
            stack[stack.Count - 1].currentChapterProgress++;
        }

        ChoiceScreen.instance.Hide();

        while (chapterProgress < data.Count)
        {

            string line = data[chapterProgress].Replace("\t", "");

            if (line.Equals("interact"))
            {
                yield return HandleInteraction();
            }
            else if (line.StartsWith("choice"))
            {
                yield return HandlingChoiceLine(line);
            }
            else if (line.StartsWith("if"))
            {
                yield return HandlingIf(line);
            }
            else if (line.StartsWith("switch"))
            {
                yield return HandlingSwitch(line);
            }
            else if (line.StartsWith("else"))
            {
                if (line.EndsWith("{"))
                {
                    chapterProgress = CloseBrackets(chapterProgress, 1);
                    stack[stack.Count - 1].currentChapterProgress = chapterProgress;
                }
            }
            else
            {
                yield return HandlingLine(line);
            }

            if (!handlingChapterFile) yield break;

            chapterProgress++;
            stack[stack.Count - 1].currentChapterProgress++;
        }

        if (!handlingChapterFile) yield break;

        print("Removign from stack");
        stack.RemoveAt(stack.Count - 1);
        if (stack.Count > 0)
        {
            print("Reload previous entry");
            StackEntry entry = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            LoadChapterFile(entry.chapterName, entry.currentChapterProgress);
        }

        handlingChapterFile = false;
        yield break;
    }

    IEnumerator HandleInteraction()
    {
        print("Starting interaction mode");
        ClearStack();
        inInteractionMode = true;
        stack[stack.Count - 1].currentChapterProgress++;
        InteractionManager.instance.SetActive(true);
        while (InteractionManager.instance.active)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!GameManager.instance.IsLoadingSave()) inInteractionMode = false;
    }

    /// <summary>
    /// Find the line of the closing brackets in the current file
    /// </summary>
    /// <param name="ifStart">The line where the first open bbracket is</param>
    /// <param name="startAmount">The amount of brackets to close at start</param>
    /// <returns>The line where the closing bracket is</returns>
    private int CloseBrackets(int ifStart, int startAmount = 1)
    {
        int amountToClose = startAmount;
        string line;
        ifStart++;
        while (amountToClose > 0 && ifStart < data.Count)
        {
            line = data[ifStart].Replace("\t", "");
            if ((line.StartsWith("if") || line.StartsWith("else"))
                && line.EndsWith("{")) amountToClose++;
            else if (line.StartsWith("}")) amountToClose--;
            ifStart++;
        }
        return ifStart - 1;
    }

    IEnumerator HandlingSwitch(string line)
    {
        yield return new WaitForEndOfFrame();

        string variablename = line.Split(" ")[1];
        string variableValue = GameManager.GetSaveManager().GetItem(variablename);
        string whatToLoadAfter = null;
        string defaultLoad = null;

        int i = chapterProgress + 1;
        while (i < data.Count && !string.IsNullOrEmpty(data[i]) && !string.IsNullOrWhiteSpace(data[i]) && data[i].StartsWith("\t"))
        {
            string choiceLine = data[i].Replace("\t", "");
            string[] split = choiceLine.Split(' ');
            print("Switch (" + variableValue + ") : " + split[0] + " -> " + split[1] + " ");
            if (split[0] == variableValue)
            {
                whatToLoadAfter = split[1];
            }
            else if (split[0] == "default")
            {
                defaultLoad = split[1];
            }
            i++;
        }

        stack[stack.Count - 1].currentChapterProgress = i;
        chapterProgress = i;

        if (whatToLoadAfter == null && defaultLoad != null) whatToLoadAfter = defaultLoad;
        if (whatToLoadAfter != null)
        {
            handlingChapterFile = false;
            stack[stack.Count - 1].currentChapterProgress++;
            print("Switch -> " + whatToLoadAfter);
            LoadChapterFile(whatToLoadAfter);
        }
    }

    IEnumerator HandlingIf(string line)
    {
        yield return new WaitForEndOfFrame();

        // if(KEY = VALUE & KEY = VALUE) NEW_CHAPTER
        string[] split = line.Split(new char[] { '(', ')' });

        bool ok = true;
        bool isAnd = true;
        bool tmpResult;

        string[] splitAnds = split[1].Split(" & ");
        if (splitAnds.Length == 1)
        {
            ok = false;
            isAnd = false;
            splitAnds = split[1].Split(" | ");
        }

        foreach (string splitAnd in splitAnds)
        {
            string[] parametersSplit = splitAnd.Split(' ');
            string key = parametersSplit[0];
            string oper = parametersSplit[1];
            string value = parametersSplit[2];

            tmpResult = IsCheckOkay(key, value, oper);

            print(splitAnd + " -> " + tmpResult);

            if (!tmpResult && isAnd)
            {
                ok = false;
                break;
            }
            else if (tmpResult && !isAnd)
            {
                ok = true;
                break;
            }
        }

        print("Handling |" + line + "| with result : " + ok);

        bool multiCommands = split[2].StartsWith("{");

        if (ok)
        {
            if (!multiCommands)
            {
                print("Executing  (true) command : " + split[2]);
                split[2] = split[2].Replace(" ", "");
                if (split.Length > 3)
                {
                    yield return HandlingLine(split[2] + "(" + split[3] + ")");
                }
                else
                {
                    handlingChapterFile = false;
                    stack[stack.Count - 1].currentChapterProgress++;
                    LoadChapterFile(split[2]);
                    yield break;
                }
            }
        }
        else if (multiCommands)
        {
            // If multi commands, skip the unaccessible code
            chapterProgress = CloseBrackets(chapterProgress);
            stack[stack.Count - 1].currentChapterProgress = chapterProgress;
            print("Skiping  (true) commands up to : " + data[chapterProgress - 1] + data[chapterProgress]);
        }

        // Handle else

        if (chapterProgress < data.Count - 1 && data[chapterProgress + 1].Replace("\t", "").StartsWith("else"))
        {
            chapterProgress++;

            if (ok)
            {
                // Do not process the else, skip it
                if (data[chapterProgress].EndsWith("{"))
                {
                    // If multi commands, skip the unaccessible code
                    chapterProgress = CloseBrackets(chapterProgress);
                    stack[stack.Count - 1].currentChapterProgress = chapterProgress;
                    print("Skiping  (false) commands up to : " + data[chapterProgress - 1] + data[chapterProgress]);
                }
                else
                {
                    print("Skiping  (false) command : " + data[chapterProgress]);
                }

            }
            else if (!data[chapterProgress].EndsWith("{"))
            {
                // Process the else
                split = data[chapterProgress].Replace("\t", "").Split("else ");

                split[1] = split[1].Replace(" ", "");

                print("Executing  (false) command : " + split[1]);

                if (split[1].Contains("("))
                {
                    yield return HandlingLine(split[1]);
                }
                else
                {
                    handlingChapterFile = false;
                    stack[stack.Count - 1].currentChapterProgress++;
                    LoadChapterFile(split[1]);
                }
            }
        }
    }

    bool IsCheckOkay(string key, string value, string oper)
    {
        string currentValue = GameManager.GetSaveManager().GetItem(key);

        switch (oper)
        {
            case "=":
                return currentValue.Equals(value);
            case ">":
                return int.Parse(currentValue) > int.Parse(value);
            case "<":
                return int.Parse(currentValue) < int.Parse(value);
            case ">=":
                return int.Parse(currentValue) >= int.Parse(value);
            case "<=":
                return int.Parse(currentValue) <= int.Parse(value);
            case "!=":
                return int.Parse(currentValue) != int.Parse(value);
        }

        return false;
    }


    IEnumerator HandlingChoiceLine(string line)
    {
        // Choice ID_QUESTION
        // ID_REP NEXTCHAPTER
        currentChoice = new Choice(line.Split(' ')[1]);

        int i = chapterProgress + 1;
        while (i < data.Count && !string.IsNullOrEmpty(data[i]) && !string.IsNullOrWhiteSpace(data[i]) && data[i].StartsWith("\t"))
        {
            string choiceLine = data[i].Replace("\t", "");
            string[] split = choiceLine.Split(' ');
            print(choiceLine + " -> " + split[0] + " " + split[1] + " " + split.Length);
            currentChoice.answers.Add(new Choice.ChoiceAnswer(split[0], split[1]));
            i++;
        }

        chapterProgress = i;
        stack[stack.Count - 1].currentChapterProgress = i;

        if (currentChoice.answers.Count > 0)
        {
            yield return HandleChoice(currentChoice);
        }
    }

    IEnumerator HandleChoice(Choice choice)
    {
        ChoiceScreen.instance.Show(choice);

        yield return new WaitForEndOfFrame();

        while (ChoiceScreen.instance.isWaitingForChoiceToBeMade)
        {
            yield return new WaitForEndOfFrame();
        }

        string action = choice.answers[ChoiceScreen.instance.chosenIndex].action;
        currentChoice = null;

        if (action.StartsWith("Map"))
        {
            string[] parameters = action.Split(new char[] { '(', ')' })[1].Split(';');
            Map.instance.OpenMap(parameters[0], parameters[1]);
        }
        else
        {
            handlingChapterFile = false;
            LoadChapterFile(action);
        }
    }

    public IEnumerator HandlingLine(string line, bool isQuickCommand = false)
    {
        if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith("}")) yield break;

        line = line.Replace("\t", "");

        string[] data = line.Split(new char[] { '(', ')' });
        if (data.Length < 2) yield break;

        string[] parameters = data[1].Split(";");

        switch (data[0])
        {
            case "dialog":
                if (isQuickCommand) break;
                next = false;

                // Speaker - CharacterModel - additive - dialog
                DialogSystem.instance.OpenAllRequirementsForDialogueSystemVisibility(true);
                DialogSystem.instance.Say(parameters[3], parameters[0], parameters[1].Equals("_") ? null : parameters[1], bool.Parse(parameters[2]));

                TextArchitect architect = DialogSystem.instance.textArchitect;

                while (architect.isConstructing)
                {
                    if (next)
                    {
                        next = false;
                        architect.skip = true;
                    }
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForEndOfFrame();

                waitingForUserToEndDialog = true;
                while (!next)
                {
                    yield return new WaitForEndOfFrame();
                }
                waitingForUserToEndDialog = false;

                break;

            case "setBackground":
                BackgroundManager.instance.ReplaceBackground(parameters[0], parameters.Length > 1 ? bool.Parse(parameters[1]) : true);
                break;

            case "playSound":
                AudioClip clip = Resources.Load("Audio/SFX/" + parameters[0]) as AudioClip;
                if (clip != null)
                {
                    AudioManager.instance.PlaySFX(clip);
                }
                break;

            case "playMusic":
                AudioManager.instance.PlaySong(parameters[0]);
                break;

            case "clearStack":
                ClearStack();
                break;

            case "removeAllCharacters":
                CharacterManager.instance.RemoveAllCharacters();
                break;

            case "addCharacter":
                CharacterManager.instance.AddCharacter(parameters[0], bool.Parse(parameters[1]));
                break;

            case "removeCharacter":
                CharacterManager.instance.RemoveCharacter(parameters[0]);
                break;

            case "setCharacterPosition":
                CharacterManager.instance.SetCharacterPosition(
                    parameters[0],
                    BackgroundManager.instance.GetMarkerPosition(parameters[1])
                );
                break;

            case "setCharacterRotation":
                float angle;
                if (!float.TryParse(parameters[1], out angle))
                {
                    angle = BackgroundManager.instance.GetMarkerRotation(parameters[1]);
                }
                CharacterManager.instance.SetCharacterRotation(
                    parameters[0],
                    angle
                );
                break;

            case "setCharacterMouth":
                CharacterManager.instance.SetCharacterMouthAnimation(
                    parameters[0],
                    parameters[1]
                );
                break;

            case "setCharacterEye":
                CharacterManager.instance.SetCharacterEyeAnimation(
                    parameters[0],
                    parameters[1]
                );
                break;

            case "setCharacterBody":
                CharacterManager.instance.SetCharacterBodyAnimation(
                    parameters[0],
                    parameters[1],
                    bool.Parse(parameters[2])
                );
                break;

            case "setInteractionChapter":
                InteractionManager.instance.ChangeObjectChapter(parameters[0], parameters[1]);
                break;

            case "setInteractionHidden":
                InteractionManager.instance.SetObjectHidden(parameters[0], bool.Parse(parameters[1]));
                break;

            case "setCharacterAlpha":
                // Character - Target - Wait for end ?

                CharacterManager.instance.TransitionCharacterAlpha(parameters[0],
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture));

                if (bool.Parse(parameters[2]))
                {
                    yield return new WaitForEndOfFrame();
                    while (CharacterManager.instance.IsCharacterFading(parameters[0]))
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }

                break;

            case "autosave":
                SaveGameFile("auto");
                break;

            case "flash":

                VNGUI.instance.FlashTo(1, 10);

                yield return new WaitForSeconds(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));

                VNGUI.instance.FlashTo(0, 10);
                break;

            case "setBgColor":

                VNGUI.instance.SetBgColor(new Color(
                    float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                ));
                break;

            case "setFgColor":

                VNGUI.instance.SetFgColor(new Color(
                    float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                ));
                break;

            case "fadeBg":
                float speedBg = parameters.Length > 2 ? float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture) : 2;
                VNGUI.instance.FadeBgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture), speedBg);
                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.fadingBg)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "fadeFg":
                float speedFg = parameters.Length > 2 ? float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture) : 2;
                VNGUI.instance.FadeFgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture), speedFg);
                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.fadingFg)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "setFg":
                VNGUI.instance.ForceFgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));
                break;

            case "setBg":
                VNGUI.instance.ForceBgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));
                break;

            case "shake":
                CameraController.instance.SetShaking(
                    int.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture)
                );

                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (CameraController.instance.isShaking)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "cameraPosition":
                // x ; y ; z ; immediate ; waitForEnd
                // default ; immediate ; waitForEnd

                int idx = 1;
                Vector3 position = new Vector3(0, 0, -10);
                if (!parameters[0].Equals("default"))
                {
                    position = new Vector3(
                        float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                    );
                    idx = 3;
                }

                CameraController.instance.SetPosition(
                    position,
                    bool.Parse(parameters[idx])
                );

                if (bool.Parse(parameters[idx + 1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (!CameraController.instance.atTargetPosition)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "cameraRotation":
                // x ; y ; z ; immediate ; waitForEnd
                // default ; immediate ; waitForEnd

                int idxR = 1;
                Vector3 rotaiton = new Vector3(0, 0, 0);
                if (!parameters[0].Equals("default"))
                {
                    rotaiton = new Vector3(
                        float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                    );
                    idxR = 3;
                }

                CameraController.instance.SetRotation(
                    rotaiton,
                    bool.Parse(parameters[idxR])
                );

                if (bool.Parse(parameters[idxR + 1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (!CameraController.instance.atTargetRotation)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "load":
                if (isQuickCommand) break;
                stack[stack.Count - 1].currentChapterProgress++;
                LoadChapterFile(parameters[0]);
                break;

            case "mainMenu":
                if (isQuickCommand) break;
                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                break;

            case "loadScene":
                if (isQuickCommand) break;
                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene(parameters[0]);
                break;

            case "voice":
                clip = Resources.Load("Audio/Voice/" + parameters[0]) as AudioClip;
                if (clip != null)
                {
                    AudioManager.instance.PlayVoice(clip);
                }
                break;

            case "map":
                if (isQuickCommand) break;
                Map.instance.OpenMap(parameters[0], parameters[1]);
                yield return new WaitForEndOfFrame();
                while (Map.instance.open)
                {
                    yield return new WaitForEndOfFrame();
                }
                break;

            case "battle":
                if (isQuickCommand) break;

                GameManager.GetRPGManager().SetNextBattleEncounter(
                    Resources.Load<BattleData>("RPG/Battles/Data/" + parameters[0]),
                    parameters[1],
                    BattleData.CloseType.VN,
                    parameters[2]);

                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
                break;

            case "dungeon":
                if (isQuickCommand) break;

                GameManager.GetRPGManager().SetNextDungeon(
                    Resources.Load<DungeonData>("RPG/Dungeons/" + parameters[0]),
                    int.Parse(parameters[1]));

                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
                break;

            case "addItem":
                GameManager.GetRPGManager().AddItemToInventory(parameters[0], int.Parse(parameters[1]));
                break;

            case "addFollower":
                GameManager.GetRPGManager().AddFollower(parameters[0]);
                break;

            case "removeFollower":
                GameManager.GetRPGManager().RemoveFollower(parameters[0]);
                break;

            case "healFollower":
                RPGCharacter follower = GameManager.GetRPGManager().GetCharacter(parameters[0]);
                if (follower != null)
                {
                    follower.SetHealthToMax();
                    follower.SetSPToMax();
                }
                break;

            case "healAllFollowers":
                foreach (int followerIndex in GameManager.GetRPGManager().GetFollowers())
                {
                    RPGCharacter follower2 = GameManager.GetRPGManager().GetCharacter(followerIndex);
                    follower2.SetHealthToMax();
                    follower2.SetSPToMax();
                }
                break;

            case "addGold":
                GameManager.GetRPGManager().AddMoney(int.Parse(parameters[0]));
                break;

            case "addExp":
                RPGCharacter character = GameManager.GetRPGManager().GetCharacter(parameters[0]);
                character.GetData().exp += int.Parse(parameters[1]);
                if (character.canLevelUp)
                {
                    VNGUI.instance.OpenLevelUpMenu(character);
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.levelingUp) yield return new WaitForEndOfFrame();
                }

                break;

            case "addExpToAll":
                List<int> followers = GameManager.GetRPGManager().GetFollowers();
                RPGCharacter follower3;
                foreach (int idxFollower in followers)
                {
                    follower3 = GameManager.GetRPGManager().GetCharacter(idxFollower);
                    follower3.GetData().exp += int.Parse(parameters[0]);
                    if (follower3.canLevelUp)
                    {
                        VNGUI.instance.OpenLevelUpMenu(follower3);
                        yield return new WaitForEndOfFrame();
                        while (VNGUI.instance.levelingUp) yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "shop":
                ShopData shopData = Resources.Load<ShopData>("RPG/Shops/" + parameters[0]);
                if (shopData)
                {
                    VNGUI.instance.OpenShop(shopData);
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.shopOpen) yield return new WaitForEndOfFrame();
                }
                break;

            case "forceEquipWeapon":
                RPGCharacter characterToEquipWeapon = GameManager.GetRPGManager().GetCharacter(parameters[0]);
                string itemID = parameters[1];
                bool addIfNoneInInventory = bool.Parse(parameters[2]);
                int amountInInventory = GameManager.GetRPGManager().GetAmountInInventory(itemID);
                if (characterToEquipWeapon != null && (amountInInventory != 0 || addIfNoneInInventory))
                {
                    if (!string.IsNullOrEmpty(characterToEquipWeapon.GetData().weapon)) GameManager.GetRPGManager().AddItemToInventory(characterToEquipWeapon.GetData().weapon, 1);
                    if (amountInInventory > 0) GameManager.GetRPGManager().AddItemToInventory(itemID, -1);
                    characterToEquipWeapon.GetData().weapon = itemID;
                    characterToEquipWeapon.UpdateComputedStats();
                }
                break;

            case "forceEquipArmor":
                RPGCharacter characterToEquipArmor = GameManager.GetRPGManager().GetCharacter(parameters[0]);
                string itemIDArmor = parameters[1];
                bool addIfNoneInInventoryArmor = bool.Parse(parameters[2]);
                int amountInInventoryArmor = GameManager.GetRPGManager().GetAmountInInventory(itemIDArmor);
                if (characterToEquipArmor != null && (amountInInventoryArmor != 0 || addIfNoneInInventoryArmor))
                {
                    if (!string.IsNullOrEmpty(characterToEquipArmor.GetData().armor)) GameManager.GetRPGManager().AddItemToInventory(characterToEquipArmor.GetData().armor, 1);
                    if (amountInInventoryArmor > 0) GameManager.GetRPGManager().AddItemToInventory(itemIDArmor, -1);
                    characterToEquipArmor.GetData().armor = itemIDArmor;
                    characterToEquipArmor.UpdateComputedStats();
                }
                break;

            case "alignToCharacterLevel":
                RPGCharacter characterToAlign = GameManager.GetRPGManager().GetCharacter(parameters[0]);
                RPGCharacter characterToAlignWith = GameManager.GetRPGManager().GetCharacter(parameters[1]);
                if (characterToAlign != null && characterToAlignWith != null)
                {
                    characterToAlign.LevelUpTo(characterToAlignWith.GetData().level);
                }
                break;

            case "addPointsToCharacter":
                GameManager.GetRPGManager().GetCharacter(parameters[0]).IncreaseStat(
                    Enum.Parse<RPGCharacterData.StatType>(parameters[1]),
                    int.Parse(parameters[2])
                );
                break;

            case "variable":
                GameManager.GetSaveManager().EditVariable(parameters[0], parameters[1]);
                break;

            case "random":
                GameManager.GetSaveManager().EditVariable("random",
                UnityEngine.Random.Range(int.Parse(parameters[0]), int.Parse(parameters[1])).ToString());
                break;

            case "changeSkybox":
                LightingManager.instance.ChangeData(parameters[0]);
                break;

            case "setLocation":
                GameManager.instance.SetCurrentLocation(parameters[0]);
                break;

            case "return":
                chapterProgress = this.data.Count - 1;
                break;

            case "setLocals":
                Locals.SetFiles(parameters);
                break;

            case "unlockLog":
                if (!GameManager.GetSaveManager().HasUnlockedLog(parameters[0]))
                {
                    GameManager.GetSaveManager().UnlockLog(parameters[0]);

                    if (bool.Parse(parameters[1]))
                    {
                        VNGUI.instance.PlayNewLogAnimation();
                    }
                }
                break;

#if UNITY_EDITOR
            case "log":
                Debug.Log(parameters[0]);
                break;

            case "warn":
                Debug.LogWarning(parameters[0]);
                break;

            case "error":
                Debug.LogError(parameters[0]);
                break;
#endif

            case "wait":
                yield return new WaitForSeconds(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));
                break;
        }
    }

}
