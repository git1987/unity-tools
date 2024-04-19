using UnityEngine;

namespace UnityTools
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
    /// <summary>
    /// 在Inspector中只显示，不可以编辑
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
}