using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//ref :https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
/*
public class Test
{
    [ReadOnly] public string a;
    [ReadOnly] public int b;
    [ReadOnly] public Material c;
    [ReadOnly] public List<int> d = new List<int>();
}*/
