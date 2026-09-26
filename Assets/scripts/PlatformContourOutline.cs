using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlatformContourOutline : MonoBehaviour
{
    public void Build(
        IReadOnlyList<List<Vector2>> contours,
        float depth,
        Material material,
        float width,
        float surfaceOffset)
    {
        if (contours == null || material == null)
            return;

        float halfDepth = Mathf.Max(0.01f, depth) * 0.5f;

        for (int i = 0; i < contours.Count; i++)
        {
            List<Vector2> contour = contours[i];

            if (contour == null || contour.Count < 3)
                continue;

            CreateLoop(
                "FrontContour_" + i,
                contour,
                -halfDepth - surfaceOffset,
                material,
                width
            );

            CreateLoop(
                "BackContour_" + i,
                contour,
                halfDepth + surfaceOffset,
                material,
                width
            );
        }
    }

    private void CreateLoop(
        string objectName,
        List<Vector2> contour,
        float z,
        Material material,
        float width)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = false;
        line.loop = true;
        line.startWidth = width;
        line.endWidth = width;
        line.textureMode = LineTextureMode.Tile;
        line.alignment = LineAlignment.View;
        line.numCapVertices = 0;
        line.numCornerVertices = 4;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.positionCount = contour.Count;

        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 point = contour[i];
            line.SetPosition(i, new Vector3(point.x, point.y, z));
        }
    }
}