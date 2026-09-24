using UnityEngine;

#if UNITY_EDITOR
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
                new GUIContent(label+"_Profile")
            );

            // ビジュアル用
            EditorGUILayout.PropertyField(
                visuals.GetArrayElementAtIndex(i),
                new GUIContent(label+"_Visual")
            );

            // Prefab用
            EditorGUILayout.PropertyField(
                prefabs.GetArrayElementAtIndex(i),
                new GUIContent(label+"_Prefab")
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

        serializedObject.ApplyModifiedProperties();
    }
}

#endif

public class CharaDataManager : MonoBehaviour
{
    /// <summary>
    /// キャラID
    /// </summary>
    public enum CharaID : byte
    {
        NARBO,
        SLASHER,
        BOY,
        ZERA,
        MAX
    }

    public static CharaDataManager Instance { get; private set; }

    [SerializeField] CharacterProfile[] characterProfiles;
    [SerializeField] CharaVisualData[] charaVisualDatas;
    [SerializeField] CharacterController[] characterPrefabs;

    [SerializeField] CharacterProfile errorProfile;
    [SerializeField] CharaVisualData errorVisualData;
    [SerializeField] CharacterController errorPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //すでにほかのContollerInputManagerがある場合は削除
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //シングルトン化
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// キャラのテキストデータを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterProfile GetCharacterProfile(int id)
    {
        if (id < 0 || id >= characterProfiles.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorProfile; }

        if (characterProfiles[id] == null) { Debug.LogError("Profileがありません。"); return errorProfile; }

        return characterProfiles[id];
    }
    public CharacterProfile GetCharacterProfile(CharaID id)
    {
        int _id = (int)id;
        return GetCharacterProfile(_id);
    }

    /// <summary>
    /// キャラのビジュアル系データを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharaVisualData GetVisualData(int id)
    {
        if (id < 0 || id >= charaVisualDatas.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorVisualData; }

        if (charaVisualDatas[id] == null) { Debug.LogError("ビジュアルデータがありません。"); return errorVisualData; }

        return charaVisualDatas[id];
    }
    public CharaVisualData GetVisualData(CharaID id)
    {
        int _id = (int)id;
        return GetVisualData(_id);
    }

    /// <summary>
    /// キャラのビジュアル系データをすべて返す
    /// </summary>
    /// <returns></returns>
    public CharaVisualData[] GetAllVisualDatas()
    {
        return charaVisualDatas;
    }

    /// <summary>
    /// キャラのPrefabを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterController GetCharaPrefab(int id)
    {
        if(id < 0 || id >= characterPrefabs.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorPrefab; }

        if (characterPrefabs[id] == null) { Debug.LogError("Prefabがありません。"); return errorPrefab; }

        return characterPrefabs[id];
    }
    public CharacterController GetCharaPrefab(CharaID id)
    {
        int _id = (int)id;
        return GetCharaPrefab(_id);
    }
}
