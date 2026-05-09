using ANF.Persistent;
using Leguar.TotalJSON;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the interaction mode
    /// </summary>
    [System.Serializable]
    public class InteractionMode : WorldComponent
    {
        [Header("Infos")]
        [SerializeField] private LayerMask interactablesMask;

        [Header("Highlight")]
        [Tooltip("Full highlight means that every interactable object will glow. Otherwise, only the currently selected object will glow.")]
        [SerializeField] private bool useFullHighlight;
        [ColorUsage(true, true)][SerializeField] private Color baseColor;
        [ColorUsage(true, true)][SerializeField] private Color selectedColor;

        private Dictionary<string, InteractableObject> registeredObjects = new Dictionary<string, InteractableObject>();
        private List<InteractableObject> currentInteractionObjects = new List<InteractableObject>();
        private int currentIndex;
        private bool reloadInteractionMode = false;
        private int currentButtonInputSide = 0;
        private float cooldownToNextButtonIncrement = 0;

        private Vector2 mousePosition;
        private bool canTryMouseClick;

        private JSON loadedDataCache = null;

        public bool inInteractionMode { get; private set; } = false;
        public string selectedScript { get; private set; } = null;


        public override WorldComponent CloneComponent()
        {
            return new InteractionMode()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                useFullHighlight = useFullHighlight,
                baseColor = baseColor,
                selectedColor = selectedColor,
                interactablesMask = interactablesMask
            };
        }

        /// <summary>
        /// Registers a new interactable object
        /// </summary>
        /// <param name="obj">The new interactableObject</param>
        public void Register(InteractableObject obj)
        {
            if (obj == null)
                return;

            string id = obj.GetID();
            if (!registeredObjects.ContainsKey(id))
            {
                RestoreFromCache(obj);
                registeredObjects.Add(id, obj);
            }
            else
            {
                Debug.LogError($"Trying to add duplicate interactable object : {id}");
            }
        }

        /// <summary>
        /// Unregisters an interactable object
        /// </summary>
        /// <param name="obj">The interactable object</param>
        public void UnRegister(InteractableObject obj)
        {
            if (obj == null)
                return;

            string id = obj.GetID();
            if (registeredObjects.ContainsKey(obj.GetID()))
                registeredObjects.Remove(obj.GetID());
            else
                Debug.LogError($"Trying to remove an non registered interactable object : {id}");
        }

        /// <summary>
		/// Changes the next script for a specific interactable object
		/// </summary>
		/// <param name="id">The object's Id</param>
		/// <param name="script">The next script</param>
        public void SetInteractableObjectNextScript(string id, string script)
        {
            if (registeredObjects.TryGetValue(id, out InteractableObject obj))
                obj.SetNextScript(script);
        }

        /// <summary>
        /// Changes if a specific interactable object is hidden or not
        /// </summary>
        /// <param name="id">The object's Id</param>
        /// <param name="hidden">True if the object is hidden</param>
        public void SetInteractableObjectHidden(string id, bool hidden)
        {
            if (registeredObjects.TryGetValue(id, out InteractableObject obj))
                obj.SetHidden(hidden);
        }

        /// <summary>
		/// Generates a sorted list of non hidden interactable objects
		/// </summary>
        private void GenerateInteractionList()
        {
            currentInteractionObjects.Clear();
            foreach (InteractableObject obj in registeredObjects.Values)
            {
                if (!obj.GetIsHidden())
                {
                    obj.ComputeAppromixateVisualPoisition();
                    currentInteractionObjects.Add(obj);
                }
            }

            currentInteractionObjects.Sort((InteractableObject o1, InteractableObject o2) =>
            {
                return o1.GetApproximateVisualPosition().x.CompareTo(o2.GetApproximateVisualPosition().x);
            });
        }

        /// <summary>
		/// Starts the interaction mode
		/// </summary>
        public void StartInteractionMode()
        {
            RestoreFromCache();
            loadedDataCache = null;

            canTryMouseClick = false;
            mousePosition = new Vector2(-1, -1);

            GenerateInteractionList();

            if (currentInteractionObjects.Count > 0)
            {
                inInteractionMode = true;
                currentIndex = 0;

                if (useFullHighlight)
                {
                    foreach (InteractableObject obj in currentInteractionObjects)
                    {
                        obj.SetHighlightAlpha(1);
                        obj.SetHighlightColor(baseColor);
                    }
                }

                currentInteractionObjects[currentIndex].SetHighlightAlpha(1);
                currentInteractionObjects[currentIndex].SetHighlightColor(selectedColor);

                OnRegisterInputs();
            }
            else
            {
                inInteractionMode = false;
            }
        }

        /// <summary>
		/// Confirms the object and ends the interaction mode
		/// </summary>
		/// <param name="index">The object's index</param>
        public void ConfirmObject(int index)
        {
            OnUnRegisterInputs();

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            inInteractionMode = false;
            selectedScript = currentInteractionObjects[index].GetNextScript();

            foreach (InteractableObject obj in currentInteractionObjects)
            {
                obj.SetHighlightAlpha(0);
            }

            currentInteractionObjects.Clear();
        }

        /// <summary>
		/// Selects an interactable object
		/// </summary>
		/// <param name="index">The object's index</param>
        /// <param name="force">True if the change should be forced</param>
        public void SelectObject(int index, bool force = false)
        {
            if (force || index != currentIndex)
            {
                if (!useFullHighlight)
                    currentInteractionObjects[currentIndex].SetHighlightAlpha(0);

                currentInteractionObjects[currentIndex].SetHighlightColor(baseColor);

                currentIndex = index;

                if (!useFullHighlight)
                    currentInteractionObjects[currentIndex].SetHighlightAlpha(1);

                currentInteractionObjects[currentIndex].SetHighlightColor(selectedColor);
            }
        }

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {

        }
        public override void OnUpdate()
        {
            if (inInteractionMode)
            {
                if (reloadInteractionMode)
                {
                    reloadInteractionMode = false;
                    StartInteractionMode();
                    return;
                }


                if (currentButtonInputSide != 0)
                {
                    cooldownToNextButtonIncrement -= Time.deltaTime;
                    if (cooldownToNextButtonIncrement <= 0)
                    {
                        IncrementObjectWithInput();
                        cooldownToNextButtonIncrement = 0.5f;
                    }
                }


                RaycastHit hit;
                InteractableObject current = null;

                if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out hit, 500, interactablesMask))
                {
                    current = hit.transform.GetComponent<InteractableObject>();
                    if (current.GetIsHidden())
                        current = null;
                }


                Cursor.SetCursor(current == null ? null : current.GetIcon(), Vector2.zero, CursorMode.Auto);

                if (current != null)
                {
                    for (int i = 0; i < currentInteractionObjects.Count; i++)
                    {
                        if (currentInteractionObjects[i] == current)
                        {
                            SelectObject(i);

                            if (canTryMouseClick)
                                ConfirmObject(i);
                            break;
                        }
                    }
                }


                canTryMouseClick = false;
            }
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode && context.ReadValueAsButton())
            {
                ConfirmObject(currentIndex);
            }
        }

        private void OnMousePosition(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode)
                mousePosition = context.ReadValue<Vector2>();
        }

        private void OnMouseClick(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode && context.ReadValueAsButton())
                canTryMouseClick = true;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide = value.x < 0 ? 1 : -1;

                        IncrementObjectWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide = 0;
                }
            }
        }

        /// <summary>
        /// Increments the current object with the keyboard input
        /// </summary>
        private void IncrementObjectWithInput()
        {
            SelectObject((currentIndex + currentButtonInputSide + currentInteractionObjects.Count) % currentInteractionObjects.Count);
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled += OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("MousePosition").performed += OnMousePosition;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("MouseClick").performed += OnMouseClick;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("Move").canceled -= OnMove;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("MousePosition").performed -= OnMousePosition;
            PersistentDataManager.instance.GetPlayerInput().actions.FindAction("MouseClick").performed -= OnMouseClick;
        }

        public override void OnChangeScene()
        {
            OnUnRegisterInputs();
        }

        /// <summary>
        /// Try to restore all interactable objects from the loaded data cache
        /// </summary>
        private void RestoreFromCache()
        {
            foreach (InteractableObject obj in registeredObjects.Values)
            {
                RestoreFromCache(obj);

                if (loadedDataCache == null)
                    return;
            }
        }

        /// <summary>
        /// Try to restore an interactable object from the loaded data cache
        /// </summary>
        /// <param name="obj">The object</param>
        private void RestoreFromCache(InteractableObject obj)
        {
            if (loadedDataCache == null)
                return;

            if (loadedDataCache.ContainsKey(obj.GetID()))
            {
                JSON objJSON = loadedDataCache.GetJSON(obj.GetID());
                obj.SetHidden(objJSON.GetBool("hidden"));

                if (objJSON.ContainsKey("script"))
                    obj.SetNextScript(objJSON.GetString("script"));

                loadedDataCache.Remove(obj.GetID());

                if (loadedDataCache.Count == 0)
                    loadedDataCache = null;
            }
        }

        public override void OnSave(JSON json)
        {
            JSON registeredObjectsJSON = new JSON();
            foreach (InteractableObject obj in registeredObjects.Values)
            {
                JSON objectJSON = new JSON();
                if (!string.IsNullOrEmpty(obj.GetNextScript()))
                    objectJSON.Add("script", obj.GetNextScript());
                objectJSON.Add("hidden", obj.GetIsHidden());
                registeredObjectsJSON.Add(obj.GetID(), objectJSON);
            }
            json.Add("registeredObjects", registeredObjectsJSON);


            if (inInteractionMode)
            {
                json.Add("inInteractionMode", inInteractionMode);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("registeredObjects"))
            {
                loadedDataCache = new JSON(json.GetJSON("registeredObjects").AsDictionary());
            }

            if (json.ContainsKey("inInteractionMode"))
            {
                inInteractionMode = true;
                reloadInteractionMode = true;
            }
        }
    }
}
