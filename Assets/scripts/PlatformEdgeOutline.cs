using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlatformEdgeOutline : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.04f;

    public void Build(
        IList<Vector3> points,
        float depth,
        float thickness,
        Material material,
        float width
    )
    {
        lineMaterial = material;
        lineWidth = width;

        if (points == null || points.Count < 2)
            return;

        float halfDepth = depth * 0.5f;

        List<Vector3> topFront =
            new List<Vector3>();

        List<Vector3> topBack =
            new List<Vector3>();

        List<Vector3> bottomFront =
            new List<Vector3>();

        List<Vector3> bottomBack =
            new List<Vector3>();

        foreach (Vector3 point in points)
        {
            Vector3 bottom =
                point + Vector3.down * thickness;

            topFront.Add(
                point + Vector3.back * halfDepth
            );

            topBack.Add(
                point + Vector3.forward * halfDepth
            );

            bottomFront.Add(
                bottom + Vector3.back * halfDepth
            );

            bottomBack.Add(
                bottom + Vector3.forward * halfDepth
            );
        }

        CreateLine("TopFront", topFront);
        CreateLine("TopBack", topBack);
        CreateLine("BottomFront", bottomFront);
        CreateLine("BottomBack", bottomBack);

        CreateEndOutline(
            "StartOutline",
            topFront[0],
            topBack[0],
            bottomBack[0],
            bottomFront[0]
        );

        int last = points.Count - 1;

        CreateEndOutline(
            "EndOutline",
            topFront[last],
            topBack[last],
            bottomBack[last],
            bottomFront[last]
        );
    }

    private void CreateEndOutline(
        string objectName,
        Vector3 topFront,
        Vector3 topBack,
        Vector3 bottomBack,
        Vector3 bottomFront
    )
    {
        List<Vector3> points =
            new List<Vector3>
            {
                topFront,
                topBack,
                bottomBack,
                bottomFront,
                topFront
            };

        CreateLine(objectName, points);
    }

    private void CreateLine(
        string objectName,
        IList<Vector3> points
    )
    {
        GameObject lineObject =
            new GameObject(objectName);

        lineObject.transform.SetParent(
            transform,
            false
        );

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = false;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.textureMode =
            LineTextureMode.Tile;

        line.numCapVertices = 2;
        line.numCornerVertices = 2;

        line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }

        line.shadowCastingMode =
            ShadowCastingMode.Off;

        line.receiveShadows = false;
    }
}