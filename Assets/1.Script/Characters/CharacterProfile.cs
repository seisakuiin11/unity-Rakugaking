using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CharacterProfile))]
public class CharacterProfileEditor : Editor
{
    readonly string[] labels =
    {
        "上攻撃",
        "左攻撃",
        "右攻撃",
        "下攻撃"
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // キャラ名フィールド 表示
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CharaName"));

        // 技名フィールド 表示
        SerializedProperty attacks = serializedObject.FindProperty("SkillNames");
        SerializedProperty explanation = serializedObject.FindProperty("SkillExplanation");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("攻撃名", EditorStyles.boldLabel);

        // 配列 設定
        if(attacks.arraySize < 4) attacks.arraySize = 4; // 技は最低4つあるため (派生技は除く)
        explanation.arraySize = attacks.arraySize;
        for (int i = 0; i < attacks.arraySize; i++)
        {
            string label = i < labels.Length ? labels[i] : $"派生技 {i-3}";

            // レイアウトを横方向に (削除ボタンが横にくるようにするため)
            EditorGUILayout.BeginHorizontal();

            var attack = attacks.GetArrayElementAtIndex(i);
            EditorGUILayout.LabelField(label,GUILayout.Width(60));

            attack.stringValue = EditorGUILayout.TextField(attack.stringValue);

            // 派生技のみ削除可能にする
            if (i >= 4)
            {
                if (GUILayout.Button("削除", GUILayout.Width(50)))
                {
                    attacks.DeleteArrayElementAtIndex(i);
                    explanation.DeleteArrayElementAtIndex(i);
                    break; // 配列サイズが変わるのでループを抜ける
                }
            }

            EditorGUILayout.EndHorizontal();

            SerializedProperty element = explanation.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("説明", GUILayout.Width(60));
            element.stringValue = EditorGUILayout.TextArea(
                element.stringValue,
                GUILayout.MinHeight(60)
            );

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("派生技を追加", GUILayout.Width(120)))
        {
            attacks.InsertArrayElementAtIndex(attacks.arraySize);
            // 新しい要素を空文字で初期化
            attacks.GetArrayElementAtIndex(attacks.arraySize - 1).stringValue = "";
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "ScriptableObjects/CreateCharacterProfile")]
public class CharacterProfile : ScriptableObject
{
    public const byte ATTACK_UP = 0, ATTACK_LEFT = 1, ATTACK_RIGHT = 2, ATTACK_DOWN = 3;
    /// <summary>
    /// キャラクターの名前
    /// </summary>
    [Header("キャラ名")]
    public string CharaName;
    /// <summary>
    /// 各種スキルの名前 
    /// </summary>
    public string[] SkillNames;
    /// <summary>
    /// 各種スキルの説明
    /// </summary>
    [TextArea(3,10)]
    public string[] SkillExplanation;
}
