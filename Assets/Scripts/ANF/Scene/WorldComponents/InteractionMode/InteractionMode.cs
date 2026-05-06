using Leguar.TotalJSON;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the interaction mode
    /// </summary>
    [System.Serializable]
    public class InteractionMode : WorldComponent
    {
        private Dictionary<string, InteractableObject> registeredObjects = new Dictionary<string, InteractableObject>();


        public bool inInteractionMode { get; private set; } = false;
        public string selectedScript { get; private set; } = null;


        public override WorldComponent CloneComponent()
        {
            return new InteractionMode();
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

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {

        }
        public override void OnUpdate()
        {

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

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
            OnUnRegisterInputs();
        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }
    }
}
