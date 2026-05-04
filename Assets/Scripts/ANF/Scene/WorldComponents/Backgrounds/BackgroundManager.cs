using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANF.Scene
{
    /// <summary>
	/// Represents how a background is handled
	/// </summary>
    public enum BackgroundType
    {
        Prefab,
        Scene
    }

    /// <summary>
	/// Handles the game's backgrounds.
    /// Backgrounds can be 3D scenes, or 3D prefabs
	/// </summary>
    [System.Serializable]
    public class BackgroundManager : WorldComponent
    {
        [SerializeField] private BackgroundType backgroundType;
        [SerializeField] private string prefabPath = "Backgrounds/";
        private Background currentBackground;
        private string currentBackgroundID;

        public override WorldComponent CloneComponent()
        {
            return new BackgroundManager()
            {
                backgroundType = backgroundType,
                prefabPath = prefabPath
            };
        }

        /// <summary>
		/// Loads a background and removes the previous background if needed
		/// </summary>
		/// <param name="ID">The new background's ID. In Scene Mode, this is the Scene's name. 
        /// In Prefab Mode, this is the prefab's path in Resources/[GeneralPrefabPath]/...</param>
		/// <param name="force"></param>
        public void LoadBackground(string ID, bool force = false)
        {
            if (!string.IsNullOrEmpty(ID) && (force || !ID.Equals(currentBackgroundID)))
            {

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

        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnSave(JSON json)
        {
            if (currentBackground != null)
            {
                json.Add("currentBackgroundID", currentBackgroundID);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("currentBackgroundID"))
                LoadBackground(json.GetString("currentBackgroundID"), true);
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

        }


    }
}

