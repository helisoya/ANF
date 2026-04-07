using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents an editor window capable of generating a character
/// </summary>
public class CharacterGeneratorEditor : EditorWindow
{
    private TextField characterIdField;
    private TextField hierarchyField;
    private ObjectField modelField;
    private ObjectField interactionIconField;
    private ObjectField eyesField;
    private ObjectField mouthField;
    private ObjectField spriteMaterialField;


    [MenuItem("LJDN/Character Generator")]
    public static void ShowExample()
    {
        CharacterGeneratorEditor wnd = GetWindow<CharacterGeneratorEditor>();
        wnd.titleContent = new GUIContent("Character Generator");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        Label label = new Label("Character Generator");
        root.Add(label);

        characterIdField = new TextField("Character's ID");
        root.Add(characterIdField);

        modelField = new ObjectField("Character's model");
        modelField.objectType = typeof(GameObject);
        root.Add(modelField);

        hierarchyField = new TextField("Path to Head");
        root.Add(hierarchyField);

        interactionIconField = new ObjectField("Interaction Icon");
        interactionIconField.objectType = typeof(Texture2D);
        root.Add(interactionIconField);

        eyesField = new ObjectField("Eyes sprite");
        eyesField.objectType = typeof(Texture2D);
        root.Add(eyesField);

        mouthField = new ObjectField("Mouth Sprite");
        mouthField.objectType = typeof(Texture2D);
        root.Add(mouthField);

        spriteMaterialField = new ObjectField("Sprite Material");
        spriteMaterialField.objectType = typeof(Material);
        root.Add(spriteMaterialField);

        characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("LJDN_CG_NAME", ""));
        hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("LJDN_CG_HEADPATH",
        "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
        modelField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("LJDN_CG_MODEL", null)));
        interactionIconField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("LJDN_CG_INTERACTION", null)));
        mouthField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("LJDN_CG_MOUTH", null)));
        eyesField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("LJDN_CG_EYE", null)));
        spriteMaterialField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Material>(EditorPrefs.GetString("LJDN_CG_MATERIAL", null)));

        Button button = new Button();
        button.name = "button";
        button.text = "Generate";
        button.clicked += OnGenerate;
        root.Add(button);

        Button resetButton = new Button();
        resetButton.name = "buttonReset";
        resetButton.text = "Reset Values";
        resetButton.clicked += ResetValues;
        root.Add(resetButton);
    }

    void ResetValues()
    {
        EditorPrefs.SetString("LJDN_CG_NAME", "");
        EditorPrefs.SetString("LJDN_CG_HEADPATH", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End");

        characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("LJDN_CG_NAME", ""));
        hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("LJDN_CG_HEADPATH",
        "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
    }

    public void OnGenerate()
    {
        if (modelField.value == null || spriteMaterialField.value == null || eyesField.value == null || mouthField.value == null ||
             interactionIconField.value == null || string.IsNullOrEmpty(characterIdField.value) ||
             string.IsNullOrEmpty(hierarchyField.value)) return;

        EditorPrefs.SetString("LJDN_CG_NAME", characterIdField.value);
        EditorPrefs.SetString("LJDN_CG_HEADPATH", hierarchyField.value);
        EditorPrefs.SetString("LJDN_CG_MODEL", AssetDatabase.GetAssetPath(modelField.value));
        EditorPrefs.SetString("LJDN_CG_INTERACTION", AssetDatabase.GetAssetPath(interactionIconField.value));
        EditorPrefs.SetString("LJDN_CG_MOUTH", AssetDatabase.GetAssetPath(mouthField.value));
        EditorPrefs.SetString("LJDN_CG_EYE", AssetDatabase.GetAssetPath(eyesField.value));
        EditorPrefs.SetString("LJDN_CG_MATERIAL", AssetDatabase.GetAssetPath(spriteMaterialField.value));


        Sprite defaultMouthSprite = null;
        Sprite defaultEyeSprite = null;

        // Generate roots
        GameObject characterRoot = new GameObject(characterIdField.value);
        characterRoot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);


        GameObject modelRoot = PrefabUtility.InstantiatePrefab(modelField.value as GameObject) as GameObject;
        //PrefabUtility.UnpackPrefabInstance(modelField.value as GameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        modelRoot.transform.SetParent(characterRoot.transform);
        modelRoot.transform.localScale = Vector3.one;

        // Generate the interaction
        GameObject interactableRoot = new GameObject("Interaction");
        InteractableObject interactableObject = interactableRoot.AddComponent<InteractableObject>();
        interactableObject.EditorInit(characterIdField.value, interactionIconField.value as Texture2D,
            modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>());
        interactableRoot.layer = LayerMask.NameToLayer("Interaction");
        BoxCollider interactionCollider = interactableRoot.AddComponent<BoxCollider>();
        interactionCollider.center = new Vector3(0.05509984f, 2.136485f, 0);
        interactionCollider.size = new Vector3(2.307784f, 4.283336f, 1f);
        interactableRoot.transform.SetParent(modelRoot.transform);
        interactableRoot.transform.localScale = Vector3.one;


        // Generate Animations
        string pathToAnimations = "Assets/Resources/Animations/Characters/" + characterIdField.value;
        AssetDatabase.CopyAsset("Assets/Resources/Animations/Characters/TEMPLATE", pathToAnimations);

        // Fix Body Animation
        AnimatorOverrideController bodyOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimations + "/Body/Normal.overrideController");
        List<KeyValuePair<AnimationClip, AnimationClip>> clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        bodyOverride.GetOverrides(clipOverrides);
        for (int i = 0; i < clipOverrides.Count; i++)
        {
            Debug.Log(pathToAnimations + "/Body/" + clipOverrides[i].Key.name + ".anim" + " -> " + AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Body/" + clipOverrides[i].Key + ".anim"));

            clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Body/" + clipOverrides[i].Key.name + ".anim"));
        }
        bodyOverride.ApplyOverrides(clipOverrides);

        // Fix Eye Animation
        AnimatorOverrideController eyeOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimations + "/Eye/Normal.overrideController");
        clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        eyeOverride.GetOverrides(clipOverrides);
        for (int i = 0; i < clipOverrides.Count; i++)
        {
            clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Eye/" + clipOverrides[i].Key.name + ".anim"));
        }
        eyeOverride.ApplyOverrides(clipOverrides);

        Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyesField.value));
        foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
        defaultEyeSprite = dicSprites['0'];

        AnimationClip eyeNormalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Eye/Normal_Idle.anim");
        EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(eyeNormalClip)[0];
        ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(eyeNormalClip, binding);
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
        }
        AnimationUtility.SetObjectReferenceCurve(eyeNormalClip, binding, frames);

        // Fix Mouth Animation
        AnimatorOverrideController mouthOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(pathToAnimations + "/Mouth/Normal.overrideController");
        clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        mouthOverride.GetOverrides(clipOverrides);
        for (int i = 0; i < clipOverrides.Count; i++)
        {
            clipOverrides[i] = KeyValuePair.Create(clipOverrides[i].Key,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Mouth/" + clipOverrides[i].Key.name + ".anim"));
        }
        mouthOverride.ApplyOverrides(clipOverrides);

        dicSprites = new Dictionary<char, Sprite>();
        sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthField.value));
        foreach (Object sprite in sprites) { if (sprite is Sprite) dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite); }
        defaultMouthSprite = dicSprites['0'];

        string[] animNames = new string[] { "Normal_Idle", "Normal_Speak" };

        foreach (string animName in animNames)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + "/Mouth/" + animName + ".anim");
            binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)[0];
            frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        }

        AssetDatabase.SaveAssets();


        // Generate body animator
        Animator bodyAnimator = modelRoot.AddComponent<Animator>();
        bodyAnimator.runtimeAnimatorController = bodyOverride;

        Transform headRoot = modelRoot.transform;
        string[] split = hierarchyField.value.Split('/');
        foreach (string path in split)
        {
            headRoot = headRoot.Find(path);
        }

        // Generate mouth animator
        GameObject mouthRoot = new GameObject("Mouth");
        mouthRoot.transform.SetParent(headRoot);
        mouthRoot.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        mouthRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
        mouthRoot.transform.localPosition = new Vector3(-3.5676203e-07f, -0.453999579f, 0.22299999f);

        SpriteRenderer spriteRenderer = mouthRoot.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultMouthSprite;
        spriteRenderer.material = spriteMaterialField.value as Material;
        spriteRenderer.sortingOrder = 1;

        Animator mouthAnimator = mouthRoot.AddComponent<Animator>();
        mouthAnimator.runtimeAnimatorController = mouthOverride;

        // Generate eye animator
        GameObject eyeRoot = new GameObject("Eye");
        eyeRoot.transform.SetParent(headRoot);
        eyeRoot.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        eyeRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
        eyeRoot.transform.localPosition = new Vector3(0, -0.264999986f, 0.223000005f);

        spriteRenderer = eyeRoot.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultEyeSprite;
        spriteRenderer.material = spriteMaterialField.value as Material;
        spriteRenderer.sortingOrder = 1;

        Animator eyeAnimator = eyeRoot.AddComponent<Animator>();
        eyeAnimator.runtimeAnimatorController = eyeOverride;

        // Add Character
        characterRoot.AddComponent<Character>().EditorInit(characterIdField.value, characterRoot.transform,
            bodyAnimator, eyeAnimator, mouthAnimator, characterRoot.GetComponentsInChildren<Renderer>(), 2, interactableObject);


        // Generate Prefab
        bool success;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(characterRoot, "Assets/Resources/Characters/" + characterIdField.value + ".prefab", out success);
        GameObject.DestroyImmediate(characterRoot);
        PrefabUtility.InstantiatePrefab(prefab);

        if (success) Debug.Log("Character Generation finished");
        else Debug.LogError("Character Generation failed");
    }
}
