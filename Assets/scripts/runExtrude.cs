using System.Collections.Generic;
using UnityEngine;

public class runExtrude : MonoBehaviour
{
    public PaperDrawer paperDrawer;
    [Min(0.1f)] public float platformDepth = 2.5f;
    [Min(0.05f)] public float platformThickness = 0.35f;
    [Min(0f)] public float simplifyTolerance = 0.06f;
    [Range(0f, 20f)] public float horizontalSnapAngle = 5f;
    [Min(0.001f)] public float minimumSegmentLength = 0.04f;
    public Material wallMaterial;
    public Material pencilOutlineMaterial;
    public Transform spawnParent;
    public string drawingBlockerLayerName = "DrawingBlocker";

    private readonly List<GameObject> spawned = new List<GameObject>();

    public void ExtrudeAllLines()
    {
        ClearSpawned();

        if (paperDrawer == null)
            paperDrawer = FindFirstObjectByType<PaperDrawer>();

        if (paperDrawer == null)
        {
            Debug.LogError("runExtrude: PaperDrawer was not found.");
            return;
        }

        foreach (GameObject lineObject in paperDrawer.ActiveLines)
        {
            if (lineObject == null || !lineObject.activeInHierarchy)
                continue;

            LineRenderer line = lineObject.GetComponent<LineRenderer>();

            if (line == null || line.positionCount < 2)
                continue;

            List<Vector3> points = GetWorldPoints(line);
            Mesh mesh = LineExtruder.BuildPlatform(points, platformDepth, platformThickness, simplifyTolerance, horizontalSnapAngle, minimumSegmentLength);

            if (mesh.vertexCount == 0)
            {
                Destroy(mesh);
                continue;
            }

            GameObject platform = new GameObject("Platform_" + lineObject.name);

            if (spawnParent != null)
                platform.transform.SetParent(spawnParent, true);

            MeshFilter filter = platform.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = platform.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = wallMaterial;

            MeshCollider collider = platform.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            int blockerLayer = LayerMask.NameToLayer(drawingBlockerLayerName);

            if (blockerLayer >= 0)
                platform.layer = blockerLayer;

            if (pencilOutlineMaterial != null)
            {
                PencilOutline outline = platform.AddComponent<PencilOutline>();
                outline.SetMaterial(pencilOutlineMaterial);
            }

            spawned.Add(platform);
        }
    }

    private static List<Vector3> GetWorldPoints(LineRenderer line)
    {
        List<Vector3> points = new List<Vector3>(line.positionCount);

        for (int i = 0; i < line.positionCount; i++)
        {
            Vector3 point = line.GetPosition(i);

            if (!line.useWorldSpace)
                point = line.transform.TransformPoint(point);

            points.Add(point);
        }

        return points;
    }

    public void ClearSpawned()
    {
        foreach (GameObject platform in spawned)
        {
            if (platform == null)
                continue;

            MeshFilter filter = platform.GetComponent<MeshFilter>();

            if (filter != null && filter.sharedMesh != null)
                Destroy(filter.sharedMesh);

            Destroy(platform);
        }

        spawned.Clear();
    }
}
