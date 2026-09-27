using System.Collections.Generic;
using LibTessDotNet;
using UnityEngine;
using UnityEngine.Rendering;

public static class PolygonExtruder
{
    public static Mesh Build(
        IReadOnlyList<List<Vector2>> contours,
        float depth,
        float textureScale)
    {
        Mesh mesh = new Mesh { name = "StrokePolygonExtrusion" };
        mesh.indexFormat = IndexFormat.UInt32;

        if (contours == null || contours.Count == 0)
            return mesh;

        float halfDepth = Mathf.Max(0.01f, depth) * 0.5f;
        float safeTextureScale = Mathf.Max(0.001f, textureScale);
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        Tess tess = new Tess();

        foreach (List<Vector2> contour in contours)
        {
            if (contour == null || contour.Count < 3)
                continue;

            ContourVertex[] tessContour = new ContourVertex[contour.Count];

            for (int i = 0; i < contour.Count; i++)
            {
                tessContour[i].Position = new Vec3
                {
                    X = contour[i].x,
                    Y = contour[i].y,
                    Z = 0f
                };
            }

            tess.AddContour(tessContour, ContourOrientation.Original);
        }

        tess.Tessellate(
            WindingRule.EvenOdd,
            ElementType.Polygons,
            3
        );

        for (int i = 0; i < tess.ElementCount; i++)
        {
            int firstIndex = tess.Elements[i * 3];
            int secondIndex = tess.Elements[i * 3 + 1];
            int thirdIndex = tess.Elements[i * 3 + 2];

            if (firstIndex == Tess.Undef ||
                secondIndex == Tess.Undef ||
                thirdIndex == Tess.Undef)
            {
                continue;
            }

            Vector2 first = ToVector2(tess.Vertices[firstIndex].Position);
            Vector2 second = ToVector2(tess.Vertices[secondIndex].Position);
            Vector2 third = ToVector2(tess.Vertices[thirdIndex].Position);

            AddFlatTriangle(
                vertices,
                uvs,
                triangles,
                first,
                second,
                third,
                halfDepth,
                true,
                safeTextureScale
            );

            AddFlatTriangle(
                vertices,
                uvs,
                triangles,
                first,
                second,
                third,
                -halfDepth,
                false,
                safeTextureScale
            );
        }

        foreach (List<Vector2> contour in contours)
        {
            if (contour == null || contour.Count < 3)
                continue;

            AddSideWalls(
                vertices,
                uvs,
                triangles,
                contour,
                halfDepth,
                safeTextureScale
            );
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddFlatTriangle(
        List<Vector3> vertices,
        List<Vector2> uvs,
        List<int> triangles,
        Vector2 first,
        Vector2 second,
        Vector2 third,
        float z,
        bool facePositiveZ,
        float textureScale)
    {
        float cross = Cross(first, second, third);
        bool currentlyPositive = cross > 0f;

        if (currentlyPositive != facePositiveZ)
        {
            Vector2 temporary = second;
            second = third;
            third = temporary;
        }

        int index = vertices.Count;
        vertices.Add(new Vector3(first.x, first.y, z));
        vertices.Add(new Vector3(second.x, second.y, z));
        vertices.Add(new Vector3(third.x, third.y, z));
        uvs.Add(first * textureScale);
        uvs.Add(second * textureScale);
        uvs.Add(third * textureScale);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    private static void AddSideWalls(
        List<Vector3> vertices,
        List<Vector2> uvs,
        List<int> triangles,
        List<Vector2> contour,
        float halfDepth,
        float textureScale)
    {
        float distance = 0f;

        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 first = contour[i];
            Vector2 second = contour[(i + 1) % contour.Count];
            float nextDistance = distance + Vector2.Distance(first, second);
            int index = vertices.Count;

            vertices.Add(new Vector3(first.x, first.y, -halfDepth));
            vertices.Add(new Vector3(second.x, second.y, -halfDepth));
            vertices.Add(new Vector3(second.x, second.y, halfDepth));
            vertices.Add(new Vector3(first.x, first.y, halfDepth));

            uvs.Add(new Vector2(distance * textureScale, 0f));
            uvs.Add(new Vector2(nextDistance * textureScale, 0f));
            uvs.Add(new Vector2(nextDistance * textureScale, depthToUv(halfDepth, textureScale)));
            uvs.Add(new Vector2(distance * textureScale, depthToUv(halfDepth, textureScale)));

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);

            distance = nextDistance;
        }
    }

    private static float depthToUv(float halfDepth, float textureScale)
    {
        return halfDepth * 2f * textureScale;
    }

    private static Vector2 ToVector2(Vec3 point)
    {
        return new Vector2(point.X, point.Y);
    }

    private static float Cross(Vector2 first, Vector2 second, Vector2 third)
    {
        Vector2 a = second - first;
        Vector2 b = third - first;
        return a.x * b.y - a.y * b.x;
    }
}
