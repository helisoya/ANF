using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    string bodyAnim = "NewAnim";

    string eyeAnim = "NewAnim";
    Texture2D eyeTexture = null;

    string mouthAnim = "NewAnim";
    Texture2D mouthTexture = null;


    bool foldoutAdd = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        string pathToAnimations = "Assets/Resources/Animations/Characters/" + target.GetComponent<Character>().characterName;
        GetAnimatorControllers(pathToAnimations, "Body");
        GetAnimatorControllers(pathToAnimations, "Eye");
        GetAnimatorControllers(pathToAnimations, "Mouth");

        GUILayout.Space(30);
        foldoutAdd = EditorGUILayout.Foldout(foldoutAdd, "Add new animations");
        if (foldoutAdd)
        {
            CreateAdditionalBodyAnim(pathToAnimations);
            GUILayout.Space(25);
            CreateAdditionalEyeAnim(pathToAnimations);
            GUILayout.Space(25);
            CreateAdditionalMouthAnim(pathToAnimations);   
        }


    }

    private void CreateAdditionalBodyAnim(string pathToAnimation)
    {
        pathToAnimation = pathToAnimation + "/Body/";

        GUILayout.Label("Create a new body animation");
        bodyAnim = GUILayout.TextField(bodyAnim);
        if (GUILayout.Button("Create") && !AnimatorExists(pathToAnimation, bodyAnim))
        {
            Debug.Log("Yes");
            // Generate Animations
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Body/Normal.overrideController", pathToAnimation + bodyAnim + ".overrideController");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Body/Normal_Enter.anim", pathToAnimation + bodyAnim + "_Enter.anim");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Body/Normal_Exit.anim", pathToAnimation + bodyAnim + "_Exit.anim");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Body/Normal_Idle.anim", pathToAnimation + bodyAnim + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Body/Normal_Speak.anim", pathToAnimation + bodyAnim + "_Speak.anim");

            // Fix Body Animation
            AnimatorOverrideController bodyOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimation + bodyAnim + ".overrideController");
            List<KeyValuePair<AnimationClip, AnimationClip>> clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            bodyOverride.GetOverrides(clipOverrides);
            string back;
            for (int i = 0; i < clipOverrides.Count; i++)
            {
                back = clipOverrides[i].Key.name.Split('_')[1];
                Debug.Log(pathToAnimation + clipOverrides[i].Key.name + ".anim" + " -> " + AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + bodyAnim + "_" + back + ".anim"));

                clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + bodyAnim + "_" + back + ".anim"));
            }
            bodyOverride.ApplyOverrides(clipOverrides);

            AssetDatabase.SaveAssets();
        }
    }

    private void CreateAdditionalEyeAnim(string pathToAnimation)
    {
        pathToAnimation = pathToAnimation + "/Eye/";

        GUILayout.Label("Create a new eye animation");
        eyeAnim = GUILayout.TextField(eyeAnim);
        eyeTexture = EditorGUILayout.ObjectField(eyeTexture,typeof(Texture2D),false) as Texture2D;
        if (GUILayout.Button("Create") && !AnimatorExists(pathToAnimation, eyeAnim))
        {
            Debug.Log("Yes");
            // Generate Animations
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Eye/Normal.overrideController", pathToAnimation + eyeAnim + ".overrideController");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Eye/Normal_Idle.anim", pathToAnimation + eyeAnim + "_Idle.anim");

            // Fix Body Animation
            AnimatorOverrideController bodyOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimation + eyeAnim + ".overrideController");
            List<KeyValuePair<AnimationClip, AnimationClip>> clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            bodyOverride.GetOverrides(clipOverrides);
            string back;
            for (int i = 0; i < clipOverrides.Count; i++)
            {
                back = clipOverrides[i].Key.name.Split('_')[1];
                Debug.Log(pathToAnimation + clipOverrides[i].Key.name + ".anim" + " -> " + AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + eyeAnim + "_" + back + ".anim"));

                clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + eyeAnim + "_" + back + ".anim"));
            }
            bodyOverride.ApplyOverrides(clipOverrides);

            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyeTexture));
            foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
            Sprite defaultEyeSprite = dicSprites['0'];

            AnimationClip eyeNormalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + eyeAnim +"_Idle.anim");
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(eyeNormalClip)[0];
            ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(eyeNormalClip, binding);
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
            }
            AnimationUtility.SetObjectReferenceCurve(eyeNormalClip, binding, frames);

            AssetDatabase.SaveAssets();
        }
    }

    private void CreateAdditionalMouthAnim(string pathToAnimation)
    {
        pathToAnimation = pathToAnimation + "/Mouth/";

        GUILayout.Label("Create a new mouth animation");
        mouthAnim = GUILayout.TextField(mouthAnim);
        mouthTexture = EditorGUILayout.ObjectField(mouthTexture,typeof(Texture2D),false) as Texture2D;
        if (GUILayout.Button("Create") && !AnimatorExists(pathToAnimation, mouthAnim))
        {
            Debug.Log("Yes");
            // Generate Animations
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Mouth/Normal.overrideController", pathToAnimation + mouthAnim + ".overrideController");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Mouth/Normal_Idle.anim", pathToAnimation + mouthAnim + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE/Mouth/Normal_Speak.anim", pathToAnimation + mouthAnim + "_Speak.anim");

            // Fix Body Animation
            AnimatorOverrideController bodyOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimation + mouthAnim + ".overrideController");
            List<KeyValuePair<AnimationClip, AnimationClip>> clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            bodyOverride.GetOverrides(clipOverrides);
            string back;
            for (int i = 0; i < clipOverrides.Count; i++)
            {
                back = clipOverrides[i].Key.name.Split('_')[1];
                Debug.Log(pathToAnimation + clipOverrides[i].Key.name + ".anim" + " -> " + AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + mouthAnim + "_" + back + ".anim"));

                clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + mouthAnim + "_" + back + ".anim"));
            }
            bodyOverride.ApplyOverrides(clipOverrides);

            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthTexture));
            foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
            Sprite defaultMouthSprite = dicSprites['0'];

            string[] animNames = new string[] { mouthAnim+"_Idle", mouthAnim+"_Speak" };

            foreach (string animName in animNames)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimation + animName + ".anim");
                EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)[0];
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
            }

            AssetDatabase.SaveAssets();
        }
    }


    private bool AnimatorExists(string path, string name)
    {
        return File.Exists(path + name + ".overrideController");
    }

    private void GetAnimatorControllers(string path, string name)
    {
        path = path + "/" + name + "/";

        GUILayout.Space(5);
        GUILayout.Label(name);

        string[] tempSplit;
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            if (file.EndsWith(".overrideController"))
            {
                tempSplit = file.Split('/');
                GUILayout.Label("- " + tempSplit[tempSplit.Length - 1].Split('.')[0]);
            }
        }
    }
}