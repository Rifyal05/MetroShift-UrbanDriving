using UnityEngine;
using UnityEditor;

public class RoadManager : EditorWindow
{
    private float gridSize = 24f;
    private bool fixRotation = true;
    private bool flattenY = true;
    private float targetHeight = 0f;
    private bool snapPosition = true;
    private bool ignoreScaledObjects = true;

    [MenuItem("Tools/Road Manager")]
    public static void ShowWindow()
    {
        GetWindow<RoadManager>("Road Manager");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
        
        EditorGUILayout.Space();
        fixRotation = EditorGUILayout.Toggle("Fix Blender Rotation", fixRotation);
        
        EditorGUILayout.Space();
        flattenY = EditorGUILayout.Toggle("Flatten Height (Y)", flattenY);
        if (flattenY) targetHeight = EditorGUILayout.FloatField("Target Y", targetHeight);
        
        EditorGUILayout.Space();
        snapPosition = EditorGUILayout.Toggle("Snap Grid Position", snapPosition);
        if (snapPosition)
        {
            ignoreScaledObjects = EditorGUILayout.Toggle("Ignore Scaled Objects", ignoreScaledObjects);
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Execute", GUILayout.Height(35)))
        {
            ProcessSelectedObjects();
        }
    }

    private void ProcessSelectedObjects()
    {
        if (Selection.gameObjects.Length == 0) return;

        Undo.RecordObjects(Selection.transforms, "Road Execution");
        int processed = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            Transform t = obj.transform;

            if (fixRotation)
            {
                Vector3 rot = t.eulerAngles;
                rot.x = Mathf.Round(rot.x / 90f) * 90f;
                rot.y = Mathf.Round(rot.y / 90f) * 90f;
                rot.z = Mathf.Round(rot.z / 90f) * 90f;
                t.eulerAngles = rot;
            }

            Vector3 pos = t.position;

            if (flattenY)
            {
                pos.y = targetHeight;
            }

            if (snapPosition)
            {
                bool isModifiedScale = Mathf.Abs(t.localScale.x - 1f) > 0.01f || 
                                       Mathf.Abs(t.localScale.y - 1f) > 0.01f || 
                                       Mathf.Abs(t.localScale.z - 1f) > 0.01f;

                if (!ignoreScaledObjects || !isModifiedScale)
                {
                    pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
                    pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
                }
            }

            t.position = pos;
            processed++;
        }

        Debug.Log($"[Road Manager] Successfully processed {processed} objects.");
    }
}