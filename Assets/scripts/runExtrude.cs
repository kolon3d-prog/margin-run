using System.Collections.Generic;
using UnityEngine;

public class runExtrude : MonoBehaviour
{
    [Header("Sketch")]
    public PaperDrawer paperDrawer;

    [Header("Stroke geometry")]
    [Min(0.01f)] public float strokeRadius = 0.22f;
    [Min(0.1f)] public float platformDepth = 2.5f;
    [Min(0f)] public float simplifyTolerance = 0.04f;
    [Min(100f)] public double clipperIntegerScale = 10000.0;
    [Min(0.001f)] public float textureScale = 1f;

    [Header("Rendering")]
    public Material platformMaterial;
    public Material edgeMaterial;
    [Min(0.005f)] public float edgeWidth = 0.035f;
    [Min(0f)] public float edgeSurfaceOffset = 0.003f;

    [Header("Scene")]
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

            List<Vector3> worldPoints = GetWorldPoints(line);
            List<List<Vector2>> contours = StrokePolygonBuilder.Build(
                worldPoints,
                strokeRadius,
                simplifyTolerance,
                clipperIntegerScale
            );

            if (contours.Count == 0)
                continue;

            Mesh mesh = PolygonExtruder.Build(
                contours,
                platformDepth,
                textureScale
            );

            if (mesh.vertexCount == 0)
            {
                Destroy(mesh);
                continue;
            }

            GameObject platform = new GameObject(
                "Extruded_" + lineObject.name
            );

            if (spawnParent != null)
                platform.transform.SetParent(spawnParent, true);

            MeshFilter filter = platform.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = platform.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = platformMaterial;

            MeshCollider collider = platform.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            int blockerLayer = LayerMask.NameToLayer(
                drawingBlockerLayerName
            );

            if (blockerLayer >= 0)
                platform.layer = blockerLayer;

            if (edgeMaterial != null)
            {
                PlatformContourOutline outline =
                    platform.AddComponent<PlatformContourOutline>();

                outline.Build(
                    contours,
                    platformDepth,
                    edgeMaterial,
                    edgeWidth,
                    edgeSurfaceOffset
                );
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
        for (int i = 0; i < spawned.Count; i++)
        {
            GameObject platform = spawned[i];

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