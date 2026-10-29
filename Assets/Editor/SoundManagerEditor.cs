using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    SerializedProperty bgmLoadFlag;
    SerializedProperty seLoadFlag;
    SerializedProperty charaseLoadFlag;
    SerializedProperty narboLoadFlag;
    SerializedProperty slasherLoadFlag;
    SerializedProperty boyLoadFlag;
    SerializedProperty zeraLoadFlag;

    private void OnEnable()
    {
        // 紐づけ
        bgmLoadFlag = serializedObject.FindProperty("bgmLoadFlag");
        seLoadFlag = serializedObject.FindProperty("seLoadFlag");
        charaseLoadFlag = serializedObject.FindProperty("charaseLoadFlag");
        narboLoadFlag = serializedObject.FindProperty("narboLoadFlag");
        slasherLoadFlag = serializedObject.FindProperty("slasherLoadFlag");
        boyLoadFlag = serializedObject.FindProperty("boyLoadFlag");
        zeraLoadFlag = serializedObject.FindProperty("zeraLoadFlag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 指定項目以外を表示
        DrawPropertiesExcluding(serializedObject,
            "bgmLoadFlag",
            "seLoadFlag",
            "charaseLoadFlag",
            "narboLoadFlag",
            "slasherLoadFlag",
            "boyLoadFlag",
            "zeraLoadFlag"
        );

        // 描画
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ロードする音素材", EditorStyles.boldLabel);
        bgmLoadFlag.boolValue = EditorGUILayout.ToggleLeft("BGM", bgmLoadFlag.boolValue);
        seLoadFlag.boolValue = EditorGUILayout.ToggleLeft("SE", seLoadFlag.boolValue);
        charaseLoadFlag.boolValue = EditorGUILayout.ToggleLeft("キャラSE", charaseLoadFlag.boolValue);

        // falseなら編集不可
        EditorGUI.BeginDisabledGroup(!charaseLoadFlag.boolValue);

        // 対象となる変数 描画
        narboLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ナルボ", narboLoadFlag.boolValue);
        slasherLoadFlag.boolValue = EditorGUILayout.ToggleLeft("スラッシャー", slasherLoadFlag.boolValue);
        boyLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ボーイ", boyLoadFlag.boolValue);
        zeraLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ゼーラ", zeraLoadFlag.boolValue);

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}