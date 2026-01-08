using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlobManager))]
public class BlobManagerEditor : Editor
{
    BlobManager script;

    private void OnEnable()
    {
        script = (BlobManager)target;
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
