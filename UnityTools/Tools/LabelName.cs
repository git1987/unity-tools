using UnityEngine;

namespace UnityTools
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(LabelNameAttribute))]
    public class LabelNameDrawer : PropertyDrawer
    {
        private GUIContent guiContent;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (guiContent == null)
            {
                string name = (attribute as LabelNameAttribute).name;
                guiContent = new GUIContent(name);
            }
            EditorGUI.PropertyField(position, property, guiContent, true);
        }
    }
#endif
    /// <summary>
    /// 设定变量在Inspector中显示的名称
    /// </summary>
    public class LabelNameAttribute : PropertyAttribute
    {
        public string name;
        public LabelNameAttribute(string name)
        {
            this.name = name;
        }
    }
}