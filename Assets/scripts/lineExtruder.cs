using System.Collections.Generic;
using UnityEngine;

public static class LineExtruder
{
    public static Mesh BuildPlatform(IList<Vector3> inputPoints, float depth, float thickness, float simplifyTolerance = 0.06f, float horizontalSnapAngle = 5f, float minimumSegmentLength = 0.04f)
    {
        Mesh mesh = new Mesh { name = "DrawnPlatform" };
        List<Vector3> points = PreparePoints(inputPoints, simplifyTolerance, horizontalSnapAngle, minimumSegmentLength);

        if (points.Count < 2)
            return mesh;

        float halfDepth = Mathf.Max(0.01f, depth) * 0.5f;
        float safeThickness = Mathf.Max(0.01f, thickness);
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        float travelled = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 firstTop = points[i];
            Vector3 secondTop = points[i + 1];
            Vector3 firstBottom = firstTop + Vector3.down * safeThickness;
            Vector3 secondBottom = secondTop + Vector3.down * safeThickness;
            float nextTravelled = travelled + Vector3.Distance(firstTop, secondTop);

            AddFace(vertices, uvs, triangles,
                firstTop + Vector3.back * halfDepth,
                firstTop + Vector3.forward * halfDepth,
                secondTop + Vector3.forward * halfDepth,
                secondTop + Vector3.back * halfDepth,
                travelled, nextTravelled);

            AddFace(vertices, uvs, triangles,
                firstBottom + Vector3.back * halfDepth,
                secondBottom + Vector3.back * halfDepth,
                secondBottom + Vector3.forward * halfDepth,
                firstBottom + Vector3.forward * halfDepth,
                travelled, nextTravelled);

            AddFace(vertices, uvs, triangles,
                firstBottom + Vector3.back * halfDepth,
                firstTop + Vector3.back * halfDepth,
                secondTop + Vector3.back * halfDepth,
                secondBottom + Vector3.back * halfDepth,
                travelled, nextTravelled);

            AddFace(vertices, uvs, triangles,
                firstTop + Vector3.forward * halfDepth,
                firstBottom + Vector3.forward * halfDepth,
                secondBottom + Vector3.forward * halfDepth,
                secondTop + Vector3.forward * halfDepth,
                travelled, nextTravelled);

            if (i == 0)
            {
                AddFace(vertices, uvs, triangles,
                    firstTop + Vector3.back * halfDepth,
                    firstBottom + Vector3.back * halfDepth,
                    firstBottom + Vector3.forward * halfDepth,
                    firstTop + Vector3.forward * halfDepth,
                    0f, 1f);
            }

            if (i == points.Count - 2)
            {
                AddFace(vertices, uvs, triangles,
                    secondTop + Vector3.back * halfDepth,
                    secondTop + Vector3.forward * halfDepth,
                    secondBottom + Vector3.forward * halfDepth,
                    secondBottom + Vector3.back * halfDepth,
                    0f, 1f);
            }

            travelled = nextTravelled;
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh BuildPrism(IList<Vector3> points, float width, float height)
    {
        return BuildPlatform(points, width, height);
    }

    private static void AddFace(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float startU, float endU)
    {
        int index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        uvs.Add(new Vector2(startU, 0f));
        uvs.Add(new Vector2(startU, 1f));
        uvs.Add(new Vector2(endU, 1f));
        uvs.Add(new Vector2(endU, 0f));
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private static List<Vector3> PreparePoints(IList<Vector3> input, float tolerance, float snapAngle, float minimumLength)
    {
        List<Vector3> clean = new List<Vector3>();

        if (input == null)
            return clean;

        float minimumSquared = minimumLength * minimumLength;

        for (int i = 0; i < input.Count; i++)
        {
            if (clean.Count == 0 || (input[i] - clean[clean.Count - 1]).sqrMagnitude >= minimumSquared)
                clean.Add(input[i]);
        }

        clean = Simplify(clean, Mathf.Max(0f, tolerance));

        for (int i = 1; i < clean.Count; i++)
        {
            Vector3 delta = clean[i] - clean[i - 1];
            float angle = Mathf.Abs(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            angle = Mathf.Min(angle, Mathf.Abs(180f - angle));

            if (angle <= snapAngle)
            {
                Vector3 point = clean[i];
                point.y = clean[i - 1].y;
                clean[i] = point;
            }
        }

        return clean;
    }

    private static List<Vector3> Simplify(List<Vector3> points, float tolerance)
    {
        if (points.Count < 3 || tolerance <= 0f)
            return new List<Vector3>(points);

        bool[] keep = new bool[points.Count];
        keep[0] = true;
        keep[points.Count - 1] = true;
        SimplifySection(points, 0, points.Count - 1, tolerance * tolerance, keep);
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            if (keep[i])
                result.Add(points[i]);
        }

        return result;
    }

    private static void SimplifySection(List<Vector3> points, int first, int last, float toleranceSquared, bool[] keep)
    {
        if (last <= first + 1)
            return;

        float greatestDistance = 0f;
        int greatestIndex = -1;

        for (int i = first + 1; i < last; i++)
        {
            float distance = DistanceToSegmentSquared(points[i], points[first], points[last]);

            if (distance > greatestDistance)
            {
                greatestDistance = distance;
                greatestIndex = i;
            }
        }

        if (greatestIndex < 0 || greatestDistance <= toleranceSquared)
            return;

        keep[greatestIndex] = true;
        SimplifySection(points, first, greatestIndex, toleranceSquared, keep);
        SimplifySection(points, greatestIndex, last, toleranceSquared, keep);
    }

    private static float DistanceToSegmentSquared(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;

        if (lengthSquared < 0.000001f)
            return (point - start).sqrMagnitude;

        float value = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
        return (point - (start + segment * value)).sqrMagnitude;
    }
}
