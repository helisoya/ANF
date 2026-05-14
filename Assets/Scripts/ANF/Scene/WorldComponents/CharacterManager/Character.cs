using ANF.Utils;
using Leguar.TotalJSON;
using Unity.Collections;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
	/// Represents a character in the scene
	/// </summary>
    public class Character : SceneObject
    {
        [Header("Character")]
        [SerializeField] private string characterName;
        [SerializeField] private Animator animator;
        [SerializeField] private float talkingTransitionDuration = 0.1f;
        private float currentTalkingValue = 0.0f;
        private LerpInstanceFloat talkingLerp;
        private string currentBody = null;
        private string currentEye = null;
        private string currentMouth = null;

        public void EditorInit(string characterName, Animator animator, Renderer[] renderers, InteractableObject interactableObject)
        {
            this.characterName = characterName;
            this.animator = animator;
            this.renderers = renderers;
            this.linkedInteraction = interactableObject;
        }

        /// <summary>
        /// Changes if the character is talking or not
        /// </summary>
        /// <param name="isTalking">True if talking</param>
        public void SetIsTalking(bool isTalking)
        {
            talkingLerp.StartLerp(currentTalkingValue, isTalking ? 1.0f : 0.0f, talkingTransitionDuration);
        }

        /// <summary>
        /// Changes the current body animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeBodyAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            currentBody = stateName;

            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 0);
            else
                animator.Play(stateName, 0);
        }

        /// <summary>
        /// Changes the current eye animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeEyeAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            currentEye = stateName;

            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 1);
            else
                animator.Play(stateName, 1);
        }

        /// <summary>
        /// Changes the current mouth animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeMouthAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            currentMouth = stateName;

            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 2);
            else
                animator.Play(stateName, 2);
        }

        /// <summary>
        /// Gets the character's animator
        /// </summary>
        /// <returns>The character's animator</returns>
        public Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Gets the character's name
        /// </summary>
        /// <returns>The character's name</returns>
        public string GetCharacterName()
        {
            return characterName;
        }

        protected override void OnCreate(ANFManager manager)
        {
            talkingLerp = new LerpInstanceFloat();

        }

        protected override void OnRemove(ANFManager manager)
        {
        }

        protected override void OnUpdate(ANFManager manager)
        {
            if (talkingLerp.lerping)
            {
                currentTalkingValue = talkingLerp.Update();
                animator.SetFloat("Talking", currentTalkingValue);
            }
        }


        protected override void OnSave(JSON json)
        {
            json.Add("currentTalkingValue", currentTalkingValue);

            if (talkingLerp != null)
            {
                JSON jsonLerp = new JSON();
                talkingLerp.Save(jsonLerp);
                json.Add("talkingLerp", jsonLerp);
            }

            if (currentBody != null)
                json.Add("currentBody", currentBody);
            if (currentMouth != null)
                json.Add("currentMouth", currentMouth);
            if (currentEye != null)
                json.Add("currentEye", currentEye);

        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("currentTalkingValue"))
            {
                currentTalkingValue = json.GetFloat("currentTalkingValue");
                animator.SetFloat("Talking", currentTalkingValue);
            }

            if (json.ContainsKey("talkingLerp"))
            {
                if (talkingLerp == null)
                    talkingLerp = new LerpInstanceFloat();

                talkingLerp.Load(json.GetJSON("talkingLerp"));
            }

            if (json.ContainsKey("currentBody"))
                ChangeBodyAnimation(json.GetString("currentBody"), true);
            if (json.ContainsKey("currentMouth"))
                ChangeMouthAnimation(json.GetString("currentMouth"), true);
            if (json.ContainsKey("currentEye"))
                ChangeEyeAnimation(json.GetString("currentEye"), true);
        }
    }
}

