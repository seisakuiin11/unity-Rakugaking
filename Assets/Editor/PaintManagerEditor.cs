using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PaintManager))]
[CanEditMultipleObjects]
public class PaintManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.propertyPath == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(property, true);
                }

                continue;
            }

            if (property.propertyPath == "layers")
            {
                DrawLayers(property);
            }
            else
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLayers(SerializedProperty layers)
    {
        int layerCount = (int)ModelVisualData.LayerType.MAX;

        layers.isExpanded = EditorGUILayout.Foldout(
            layers.isExpanded,
            layers.displayName,
            true
        );

        if (!layers.isExpanded)
            return;

        EditorGUI.indentLevel++;

        for (int i = 0; i < layerCount; i++)
        {
            SerializedProperty element = layers.GetArrayElementAtIndex(i);

            SerializedProperty parent = element.FindPropertyRelative("layer");

            SerializedProperty children = element.FindPropertyRelative("paintImages");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Element 0 ではなく LayerType の名前を表示
            EditorGUILayout.LabelField(
                ((ModelVisualData.LayerType)i).ToString(),
                EditorStyles.boldLabel
            );

            // 親オブジェクト：編集可能
            EditorGUILayout.PropertyField(
                parent,
                new GUIContent("Layer")
            );

            // 子要素：読み取り専用
            using (new EditorGUI.DisabledScope(true))
            {
                if (children != null && children.arraySize >= 2)
                {
                    EditorGUILayout.PropertyField(
                        children.GetArrayElementAtIndex(0),
                        new GUIContent("Front")
                    );

                    EditorGUILayout.PropertyField(
                        children.GetArrayElementAtIndex(1),
                        new GUIContent("Back")
                    );
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUI.indentLevel--;
    }
}