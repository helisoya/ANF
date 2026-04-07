using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RPGCharacterStatsConstants))]
public class RPGCharacterStatsConstantsEditor : Editor
{
    AnimationCurve health;
    AnimationCurve attack;
    AnimationCurve evasion;
    AnimationCurve sp;
    AnimationCurve price;
    AnimationCurve defense;
    AnimationCurve expCap;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        health = new AnimationCurve();
        attack = new AnimationCurve();
        evasion = new AnimationCurve();
        sp = new AnimationCurve();
        price = new AnimationCurve();
        defense = new AnimationCurve();
        expCap = new AnimationCurve();

        for (int level = 1; level <= 99; level++)
        {
            health.AddKey(level, serializedObject.FindProperty("HEALTH_BASE").intValue +
                Mathf.FloorToInt(serializedObject.FindProperty("HEALTH_MULT_LEVEL").floatValue * level));

            attack.AddKey(level, Mathf.FloorToInt(
                (1.0f + 1 / serializedObject.FindProperty("ATTACK_MULT_FORCE").floatValue + level / serializedObject.FindProperty("ATTACK_MULT_LEVEL").floatValue)
                * serializedObject.FindProperty("ATTACK_BASE").intValue));

            evasion.AddKey(level, serializedObject.FindProperty("EVASION_BASE").floatValue + 1 / serializedObject.FindProperty("EVASION_MULT_AGILITY").floatValue);

            sp.AddKey(level, serializedObject.FindProperty("SP_BASE").intValue + Mathf.FloorToInt(level * serializedObject.FindProperty("SP_MULT_LEVEL").floatValue + 1 * serializedObject.FindProperty("SP_MULT_STRATEGY").floatValue));

            price.AddKey(level, serializedObject.FindProperty("PRICE_BASE").floatValue - 1 / serializedObject.FindProperty("PRICE_MULT_CHARISMA").floatValue);

            defense.AddKey(level, Mathf.FloorToInt(
            (1.0f + 1 / serializedObject.FindProperty("DEFENSE_MULT_RESILIENCE").floatValue + level / serializedObject.FindProperty("DEFENSE_MULT_LEVEL").floatValue)
            * serializedObject.FindProperty("DEFENSE_BASE").intValue));

            expCap.AddKey(level, serializedObject.FindProperty("EXP_BASE").intValue + Mathf.FloorToInt(Mathf.Pow(level * 2, 2)));
        }


        EditorGUILayout.CurveField("Max health", health);
        EditorGUILayout.CurveField("Base attack", attack);
        EditorGUILayout.CurveField("Evasion", evasion);
        EditorGUILayout.CurveField("Max SP", sp);
        EditorGUILayout.CurveField("Price reduction", price);
        EditorGUILayout.CurveField("Defense", defense);
        EditorGUILayout.CurveField("Exp Cap", expCap);
    }
}
