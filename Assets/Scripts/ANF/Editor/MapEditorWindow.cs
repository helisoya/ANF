using ANF.Persistent;
using Codice.Client.BaseCommands;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField] private Sprite defaultButtonSprite = default;
    [SerializeField] private Sprite defaultBackgroundSprite = default;
    [SerializeField] private MapsData maps = default;

    private List<ANF.Persistent.MapButton> currentButtons;
    private List<Image> currentButtonsOnMap;


    private TextField mapIdTextField;
    private Button saveButton;
    private Button newMapButton;
    private Button newButtonButton;
    private Button removeMapButton;

    private VisualElement buttonInfoRoot;
    private TextField buttonIdTextField;
    private Slider buttonPosXTextField;
    private Slider buttonPosYTextField;
    
    private Image backgroundImage;

    private ScrollView mapScrollView;
    private ScrollView buttonsScrollView;


    [MenuItem("ANF/Map Editor")]
    public static void ShowExample()
    {
        MapEditorWindow wnd = GetWindow<MapEditorWindow>();
        wnd.titleContent = new GUIContent("Map Editor");
    }

    public void CreateGUI()
    {
        currentButtons = new List<ANF.Persistent.MapButton>();
        currentButtonsOnMap = new List<Image>();

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        ///root.Add(label);

        // Instantiate UXML
        VisualElement uxmlData = m_VisualTreeAsset.Instantiate();
        root.Add(uxmlData);

        mapIdTextField = uxmlData.Q<TextField>("MapIDTextField");
        saveButton = uxmlData.Q<Button>("SaveButton");
        newMapButton = uxmlData.Q<Button>("NewMap");
        newButtonButton = uxmlData.Q<Button>("NewButton");
        removeMapButton = uxmlData.Q<Button>("RemoveButton");

        buttonInfoRoot = uxmlData.Q<VisualElement>("ButtonInfoRoot");
        buttonIdTextField = uxmlData.Q<TextField>("ButtonIDTextField");
        buttonPosXTextField = uxmlData.Q<Slider>("XSlider");
        buttonPosYTextField = uxmlData.Q<Slider>("YSlider");

        backgroundImage = uxmlData.Q<Image>("BackgroundImg");

        mapScrollView = uxmlData.Q<ScrollView>("ScrollViewMaps");
        buttonsScrollView = uxmlData.Q<ScrollView>("ScrollViewButtons");

        backgroundImage.RegisterCallback<DragUpdatedEvent>(OnBackgroundDrag);
        backgroundImage.RegisterCallback<DragPerformEvent>(OnBackgroundDrop);

        newMapButton.clicked += NewMap;
        removeMapButton.clicked += RemoveMap;
        saveButton.clicked += SaveMap;
        newButtonButton.clicked += NewButton;

        backgroundImage.sprite = defaultBackgroundSprite;

        RefreshMapScrollView();

        NewMap();
    }

    /// <summary>
    /// Creates a new map
    /// </summary>
    private void NewMap()
    {
        currentButtons.Clear();
        backgroundImage.Clear();
        currentButtonsOnMap.Clear();
        buttonInfoRoot.SetEnabled(false);

        RefreshButtonScrollView();
    }

    /// <summary>
    /// Removes the map from the pool
    /// </summary>
    private void RemoveMap()
    {
        if (!string.IsNullOrEmpty(mapIdTextField.text) && !string.IsNullOrWhiteSpace(mapIdTextField.text))
        {
            for(int i = 0; i < maps.maps.Count;i++)
            {
                if (maps.maps[i].id.Equals(mapIdTextField.text))
                {
                    maps.maps.RemoveAt(i);
                    EditorUtility.SetDirty(maps);
                }
            }
        }
        NewMap();
        RefreshMapScrollView();
    }

    /// <summary>
    /// Saves the map
    /// </summary>
    private void SaveMap()
    {
        if(!string.IsNullOrEmpty(mapIdTextField.text) && !string.IsNullOrWhiteSpace(mapIdTextField.text))
        {
            ANF.Persistent.MapData map = maps.GetMap(mapIdTextField.text);
            
            if(map == null)
            {
                map = new ANF.Persistent.MapData();
                map.id = mapIdTextField.text;
                maps.maps.Add(map);
            }

            map.backgroundSprite = backgroundImage.sprite;
            map.buttons.Clear();

            foreach(ANF.Persistent.MapButton button in currentButtons)
            {
                map.buttons.Add(new ANF.Persistent.MapButton() {
                    id=button.id, 
                    sprite=button.sprite
                });
            }

            EditorUtility.SetDirty(maps);
        }

        RefreshMapScrollView();
    }

    /// <summary>
    /// Creates a new button on screen
    /// </summary>
    private void NewButton()
    {
        int id = 0;
        while (ButtonNameExists(id.ToString()))
            id++;

        CreateButton(id.ToString(), defaultButtonSprite);

        RefreshButtonScrollView();
    }

    /// <summary>
    /// Refreshs the button scroll view
    /// </summary>
    private void RefreshMapScrollView()
    {
        mapScrollView.Clear();

        for (int i = 0; i < maps.maps.Count; i++)
        {
            Button instance = new Button();
            instance.text = maps.maps[i].id;
            instance.clicked += () => { OnMapClick(i); };
            mapScrollView.Add(instance);
        }
    }

    /// <summary>
    /// Callback when clicking on a map
    /// </summary>
    /// <param name="index">The map's index</param>
    private void OnMapClick(int index)
    {
        currentButtons.Clear();
        currentButtonsOnMap.Clear();
        backgroundImage.Clear();

        buttonInfoRoot.SetEnabled(false);
        ANF.Persistent.MapData newMap = maps.maps[index];

        mapIdTextField.SetValueWithoutNotify(newMap.id);
        backgroundImage.sprite = newMap.backgroundSprite;

        foreach (ANF.Persistent.MapButton button in newMap.buttons)
            CreateButton(button.id, button.sprite);

        RefreshButtonScrollView();
    }

    /// <summary>
    /// Refreshs the button scroll view
    /// </summary>
    private void RefreshButtonScrollView()
    {
        buttonsScrollView.Clear();

        for(int i = 0; i < currentButtons.Count;i++)
        {
            Button instance = new Button();
            instance.text = currentButtons[i].id;
            instance.clicked += () => { OnButtonClick(i); };
            buttonsScrollView.Add(instance);
        }
    }

    /// <summary>
    /// Callback when clicking on a button
    /// </summary>
    /// <param name="index">The button's index</param>
    private void OnButtonClick(int index)
    {
        buttonInfoRoot.SetEnabled(true);
    }

    /// <summary>
    /// Checks if a button id is already in use
    /// </summary>
    /// <param name="name">The button's id</param>
    /// <returns>True if it is already in use</returns>
    private bool ButtonNameExists(string id)
    {
        foreach (ANF.Persistent.MapButton button in currentButtons)
            if (button.id.Equals(id))
                return true;
        return false;
    }

    /// <summary>
    /// Creates a new button and its visual
    /// </summary>
    /// <param name="id">The button's id</param>
    /// <param name="sprite">The button's sprite</param>
    private void CreateButton(string id, Sprite sprite)
    {
        currentButtons.Add(new ANF.Persistent.MapButton()
        {
            id = id,
            sprite = sprite
        });
    }

    void OnBackgroundDrag(DragUpdatedEvent _)
    {
        Object droppedObject = DragAndDrop.objectReferences[0];

        if (droppedObject && droppedObject is Sprite)
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        else
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
    }

    void OnBackgroundDrop(DragPerformEvent _)
    {
        Object droppedObject = DragAndDrop.objectReferences[0];

        if (droppedObject && droppedObject is Sprite)
        {
            backgroundImage.sprite = (Sprite)droppedObject;
        }
    }
}
