using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharaDataManager))]
public class CharaDataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // プロファイルフィールド Prefabフィールド 表示
        SerializedProperty profiles = serializedObject.FindProperty("characterProfiles");
        SerializedProperty visuals = serializedObject.FindProperty("charaVisualDatas");
        SerializedProperty prefabs = serializedObject.FindProperty("characterPrefabs");

        EditorGUILayout.LabelField("キャラデータ", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 配列 設定
        profiles.arraySize = (int)CharaDataManager.CharaID.MAX;
        visuals.arraySize = (int)CharaDataManager.CharaID.MAX;
        prefabs.arraySize = (int)CharaDataManager.CharaID.MAX;
        for (int i = 0; i < profiles.arraySize; i++)
        {
            string label = i < (int)CharaDataManager.CharaID.MAX ? ((CharaDataManager.CharaID)i).ToString() : $"Element {i}";

            // プロファイル用
            EditorGUILayout.PropertyField(
                profiles.GetArrayElementAtIndex(i),
                new GUIContent(label + "_Profile")
            );

            // ビジュアル用
            EditorGUILayout.PropertyField(
                visuals.GetArrayElementAtIndex(i),
                new GUIContent(label + "_Visual")
            );

            // Prefab用
            EditorGUILayout.PropertyField(
                prefabs.GetArrayElementAtIndex(i),
                new GUIContent(label + "_Prefab")
            );

            EditorGUILayout.Space();
        }

        EditorGUILayout.Space(20);


        // エラー用 表示
        SerializedProperty errorProfile = serializedObject.FindProperty("errorProfile");
        SerializedProperty errorVisual = serializedObject.FindProperty("errorVisualData");
        SerializedProperty errorPrefab = serializedObject.FindProperty("errorPrefab");

        EditorGUILayout.LabelField("エラー用キャラデータ", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // プロファイル用
        EditorGUILayout.PropertyField(errorProfile, new GUIContent("Error_Profile"));
        // ビジュアル用
        EditorGUILayout.PropertyField(errorVisual, new GUIContent("Error_Visual"));
        // Prefab用
        EditorGUILayout.PropertyField(errorPrefab, new GUIContent("Error_Prefab"));

        EditorGUILayout.Space();

        SerializedProperty modelVisualDatas = serializedObject.FindProperty("modelVisualDatas");
        EditorGUILayout.PropertyField(modelVisualDatas);

        serializedObject.ApplyModifiedProperties();
    }
}