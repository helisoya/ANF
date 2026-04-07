using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Background))]
public class BackgroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        if (GUILayout.Button("Rotate Items"))
        {
            target.GetComponent<Background>().RotateItems();
        }
    }
}
