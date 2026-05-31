using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CityBuilder : EditorWindow
{
    public List<GameObject> buildingPrefabs = new List<GameObject>();
    private float offsetFromRoad = 0.3f;
    private float buildingDistance = 0.1f;
    private float buildingRotationOffset = 0f;
    private Transform cityParent;

    [MenuItem("Tools/City Builder")]
    public static void ShowWindow()
    {
        GetWindow<CityBuilder>("City Builder");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Prefab Settings", EditorStyles.boldLabel);
        
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("buildingPrefabs");
        if (stringsProperty != null)
        {
            EditorGUILayout.PropertyField(stringsProperty, true);
            so.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Smart Layout Settings", EditorStyles.boldLabel);
        offsetFromRoad = EditorGUILayout.FloatField("Distance from Edge", offsetFromRoad);
        buildingDistance = EditorGUILayout.FloatField("Gap Between Buildings", buildingDistance);
        buildingRotationOffset = EditorGUILayout.FloatField("Rotation Offset", buildingRotationOffset);
        
        EditorGUILayout.Space();
        cityParent = (Transform)EditorGUILayout.ObjectField("City Parent", cityParent, typeof(Transform), true);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate Smart City", GUILayout.Height(40)))
        {
            GenerateCity();
        }
    }

    private void GenerateCity()
    {
        if (buildingPrefabs.Count == 0 || Selection.gameObjects.Length == 0) return;

        if (cityParent == null)
        {
            GameObject newParent = new GameObject("Procedural City");
            cityParent = newParent.transform;
        }

        int generatedCount = 0;

        foreach (GameObject road in Selection.gameObjects)
        {
            Bounds roadBounds = GetWorldBounds(road);
            if (roadBounds.size == Vector3.zero) continue;

            float sizeX = roadBounds.size.x;
            float sizeZ = roadBounds.size.z;
            float roadY = roadBounds.max.y;

            bool isSquare = Mathf.Abs(sizeX - sizeZ) < 2f; 

            if (isSquare)
            {
                generatedCount += BuildAlongEdge(road, true, true, roadBounds.min.x, roadBounds.max.x, roadBounds.max.z + offsetFromRoad, roadY);
                generatedCount += BuildAlongEdge(road, true, false, roadBounds.min.x, roadBounds.max.x, roadBounds.min.z - offsetFromRoad, roadY);
                generatedCount += BuildAlongEdge(road, false, true, roadBounds.min.z, roadBounds.max.z, roadBounds.max.x + offsetFromRoad, roadY);
                generatedCount += BuildAlongEdge(road, false, false, roadBounds.min.z, roadBounds.max.z, roadBounds.min.x - offsetFromRoad, roadY);
            }
            else
            {
                bool isAlongX = sizeX > sizeZ;

                float roadMin = isAlongX ? roadBounds.min.x : roadBounds.min.z;
                float roadMax = isAlongX ? roadBounds.max.x : roadBounds.max.z;

                float rightEdge = isAlongX ? roadBounds.max.z : roadBounds.max.x;
                float leftEdge = isAlongX ? roadBounds.min.z : roadBounds.min.x;

                generatedCount += BuildAlongEdge(road, isAlongX, true, roadMin, roadMax, rightEdge + offsetFromRoad, roadY);
                generatedCount += BuildAlongEdge(road, isAlongX, false, roadMin, roadMax, leftEdge - offsetFromRoad, roadY);
            }
        }

        Debug.Log($"[City Builder] Successfully generated {generatedCount} buildings!");
    }

    private int BuildAlongEdge(GameObject road, bool isAlongX, bool isRightSide, float minPos, float maxPos, float edgeCoord, float roadY)
    {
        int count = 0;
        float currentPos = minPos;

        while (currentPos <= maxPos)
        {
            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
            if (prefab == null) continue;

            float faceAngle = isAlongX ? (isRightSide ? 180f : 0f) : (isRightSide ? -90f : 90f);
            Quaternion facingRot = Quaternion.Euler(0, faceAngle + buildingRotationOffset, 0);

            Vector3 bSize = GetPrefabWorldSize(prefab, facingRot);
            float bWidthAlongRoad = isAlongX ? bSize.x : bSize.z;
            float bDepth = isAlongX ? bSize.z : bSize.x;
            float bHeight = bSize.y;

            if (currentPos + bWidthAlongRoad > maxPos + 0.1f) break;

            float centerPosAlongRoad = currentPos + (bWidthAlongRoad / 2f);
            float centerPosPushAway = isRightSide ? edgeCoord + (bDepth / 2f) : edgeCoord - (bDepth / 2f);

            Vector3 finalPos = isAlongX ? 
                new Vector3(centerPosAlongRoad, roadY, centerPosPushAway) : 
                new Vector3(centerPosPushAway, roadY, centerPosAlongRoad);

            Physics.SyncTransforms();

            Vector3 overlapCenter = finalPos + new Vector3(0, bHeight / 2f, 0);
            Vector3 checkSize = new Vector3(bSize.x * 0.45f, (bHeight / 2f) + 0.5f, bSize.z * 0.45f);

            Collider[] colliders = Physics.OverlapBox(overlapCenter, checkSize, Quaternion.identity);
            bool isSpaceEmpty = true;

            foreach (Collider col in colliders)
            {
                string colName = col.gameObject.name.ToLower();
                if (col.gameObject != road && !col.isTrigger && !colName.Contains("terrain") && !colName.Contains("plane"))
                {
                    isSpaceEmpty = false;
                    break;
                }
            }

            if (isSpaceEmpty)
            {
                GameObject newBuilding = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                newBuilding.transform.position = finalPos;
                newBuilding.transform.rotation = facingRot;
                newBuilding.transform.parent = cityParent;
                
                Undo.RegisterCreatedObjectUndo(newBuilding, "City Builder Generate");
                
                count++;
                currentPos += bWidthAlongRoad + buildingDistance;
            }
            else
            {
                currentPos += 0.5f;
            }
        }
        return count;
    }

    private Bounds GetWorldBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    private Vector3 GetPrefabWorldSize(GameObject prefab, Quaternion rotation)
    {
        GameObject temp = Instantiate(prefab, Vector3.zero, rotation);
        temp.hideFlags = HideFlags.HideAndDontSave;
        
        Bounds b = GetWorldBounds(temp);
        Vector3 size = b.size;
        
        DestroyImmediate(temp);
        return size;
    }
}