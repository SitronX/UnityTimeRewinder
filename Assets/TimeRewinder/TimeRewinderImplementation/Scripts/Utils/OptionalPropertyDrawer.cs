#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(OptionalParticleSettings))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(valueProperty);
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            var valueProperty = property.FindPropertyRelative("value");
            var enabledProperty = property.FindPropertyRelative("enabled");

            EditorGUI.BeginProperty(position, label, property);
            position.width -= 24;
            EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
            EditorGUI.PropertyField(position, valueProperty, label, true);
            EditorGUI.EndDisabledGroup();

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            position.x += position.width + 24;
            position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
            position.x -= position.width;
            EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
#endif