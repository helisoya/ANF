using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GAMEFILE
{
    public List<NovelController.StackEntry> stack;

    public List<string> currentTextsIds;
    public string currentTextSystemSpeakerDisplayText = "";

    public List<CHARACTERDATA> characterInScene = null;

    public string background;

    public string music;

    public Choice currentChoice;

    public List<VARIABLE> variables;

    public float fadeBg;
    public float fadeFg;
    public Color colorBg;
    public Color colorFg;
    public bool interactionMode;
    public List<INTERACTABLEDATA> interactables;

    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public string skyData;

    public string locationName;

    public List<RPGCharacterData> rpgCharacters;
    public List<InventorySlot> inventory;
    public List<int> followers;
    public int money;

    public bool inDungeon;
    public string dungeonID;
    public int dungeonFloor;

    public string[] localFiles;
    public List<string> unlockedLogs;

    public GAMEFILE()
    {
        this.stack = new List<NovelController.StackEntry>();
        this.characterInScene = new List<CHARACTERDATA>();
        this.background = null;
        this.music = null;
        this.currentChoice = null;
        this.variables = new List<VARIABLE>();
        this.fadeBg = 0;
        this.fadeFg = 0;
        this.interactionMode = false;
        this.interactables = new List<INTERACTABLEDATA>();
        this.cameraPosition = new Vector3(0, 0, -10);
        this.cameraRotation = Vector3.zero;
        this.skyData = "Default";
        this.locationName = "NO_DATA";
        this.rpgCharacters = new List<RPGCharacterData>();
        this.inventory = new List<InventorySlot>();
        this.followers = new List<int>();
        this.inDungeon = false;
        this.dungeonID = null;
        this.dungeonFloor = 0;
        this.money = 0;
        this.localFiles = new string[0];
        this.unlockedLogs = new List<string>();
    }

    [System.Serializable]
    public class CHARACTERDATA
    {
        public string characterName;
        public string bodyAnimationSet = "";
        public string eyeAnimationSet = "";
        public string mouthAnimationSet = "";
        public Vector3 position;
        public float rotation;
        public float alpha;

        public CHARACTERDATA(Character character)
        {
            characterName = character.characterName;
            position = character.characterPosition;
            rotation = character.characterRotation;
            alpha = character.characterAlpha;
            bodyAnimationSet = character.bodyAnimationSet;
            eyeAnimationSet = character.eyeAnimationSet;
            mouthAnimationSet = character.mouthAnimationSet;
        }
    }

    [System.Serializable]
    public class INTERACTABLEDATA
    {
        public string ID;
        public string chapter;
        public bool hidden;

        public INTERACTABLEDATA(string ID, string chapter, bool hidden)
        {
            this.ID = ID;
            this.chapter = chapter;
            this.hidden = hidden;
        }
    }

    [System.Serializable]
    public class VARIABLE
    {
        public string name;
        public string value;
        public string defaultValue;

        public VARIABLE(string key, string defaultValue)
        {
            this.name = key;
            this.value = defaultValue;
            this.defaultValue = defaultValue;
        }
    }
}
