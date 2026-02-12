using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RtpcDependent))]
public class RtpcDependentEditor : PropertyDrawer
{
    float propertyHeight;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        propertyHeight = 0;

        //Foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            propertyHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.EndProperty();
            return;
        }

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight + 2;
        propertyHeight += EditorGUIUtility.singleLineHeight + 2;

        //Permanent fields
        SerializedProperty rtpcProp = property.FindPropertyRelative("rtpc");
        DrawDefaultProperty(ref position, rtpcProp);
        SerializedProperty wwiseObjRef = rtpcProp.FindPropertyRelative("WwiseObjectReference");

        if (wwiseObjRef == null || wwiseObjRef.objectReferenceValue == null)
        {
            DrawDefaultProperty(ref position, property.FindPropertyRelative("baseValue"));
            EditorGUI.indentLevel--;
            return;
        }

        DisplayMinMax(ref position, property);
        var evaluationMode = property.FindPropertyRelative("evaluationMode");
        DrawDefaultProperty(ref position, evaluationMode);

        //Evaluation Mode
        switch (evaluationMode.enumValueIndex)
        {
            case 0:

                break;

            case 1:
                DrawDefaultProperty(ref position, property.FindPropertyRelative("outputMin"));
                DrawDefaultProperty(ref position, property.FindPropertyRelative("outputMax"));
                break;
            case 2:
                DrawDefaultProperty(ref position, property.FindPropertyRelative("normalizedCurve"));
                break;
            case 3:
                DrawDefaultProperty(ref position, property.FindPropertyRelative("outputMin"));
                DrawDefaultProperty(ref position, property.FindPropertyRelative("outputMax"));
                DrawDefaultProperty(ref position, property.FindPropertyRelative("rangeCurve"));
                break;
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    private void DisplayMinMax(ref Rect pos, SerializedProperty prop)
    {
        SerializedProperty min = prop.FindPropertyRelative("rtpcMin");
        SerializedProperty max = prop.FindPropertyRelative("rtpcMax");

        DrawDefaultProperty(ref pos, min);
        DrawDefaultProperty(ref pos, max);

        if (max.intValue < min.intValue)
        {
            EditorGUI.HelpBox(pos,"Min can't be above max ...",MessageType.Error);
            pos.y += EditorGUIUtility.singleLineHeight + 2;
            propertyHeight += EditorGUIUtility.singleLineHeight + 2;
        }
    }

    void DrawDefaultProperty(ref Rect pos, SerializedProperty prop)
    {
        EditorGUI.PropertyField(pos, prop);
        pos.y += EditorGUIUtility.singleLineHeight + 2;
        propertyHeight += EditorGUIUtility.singleLineHeight + 2;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return propertyHeight;
    }
}
