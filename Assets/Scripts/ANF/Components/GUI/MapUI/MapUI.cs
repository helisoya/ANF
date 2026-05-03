using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Leguar.TotalJSON;
using ANF.Persistent;
using DG.Tweening;
using UnityEngine.InputSystem;


namespace ANF.GUI
{
    /// <summary>
    /// Handles ANF's map system
    /// </summary>
    public class MapUI : GUIComponent
    {
        [Header("Map")]
        [SerializeField] private RectTransform mapRoot;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private MapUIButton prefabButton;
        private List<MapUIButton> buttons;
        private MapDefs currentMapDefs;
        private ANF.Persistent.MapData currentMap;
        private int currentButtonIndex;
        private Vector2Int currentButtonInputSide = new Vector2Int();
        private float cooldownToNextButtonIncrement = 0;

        public bool showingMap { get; private set; } = false;
        public string selectedScript { get; private set; } = null;

        /// <summary>
        /// Opens the map
        /// </summary>
        /// <param name="enabled">Enabl</param>
        /// <param name="map">The map to open</param>
        /// <param name="currentButton">The player's current position/button</param>
        public void SetEnabled(bool enabled, ANF.Persistent.MapData mapData, ANF.Persistent.MapDefs mapDefs)
        {
            // ! Current point
            if (!isEnabled && enabled)
            {
                showingMap = true;
                selectedScript = null;
                currentMap = mapData;
                currentMapDefs = mapDefs;
            }

            SetEnabled(enabled);
        }

        public override void OnInitialize()
        {
            mapRoot.localScale = new Vector3(0, 1, 1);
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide.x != 0 || currentButtonInputSide.y != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.5f;
                }
            }
        }

        /// <summary>
		/// Computes the unstretched background image's size
		/// </summary>
		/// <returns></returns>
        private Vector2 CalculateImageSize()
        {
            Vector2 size = new Vector2();
            if (backgroundImg.rectTransform.sizeDelta.x > backgroundImg.rectTransform.sizeDelta.y)
            {
                float mul = backgroundImg.rectTransform.sizeDelta.y / backgroundImg.sprite.rect.height;
                size.x = backgroundImg.sprite.rect.width * mul;
                size.y = backgroundImg.rectTransform.sizeDelta.y;
            }
            else
            {
                float mul = backgroundImg.rectTransform.sizeDelta.x / backgroundImg.sprite.rect.width;
                size.x = backgroundImg.rectTransform.sizeDelta.x;
                size.y = backgroundImg.sprite.rect.height * mul;
            }
            return size;
        }

        public override void OnEnabled()
        {
            currentButtonIndex = 0;

            backgroundImg.sprite = currentMap.backgroundSprite;
            Vector2 imageSize = CalculateImageSize();
            Vector2 sizeDifference = backgroundImg.rectTransform.sizeDelta - imageSize;
            buttons = new List<MapUIButton>();

            foreach (Transform child in backgroundImg.transform)
                Destroy(child.gameObject);

            foreach (ANF.Persistent.MapButton button in currentMap.buttons)
            {
                bool canShow = false;
                string scriptFound = "";
                foreach (MapButtonDef def in currentMapDefs.buttons)
                {
                    if (def.linkedButton.Equals(button.id))
                    {
                        int variableValue = 0;
                        if ((def.type == MapDefsType.Variable || def.type == MapDefsType.VariableToggle) &&
                            PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer container))
                            container.GetVariable(def.linkedVariable, out variableValue);

                        if (def.CheckIfVisible(variableValue))
                        {
                            scriptFound = def.linkedScript;
                            canShow = true;
                        }

                        break;
                    }
                }

                if (canShow)
                {
                    MapUIButton instance = Instantiate(prefabButton, backgroundImg.rectTransform);
                    RectTransform instanceTransform = instance.GetComponent<RectTransform>();
                    instanceTransform.eulerAngles = new Vector3(0, 0, button.rotation);
                    instanceTransform.anchoredPosition = new Vector2(
                        sizeDifference.x / 2.0f + imageSize.x * button.normalizedPosition.x,
                        -sizeDifference.y / 2.0f - imageSize.y * button.normalizedPosition.y
                    );
                    instance.Initialize(buttons.Count, currentMap.id + "_" + button.id, scriptFound, button.sprite, this);
                    buttons.Add(instance);
                }
            }

            buttons[currentButtonIndex].OnEnter();
            mapRoot.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnDisabled()
        {
            mapRoot.DOScaleX(0, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                showingMap = false;
            });
        }

        /// <summary>
		/// Selects a map button and applies its effect
		/// </summary>
		/// <param name="id">The button's index</param>
        public void SelectButton(int id)
        {
            selectedScript = buttons[id].GetLinkedScript();
            SetEnabled(false);
        }

        /// <summary>
		/// Changes the current button
		/// </summary>
		/// <param name="id">The new button's id</param>
        /// <param name="force">True if the id check should be skipped</param>
        public void SetCurrentButton(int id, bool force = false)
        {
            if (id < 0)
                return;

            if (force || currentButtonIndex != id)
            {
                buttons[currentButtonIndex].OnExit();
                currentButtonIndex = id;
                buttons[currentButtonIndex].OnEnter();
            }
        }

        /// <summary>
        /// Increments the current button with the keyboard input
        /// </summary>
        private void IncrementButtonWithInput()
        {
            Vector2 position = buttons[currentButtonIndex].GetComponent<RectTransform>().anchoredPosition;
            Vector2Int closestIndex = new Vector2Int(currentButtonIndex, currentButtonIndex);
            Vector2 closestPosition = new Vector2(-9999 * currentButtonInputSide.x, -9999 * currentButtonInputSide.y);

            Vector2 tmpPos;
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == currentButtonIndex)
                    continue;

                tmpPos = buttons[i].GetComponent<RectTransform>().anchoredPosition;

                if ((tmpPos.x <= position.x && currentButtonInputSide.x == 1 && tmpPos.x > closestPosition.x) ||
                    (tmpPos.x >= position.x && currentButtonInputSide.x == -1 && tmpPos.x < closestPosition.x))
                {
                    closestPosition.x = tmpPos.x;
                    closestIndex.x = i;
                }

                if ((tmpPos.y <= position.y && currentButtonInputSide.y == 1 && tmpPos.y > closestPosition.y) ||
                    (tmpPos.y >= position.y && currentButtonInputSide.y == -1 && tmpPos.y < closestPosition.y))
                {
                    closestPosition.y = tmpPos.y;
                    closestIndex.y = i;
                }
            }

            if (currentButtonInputSide.x != 0 && currentButtonInputSide.y != 0)
            {
                if (Mathf.Abs(position.x - closestPosition.x) < Mathf.Abs(position.y - closestPosition.y))
                    SetCurrentButton(closestIndex.x);
                else
                    SetCurrentButton(closestIndex.y);
            }
            else if (currentButtonInputSide.x != 0)
            {
                SetCurrentButton(closestIndex.x);
            }
            else if (currentButtonInputSide.y != 0)
            {
                SetCurrentButton(closestIndex.y);
            }

        }


        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                SelectButton(currentButtonIndex);
            }
        }
        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.x = value.x < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (Mathf.Abs(value.y) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.y = value.y < 0 ? 1 : -1;
                        IncrementButtonWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide.x = 0;
                    currentButtonInputSide.y = 0;
                }
            }
        }

        public override void OnPaused()
        {
            cooldownToNextButtonIncrement = 0.0f;
            currentButtonInputSide = Vector2Int.zero;
            mapRoot.DOScaleX(0, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            mapRoot.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnSave(JSON json)
        {
            json.Add("showingMap", showingMap);
            json.Add("selectedScript", selectedScript);

            if (showingMap)
            {
                json.Add("currentMap", currentMap.id);
                json.Add("currentMapDefs", currentMapDefs.id);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingMap"))
                showingMap = json.GetBool("showingMap");

            if (json.ContainsKey("selectedScript"))
                selectedScript = json.GetString("selectedScript");

            if (showingMap && json.ContainsKey("currentMap") && json.ContainsKey("currentMapDefs") &&
            PersistentDataManager.instance.GetGlobalData().GetComponent<MapContainer>(out MapContainer container))
            {
                if (container.GetMap(json.GetString("currentMap"), out ANF.Persistent.MapData loadedMap) &&
                    container.GetDef(json.GetString("currentMapDefs"), out MapDefs loadedDefs))
                {
                    isEnabled = false;
                    SetEnabled(true, loadedMap, loadedDefs);

                    if (json.ContainsKey("isEnabled"))
                        json.Remove("isEnabled");
                }
            }
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled += OnMove;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled -= OnMove;
        }
        public override void OnChangeScene()
        {
        }
    }
}

