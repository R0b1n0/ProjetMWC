using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MarbleManager))]
public class MarbleManagerEditor : Editor
{
    MarbleManager script;
    private ReorderableList moodOrder;

    private void OnEnable()
    {
        script = (MarbleManager)target;
        
        //Set the array with default values
        SerializedProperty MoodList = serializedObject.FindProperty("moodOrder");
        if (MoodList.arraySize == 0 )
        {
            MoodList.arraySize = System.Enum.GetValues(typeof(Mood)).Length;

            for (int i = 0; i < MoodList.arraySize; i++)
                MoodList.GetArrayElementAtIndex(i).enumValueIndex = i;
            serializedObject.ApplyModifiedProperties();
        }

        moodOrder = new ReorderableList(serializedObject, MoodList, true, true, false, false);

        moodOrder.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Marble Order");
        };

        moodOrder.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty element =
                MoodList.GetArrayElementAtIndex(index);

            string label =
                element.enumDisplayNames[element.enumValueIndex];

            EditorGUI.LabelField(rect, label);
        };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        GUILayout.Space(5);

        moodOrder.DoLayoutList();

        if (GUILayout.Button("Start Lerping"))
        {
            script.TriggerLerpInAnimation();
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
