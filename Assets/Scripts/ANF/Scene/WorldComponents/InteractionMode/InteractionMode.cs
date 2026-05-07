using ANF.Persistent;
using Leguar.TotalJSON;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the interaction mode
    /// </summary>
    [System.Serializable]
    public class InteractionMode : WorldComponent
    {
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

        public bool inInteractionMode { get; private set; } = false;
        public string selectedScript { get; private set; } = null;


        public override WorldComponent CloneComponent()
        {
            return new InteractionMode()
            {
                useFullHighlight = useFullHighlight,
                baseColor = baseColor,
                selectedColor = selectedColor
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
                registeredObjects.Add(id, obj);
            else
                Debug.LogError($"Trying to add duplicate interactable object : {id}");
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
                    StartInteractionMode();
                    return;
                }


                if (currentButtonInputSide != 0)
                {
                    cooldownToNextButtonIncrement -= Time.deltaTime;
                    if (cooldownToNextButtonIncrement <= 0)
                    {
                        IncrementButtonWithInput();
                        cooldownToNextButtonIncrement = 0.5f;
                    }
                }
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
        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
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

                        IncrementButtonWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide = 0;
                }
            }
        }

        private void IncrementButtonWithInput()
        {
            SelectObject((currentIndex + currentButtonInputSide + currentInteractionObjects.Count) % currentInteractionObjects.Count);
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
            OnUnRegisterInputs();
        }

        public override void OnSave(JSON json)
        {
            if (inInteractionMode)
            {
                json.Add("inInteractionMode", inInteractionMode);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("inInteractionMode"))
            {
                inInteractionMode = true;
                reloadInteractionMode = true;
            }
        }
    }
}
