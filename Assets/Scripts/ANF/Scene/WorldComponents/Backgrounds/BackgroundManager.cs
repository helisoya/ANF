using Leguar.TotalJSON;
using UnityEditor;
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
        [SerializeField] private bool asyncLoading = true;
        [SerializeField] private string prefabPath = "Backgrounds/";
        private Background currentBackground;
        private string currentBackgroundID;

        private AsyncOperation currentOperation;
        private string cachedNextBackgroundID;

        public bool loadingBackground { get; private set; }
        public bool unloadingBackground { get; private set; }

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
		/// <param name="force">True if the change should be forced even if </param>
        public void SetBackground(string ID, bool force = false)
        {
            if (force || ID != currentBackgroundID)
            {
                cachedNextBackgroundID = ID;

                currentOperation = RemoveCurrentBackground();

                unloadingBackground = currentOperation != null;

                if(!unloadingBackground || (currentOperation != null && currentOperation.isDone))
                {
                    EndBackgroundUnloading();
                }
            }
        }

        /// <summary>
        /// Callback for when the background's unloading has stoped
        /// </summary>
        /// <param name="forceSync">True if async operations should be forbiden</param>
        private void EndBackgroundUnloading(bool forceSync = false)
        {
            unloadingBackground = false;
            currentBackground = null;
            currentOperation = null;

            if (!string.IsNullOrEmpty(cachedNextBackgroundID))
            {
                currentOperation = LoadBackground(cachedNextBackgroundID);

                loadingBackground = asyncLoading && !forceSync;

                if (!loadingBackground || (currentOperation != null && currentOperation.isDone))
                    EndBackgroundLoading();
            }
        }

        /// <summary>
        /// Ends the background loading
        /// </summary>
        private void EndBackgroundLoading()
        {
            loadingBackground = false;
            currentBackgroundID = cachedNextBackgroundID;
            cachedNextBackgroundID = null;
            currentBackground = null;

            // Scene is already loaded
            // Prefab needs to be instanced
            if (backgroundType == BackgroundType.Prefab)
            {
                Background background = null;
                if (currentOperation is DummyResourceRequest)
                    background = ((DummyResourceRequest)currentOperation).GetObject() as Background;
                else if (currentOperation is ResourceRequest)
                    background = ((ResourceRequest)currentOperation).asset as Background;

                if(background != null)
                {
                    currentBackground = Object.Instantiate(background, manager.transform);
                }
            }
            else
            {
                UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(currentBackgroundID);
                if(scene != null && scene.GetRootGameObjects().Length == 1)
                {
                    currentBackground = scene.GetRootGameObjects()[0].GetComponent<Background>();
                }
            }

            if (currentBackground)
                currentBackground.OnLoad();

            currentOperation = null;
        }

        /// <summary>
        /// Loads a background
        /// </summary>
        /// <param name="ID">The background's ID</param>
        /// <param name="forceSync">True if async operations should be forbiden</param>
        /// <returns>An operation </returns>
        private AsyncOperation LoadBackground(string ID, bool forceSync = false)
        {
            AsyncOperation operation = null;

            if (backgroundType == BackgroundType.Scene)
            {
                if (forceSync || !asyncLoading)
                    SceneManager.LoadScene(ID, LoadSceneMode.Additive);
                else
                    operation = SceneManager.LoadSceneAsync(ID, LoadSceneMode.Additive);
            }
            else if (backgroundType == BackgroundType.Prefab)
            {
                if (forceSync || !asyncLoading)
                    operation = Resources.LoadAsync<Background>(prefabPath + ID);
                else
                    operation = new DummyResourceRequest(Resources.Load<Background>(prefabPath + ID));
            }

            return operation;
        }

        /// <summary>
        /// Removes the current Background
        /// </summary>
        private AsyncOperation RemoveCurrentBackground()
        {
            if (currentBackgroundID == null)
                return null;

            AsyncOperation operation = null;

            currentBackground.OnUnLoad();

            if (backgroundType == BackgroundType.Scene)
            {
                operation = SceneManager.UnloadSceneAsync(currentBackgroundID);
            }
            else if(backgroundType == BackgroundType.Prefab)
            {
                Object.Destroy(currentBackground.gameObject);
            }

            return operation;
        }

        public override void OnInitialize()
        {

        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {
            if (currentOperation != null && currentOperation.isDone)
            {
                if (unloadingBackground)
                    EndBackgroundUnloading();
                else if (loadingBackground)
                    EndBackgroundLoading();
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnSave(JSON json)
        {
            string current = null;

            if (currentBackgroundID != null)
                current = currentBackgroundID;
            else if (cachedNextBackgroundID != null)
                current = cachedNextBackgroundID;

            if (current != null)
            {
                json.Add("backgroundID", current);
            }
        }

        public override void OnLoad(JSON json)
        {
            cachedNextBackgroundID = null;
            currentBackgroundID = null;
      
            if (json.ContainsKey("backgroundID"))
            {
                cachedNextBackgroundID = json.GetString("backgroundID");
                EndBackgroundUnloading(true);
            }
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
            if(currentBackground != null && backgroundType == BackgroundType.Scene)
            {
                // Not optimal
                SceneManager.UnloadSceneAsync(currentBackgroundID);
            }
        }

        /// <summary>
        /// Represents a dummy ressource request used when loading resources
        /// </summary>
        private class DummyResourceRequest : AsyncOperation
        {
            private Object obj;

            public DummyResourceRequest(Object obj)
            {
                this.obj = obj;
            }

            public Object GetObject()
            {
                return obj;
            }
        }
    }
}

