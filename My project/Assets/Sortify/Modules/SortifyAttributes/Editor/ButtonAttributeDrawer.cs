#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonAttribute buttonAttribute = (ButtonAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                if (GUI.Button(position, buttonAttribute.ButtonText))
                    property.boolValue = !property.boolValue;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use with bool or method.");
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            try
            {
                DrawDefaultInspector();
                var buttonAttributeType = typeof(ButtonAttribute);
                var methods = target.GetType()
                                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m => m.IsDefined(buttonAttributeType, true));

                foreach (var method in methods)
                {
                    var buttonAttribute = (ButtonAttribute)method.GetCustomAttributes(buttonAttributeType, true).First();
                    if (method.GetParameters().Length > 0)
                    {
                        Debug.LogWarning($"Method '{method.Name}' has parameters and cannot be called by a button.");
                        EditorGUILayout.HelpBox($"Method '{method.Name}' has parameters and cannot be called by a button.", MessageType.Warning);
                        continue;
                    }

                    if (GUILayout.Button(buttonAttribute.ButtonText))
                    {
                        try
                        {
                            method.Invoke(target, null);
                            EditorUtility.SetDirty(target);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error invoking method '{method.Name}': {e.Message}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in ButtonEditor: {e.Message}");
                EditorGUILayout.HelpBox($"Error in ButtonEditor: {e.Message}", MessageType.Error);
            }
        }
    }
}
#endif
