using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModelVisualData))]
public class ModelVisualDataEditor : Editor
{
    private SerializedProperty partSpriteProperty;
    private SerializedProperty keyVisualProperty;
    private SerializedProperty bustUpProperty;

    private void OnEnable()
    {
        partSpriteProperty = serializedObject.FindProperty("partSprite");
        keyVisualProperty = serializedObject.FindProperty("KeyVisual");
        bustUpProperty = serializedObject.FindProperty("BustUp");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 最大数
        int layerCount = (int)ModelVisualData.LayerType.MAX;

        // 配列サイズを固定
        if (partSpriteProperty.arraySize != layerCount)
            partSpriteProperty.arraySize = layerCount;

        // 各要素をLayerTypeの名前で表示
        for (int i = 0; i < layerCount; i++)
        {
            var layerType = (ModelVisualData.LayerType)i;

            SerializedProperty element = partSpriteProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.PropertyField(element, new GUIContent(layerType.ToString()));
        }

        EditorGUILayout.Space();

        // 通常のInspector表示
        EditorGUILayout.PropertyField(keyVisualProperty);
        EditorGUILayout.PropertyField(bustUpProperty);

        serializedObject.ApplyModifiedProperties();
    }
}