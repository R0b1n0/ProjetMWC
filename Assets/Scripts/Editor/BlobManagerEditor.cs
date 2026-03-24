using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlobRenderer))]
public class BlobManagerEditor : Editor
{
    BlobRenderer script;

    private void OnEnable()
    {
        script = (BlobRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);

        if (GUILayout.Button("Start Lerping"))
        {
            script.StartLerping();
        }
    }
}
