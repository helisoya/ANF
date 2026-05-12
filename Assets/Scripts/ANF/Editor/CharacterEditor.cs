using System.Collections.Generic;
using System.Drawing.Printing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;

namespace ANF.Editor
{
    /// <summary>
    /// Represents the editor of the Character class
    /// </summary>
    [CustomEditor(typeof(ANF.Scene.Character))]
    public class CharacterEditor : UnityEditor.Editor
    {
        private bool foldoutBody = true;
        private bool foldoutEye = true;
        private bool foldoutMouth = true;

        private bool foldoutAddBody = true;
        private AnimationClip bodyStateClip1 = null;
        private AnimationClip bodyStateClip2 = null;
        private string bodyStateName = "NewBodyState";

        private bool foldoutAddEye = true;
        private Texture2D eyeStateTexture = null;
        private AnimationClip eyeStateClip1 = null;
        private AnimationClip eyeStateClip2 = null;
        private string eyeStateName = "NewEyeState";

        private bool foldoutAddMouth = true;
        private Texture2D mouthStateTexture = null;
        private AnimationClip mouthStateClip1 = null;
        private AnimationClip mouthStateClip2 = null;
        private string mouthStateName = "NewMouthState";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(50);

            ANF.Scene.Character character = target.GetComponent<ANF.Scene.Character>();
            string pathToAnimations = "Assets/Resources/Animations/Characters/"+character.GetCharacterName()+"/";

            if (character.GetAnimator().runtimeAnimatorController &&
                character.GetAnimator().runtimeAnimatorController is AnimatorController)
            {
                AnimatorController animator = target.GetComponent<ANF.Scene.Character>().GetAnimator().runtimeAnimatorController as AnimatorController;
                DrawBodyAnimations(animator, pathToAnimations, 0);
                GUILayout.Space(15);
                DrawEyeAnimations(animator, pathToAnimations, 1);
                GUILayout.Space(15);
                DrawMouthAnimations(animator, pathToAnimations, 2);
            }
            else
            {
                if (GUILayout.Button("Create Controller"))
                {
                    CreateDefaultController(character, pathToAnimations);
                }
            }

            serializedObject.Update();
        }

        /// <summary>
        /// Creates a default animation controller for this character
        /// </summary>
        /// <param name="character">The character</param>
        private void CreateDefaultController(ANF.Scene.Character character, string animationFolder)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animationFolder+character.GetCharacterName()+".controller");

            controller.AddParameter("Talking", AnimatorControllerParameterType.Float);

            controller.AddLayer("Body");
            controller.AddLayer("Eye");
            controller.AddLayer("Mouth");
            controller.layers[0].blendingMode = AnimatorLayerBlendingMode.Additive;
            controller.layers[1].blendingMode = AnimatorLayerBlendingMode.Additive;
            controller.layers[2].blendingMode = AnimatorLayerBlendingMode.Additive;

            character.GetAnimator().runtimeAnimatorController = controller;

            AssetDatabase.SaveAssetIfDirty(controller);
        }

        /// <summary>
        /// Opens the animator window to a new clip
        /// </summary>
        /// <param name="clip">The clip to show</param>
        private void OpenAnimatorWindow(AnimationClip clip)
        {
            AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
            if(!window)
                window = EditorWindow.CreateWindow<AnimationWindow>();

            window.animationClip = clip;
        }

        /// <summary>
        /// Draws a state machine
        /// </summary>
        /// <param name="animator">The linked animator controller</param>
        /// <param name="stateMachine">The state machine</param>
        private void DrawStateMachine(AnimatorController animator, AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(state.state.name);

                Motion motion = state.state.motion;

                if (motion != null && motion is AnimationClip)
                {
                    if (GUILayout.Button("Show clip"))
                    {
                        OpenAnimatorWindow(motion as AnimationClip);
                    }
                }

                if (GUILayout.Button("Remove"))
                {
                    stateMachine.RemoveState(state.state);
                    AssetDatabase.SaveAssetIfDirty(animator);
                    GUILayout.EndHorizontal();
                    return;
                }

                GUILayout.EndHorizontal();

                if (motion is BlendTree)
                {
                    BlendTree blendTree = motion as BlendTree;

                    foreach (ChildMotion childMotion in blendTree.children)
                    {
                        if (childMotion.motion is AnimationClip)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("  ->");
                            if (GUILayout.Button(childMotion.motion.name))
                            {
                                OpenAnimatorWindow(childMotion.motion as AnimationClip);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates an empty state with an animation clip inside
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="clip">The linked clip (can be null)</param>
        private void CreateFromAnimationClip(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName, AnimationClip clip)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            if(clip == null)
            {
                clip = new AnimationClip();
                clip.name = newStateName;
                AssetDatabase.CreateAsset(clip, pathToAnimations + newStateName + ".anim");
            }

            AnimatorState createdState = stateMachine.AddState(newStateName);
            createdState.motion = clip;

            AssetDatabase.SaveAssetIfDirty(clip);
            AssetDatabase.SaveAssetIfDirty(animator);
        }

        /// <summary>
        /// Creates a new state with an empty blend tree
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="clipNormal">The normal clip (can be null)</param>
        /// <param name="clipTalking">The talking clip (can be null)</param>
        private void CreateFromBlendTree(AnimatorController animator, AnimatorStateMachine stateMachine, string newStateName, AnimationClip clipNormal, AnimationClip clipTalking)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            BlendTree blendTree = new BlendTree();
            blendTree.name = newStateName;
            blendTree.blendParameter = "Talking";
            blendTree.AddChild(clipNormal, 0);
            blendTree.AddChild(clipTalking, 1);

            AnimatorState createdState = stateMachine.AddState(newStateName);
            createdState.motion = blendTree;

            AssetDatabase.SaveAssetIfDirty(animator);
        }

        /// <summary>
        /// Creates a new body animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        private void CreateBodyFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Body/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Body/Normal_Speak.anim", pathToAnimations + newStateName + "_Speak.anim");

            CreateFromBlendTree(animator, stateMachine, newStateName,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim"),
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Speak.anim"));
        }

        /// <summary>
        /// Creates a new eye animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="eyeTexture">The eye texture</param>
        private void CreateEyeFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName, Texture2D eyeTexture)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Eye/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");

            
            // Change Sprites
            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyeTexture));
            foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
            Sprite defaultEyeSprite = dicSprites['0'];

            AnimationClip eyeNormalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim");
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(eyeNormalClip)[0];
            ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(eyeNormalClip, binding);
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
            }
            AnimationUtility.SetObjectReferenceCurve(eyeNormalClip, binding, frames);

            AssetDatabase.SaveAssetIfDirty(eyeNormalClip);


            CreateFromAnimationClip(animator, stateMachine, pathToAnimations, newStateName, eyeNormalClip);
        }

        /// <summary>
        /// Creates a new mouth animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="mouthTexture">The mouth texture</param>
        private void CreateMouthFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName, Texture2D mouthTexture)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Mouth/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Mouth/Normal_Speak.anim", pathToAnimations + newStateName + "_Speak.anim");

            // Change Sprites
            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthTexture));
            foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
            Sprite defaultMouthSprite = dicSprites['0'];

            string[] animNames = new string[] { newStateName + "_Idle", newStateName + "_Speak" };

            foreach (string animName in animNames)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + animName + ".anim");
                EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)[0];
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                AssetDatabase.SaveAssetIfDirty(clip);
            }

            CreateFromBlendTree(animator, stateMachine, newStateName,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim"),
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Speak.anim"));
        }

        /// <summary>
        /// Draws the body animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the body animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawBodyAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutBody = EditorGUILayout.Foldout(foldoutBody, "Body Animations");
            if (foldoutBody)
            {
                if(animator.layers.Length <= layer)
                {
                    GUILayout.Label("No body layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddBody = EditorGUILayout.Foldout(foldoutAddBody,"Add new state");
                if(foldoutAddBody)
                {
                    bodyStateName = EditorGUILayout.TextField("State name", bodyStateName);

                    bodyStateClip1 = EditorGUILayout.ObjectField("Clip1", bodyStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    bodyStateClip2 = EditorGUILayout.ObjectField("Clip2", bodyStateClip2, typeof(AnimationClip), false) as AnimationClip;

                    if (GUILayout.Button("Create Empty (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Body/", bodyStateName, bodyStateClip1);
                    }

                    if (GUILayout.Button("Create Empty (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, bodyStateName, bodyStateClip1, bodyStateClip2);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateBodyFromTemplate(animator,stateMachine, pathToAnimations + "Body/", bodyStateName);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the eye animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the eye animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawEyeAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutEye = EditorGUILayout.Foldout(foldoutEye, "Eye Animations");
            if (foldoutEye)
            {
                if (animator.layers.Length <= layer)
                {
                    GUILayout.Label("No eye layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddEye = EditorGUILayout.Foldout(foldoutAddEye, "Add new state");
                if (foldoutAddEye)
                {
                    eyeStateName = EditorGUILayout.TextField("State name", eyeStateName);

                    eyeStateClip1 = EditorGUILayout.ObjectField("Clip1", eyeStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    eyeStateClip2 = EditorGUILayout.ObjectField("Clip2", eyeStateClip2, typeof(AnimationClip), false) as AnimationClip;
                    eyeStateTexture = EditorGUILayout.ObjectField("Texture (for Template)", eyeStateTexture, typeof(Texture2D), false) as Texture2D;

                    if (GUILayout.Button("Create Empty (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Eye/", eyeStateName, eyeStateClip1);
                    }

                    if (GUILayout.Button("Create Empty (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, eyeStateName, eyeStateClip1, eyeStateClip2);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateEyeFromTemplate(animator, stateMachine, pathToAnimations + "Eye/", eyeStateName, eyeStateTexture);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the mouth animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the mouth animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawMouthAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutMouth = EditorGUILayout.Foldout(foldoutMouth, "Mouth Animations");
            if (foldoutMouth)
            {
                if (animator.layers.Length <= layer)
                {
                    GUILayout.Label("No mouth layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddMouth = EditorGUILayout.Foldout(foldoutAddMouth, "Add new state");
                if (foldoutAddMouth)
                {
                    mouthStateName = EditorGUILayout.TextField("State name", mouthStateName);

                    mouthStateClip1 = EditorGUILayout.ObjectField("Clip1", mouthStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    mouthStateClip2 = EditorGUILayout.ObjectField("Clip2", mouthStateClip2, typeof(AnimationClip), false) as AnimationClip;
                    mouthStateTexture = EditorGUILayout.ObjectField("Texture (for Template)", mouthStateTexture, typeof(Texture2D), false) as Texture2D;

                    if (GUILayout.Button("Create Empty (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Mouth/", mouthStateName, mouthStateClip1);
                    }

                    if (GUILayout.Button("Create Empty (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, mouthStateName, mouthStateClip1, mouthStateClip2);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateMouthFromTemplate(animator, stateMachine, pathToAnimations + "Mouth/", mouthStateName, mouthStateTexture);
                    }
                }
            }
        }
    }
}
