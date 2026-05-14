using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ANF.Editor
{
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
        private Toggle createTemplateAnimationsToggle;

        private CharacterGeneratorType type;

        public enum CharacterGeneratorType
        {
            Default,
            MouthEyeSprites
        }

        public void Init(CharacterGeneratorType type)
        {
            this.type = type;
            CreateGUIElements();
        }


        [MenuItem("ANF/Character Generator/Mouth & Eyes Sprites")]
        public static void ShowHQGenerator()
        {
            CharacterGeneratorEditor wnd = CreateWindow<CharacterGeneratorEditor>();
            wnd.Init(CharacterGeneratorType.MouthEyeSprites);
            wnd.titleContent = new GUIContent("Character Generator");
        }

        [MenuItem("ANF/Character Generator/Default")]
        public static void ShowDefaultGenerator()
        {
            CharacterGeneratorEditor wnd = CreateWindow<CharacterGeneratorEditor>();
            wnd.Init(CharacterGeneratorType.Default);
            wnd.titleContent = new GUIContent("Character Generator");
        }

        public void CreateGUIElements()
        {
            VisualElement root = rootVisualElement;

            Label label = new Label("Character Generator");
            root.Add(label);

            characterIdField = new TextField("Character's ID");
            root.Add(characterIdField);

            modelField = new ObjectField("Character's model");
            modelField.objectType = typeof(GameObject);
            root.Add(modelField);

            interactionIconField = new ObjectField("Interaction Icon");
            interactionIconField.objectType = typeof(Texture2D);
            root.Add(interactionIconField);

            characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_NAME", ""));
            modelField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("ANF_CG_MODEL", null)));
            interactionIconField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("ANF_CG_INTERACTION", null)));


            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                hierarchyField = new TextField("Path to Head");
                root.Add(hierarchyField);

                eyesField = new ObjectField("Eyes sprite");
                eyesField.objectType = typeof(Texture2D);
                root.Add(eyesField);

                mouthField = new ObjectField("Mouth Sprite");
                mouthField.objectType = typeof(Texture2D);
                root.Add(mouthField);

                spriteMaterialField = new ObjectField("Sprite Material");
                spriteMaterialField.objectType = typeof(Material);
                root.Add(spriteMaterialField);

                hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_HEADPATH",
                    "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
                mouthField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("ANF_CG_MOUTH", null)));
                eyesField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("ANF_CG_EYE", null)));
                spriteMaterialField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Material>(EditorPrefs.GetString("ANF_CG_MATERIAL", null)));

            }

            createTemplateAnimationsToggle = new Toggle("Create Template Animations");
            root.Add(createTemplateAnimationsToggle);

            createTemplateAnimationsToggle.SetValueWithoutNotify(EditorPrefs.GetBool("ANF_CG_CREATETEMPLATE", true));


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
            EditorPrefs.SetString("ANF_CG_NAME", "");

            characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_NAME", ""));

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                EditorPrefs.SetString("ANF_CG_HEADPATH", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End");
                hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_HEADPATH",
                    "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
            }
        }

        public void OnGenerate()
        {
            if (type == CharacterGeneratorType.MouthEyeSprites && (spriteMaterialField.value == null || eyesField.value == null ||
                mouthField.value == null || string.IsNullOrEmpty(hierarchyField.value)))
                return;

            if (modelField.value == null || interactionIconField.value == null || string.IsNullOrEmpty(characterIdField.value))
                return;

            EditorPrefs.SetString("ANF_CG_NAME", characterIdField.value);
            EditorPrefs.SetString("ANF_CG_MODEL", AssetDatabase.GetAssetPath(modelField.value));
            EditorPrefs.SetString("ANF_CG_INTERACTION", AssetDatabase.GetAssetPath(interactionIconField.value));
            EditorPrefs.SetBool("ANF_CG_CREATETEMPLATE", createTemplateAnimationsToggle.value);

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                EditorPrefs.SetString("ANF_CG_HEADPATH", hierarchyField.value);
                EditorPrefs.SetString("ANF_CG_MOUTH", AssetDatabase.GetAssetPath(mouthField.value));
                EditorPrefs.SetString("ANF_CG_EYE", AssetDatabase.GetAssetPath(eyesField.value));
                EditorPrefs.SetString("ANF_CG_MATERIAL", AssetDatabase.GetAssetPath(spriteMaterialField.value));
            }

            Sprite defaultMouthSprite = null;
            Sprite defaultEyeSprite = null;

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyesField.value));
                foreach (Object sprite in sprites)
                {
                    if (sprite is Sprite && sprite.name.EndsWith('0'))
                    {
                        defaultEyeSprite = sprite as Sprite;
                        break;
                    }
                }

                sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthField.value));
                foreach (Object sprite in sprites)
                {
                    if (sprite is Sprite && sprite.name.EndsWith('0'))
                    {
                        defaultMouthSprite = sprite as Sprite;
                        break;
                    }
                }
            }

            // Generate roots
            GameObject characterRoot = new GameObject(characterIdField.value);
            characterRoot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            GameObject modelRoot = PrefabUtility.InstantiatePrefab(modelField.value as GameObject) as GameObject;
            modelRoot.name = "Model";
            //PrefabUtility.UnpackPrefabInstance(modelField.value as GameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            modelRoot.transform.SetParent(characterRoot.transform);
            modelRoot.transform.localScale = Vector3.one;

            // Generate the interaction
            GameObject interactableRoot = new GameObject("Interaction");
            Scene.InteractableObject interactableObject = interactableRoot.AddComponent<Scene.InteractableObject>();
            interactableRoot.layer = LayerMask.NameToLayer("Interaction");
            BoxCollider interactionCollider = interactableRoot.AddComponent<BoxCollider>();
            interactionCollider.center = new Vector3(0.05509984f, 3.324028f, 0f);
            interactionCollider.size = new Vector3(2.307784f, 6.658423f, 1f);
            interactableRoot.transform.SetParent(modelRoot.transform);
            interactableRoot.transform.localScale = Vector3.one;

            // Generate Animations
            string pathToAnimations = "Assets/Resources/Animations/Characters/" + characterIdField.value + "/";
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Characters/" + characterIdField.value))
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Characters", characterIdField.value);

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Characters/" + characterIdField.value + "/Body"))
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Characters/" + characterIdField.value, "Body");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Characters/" + characterIdField.value + "/Eye"))
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Characters/" + characterIdField.value, "Eye");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Characters/" + characterIdField.value + "/Mouth"))
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Characters/" + characterIdField.value, "Mouth");


            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
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
                mouthRoot.transform.localPosition = new Vector3(0f, -0.737f, 0.319f);

                SpriteRenderer spriteRenderer = mouthRoot.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultMouthSprite;
                spriteRenderer.material = spriteMaterialField.value as Material;
                spriteRenderer.sortingOrder = 1;

                // Generate eye animator
                GameObject eyeRoot = new GameObject("Eye");
                eyeRoot.transform.SetParent(headRoot);
                eyeRoot.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                eyeRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
                eyeRoot.transform.localPosition = new Vector3(0, -0.356f, 0.318f);

                spriteRenderer = eyeRoot.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultEyeSprite;
                spriteRenderer.material = spriteMaterialField.value as Material;
                spriteRenderer.sortingOrder = 1;
            }

            // Add Character

            interactableObject.EditorInit(characterIdField.value, interactionIconField.value as Texture2D,
                    modelRoot.GetComponentsInChildren<Renderer>(),
                    interactionCollider);

            Scene.Character character = characterRoot.AddComponent<Scene.Character>();
            character.EditorInit(characterIdField.value, characterRoot.AddComponent<Animator>(),
                characterRoot.GetComponentsInChildren<Renderer>(),
                interactableObject);

            Editor.CharacterEditor.CreateDefaultController(character, pathToAnimations);

            Animator animator = character.GetAnimator();
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;

            if (createTemplateAnimationsToggle.value)
            {
                Editor.CharacterEditor.CreateBodyFromTemplate(controller, controller.layers[0].stateMachine,
                    pathToAnimations + "Body/", "Normal");

                if (type == CharacterGeneratorType.MouthEyeSprites)
                {
                    Editor.CharacterEditor.CreateEyeFromTemplate(controller, controller.layers[1].stateMachine,
                        pathToAnimations + "Eye/", "Normal", eyesField.value as Texture2D);

                    Editor.CharacterEditor.CreateMouthFromTemplate(controller, controller.layers[2].stateMachine,
                        pathToAnimations + "Mouth/", "Normal", mouthField.value as Texture2D);
                }
            }


            // Generate Prefab
            bool success;
            PrefabUtility.SaveAsPrefabAssetAndConnect(characterRoot,
                "Assets/Resources/Characters/" + characterIdField.value + ".prefab",
                InteractionMode.AutomatedAction, out success);

            AssetDatabase.SaveAssets();

            if (success) Debug.LogWarning("Character Generation finished");
            else Debug.LogError("Character Generation failed");

            Close();
        }
    }
}