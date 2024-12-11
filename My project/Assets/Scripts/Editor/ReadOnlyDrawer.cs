#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Drawer para manejar el atributo ReadOnly en el Inspector.
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // Desactiva la edici�n
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;  // Reactiva la edici�n para otros campos
    }
}
#endif