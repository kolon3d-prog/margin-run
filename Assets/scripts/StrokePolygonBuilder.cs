using System;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

public static class StrokePolygonBuilder
{
    public static List<List<Vector2>> Build(
        IList<Vector3> worldPoints,
        float strokeRadius,
        float simplifyTolerance,
        double integerScale)
    {
        List<Vector2> points = PreparePoints(worldPoints, simplifyTolerance);
        List<List<Vector2>> result = new List<List<Vector2>>();

        if (points.Count < 2)
            return result;

        double scale = Math.Max(100.0, integerScale);
        Path64 centerLine = new Path64(points.Count);

        foreach (Vector2 point in points)
        {
            centerLine.Add(new Point64(
                (long)Math.Round(point.x * scale),
                (long)Math.Round(point.y * scale)
            ));
        }

        Paths64 source = new Paths64 { centerLine };
        Paths64 expanded = Clipper.InflatePaths(
            source,
            Math.Max(0.001f, strokeRadius) * scale,
            JoinType.Round,
            EndType.Round
        );

        Paths64 united = Clipper.Union(
            expanded,
            FillRule.NonZero
        );

        foreach (Path64 path in united)
        {
            if (path.Count < 3)
                continue;

            List<Vector2> contour = new List<Vector2>(path.Count);

            foreach (Point64 point in path)
            {
                contour.Add(new Vector2(
                    (float)(point.X / scale),
                    (float)(point.Y / scale)
                ));
            }

            RemoveDuplicateEnd(contour);

            if (contour.Count >= 3)
                result.Add(contour);
        }

        return result;
    }

    private static List<Vector2> PreparePoints(
        IList<Vector3> input,
        float tolerance)
    {
        List<Vector2> points = new List<Vector2>();

        if (input == null)
            return points;

        const float minimumDistanceSquared = 0.000025f;

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 point = new Vector2(input[i].x, input[i].y);

            if (points.Count == 0 ||
                (point - points[points.Count - 1]).sqrMagnitude >= minimumDistanceSquared)
                                        {
                points.Add(point);
                                        }
        }

        return Simplify(points, Mathf.Max(0f, tolerance));
    }

    private static List<Vector2> Simplify(
        List<Vector2> points,
        float tolerance)
    {
        if (points.Count < 3 || tolerance <= 0f)
            return new List<Vector2>(points);

        bool[] keep = new bool[points.Count];
        keep[0] = true;
        keep[points.Count - 1] = true;

        SimplifySection(
            points,
            0,
            points.Count - 1,
            tolerance * tolerance,
            keep
        );

        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            if (keep[i])
                result.Add(points[i]);
        }

        return result;
    }

    private static void SimplifySection(
        List<Vector2> points,
        int first,
        int last,
        float toleranceSquared,
        bool[] keep)
    {
        if (last <= first + 1)
            return;

        float greatestDistance = 0f;
        int greatestIndex = -1;

        for (int i = first + 1; i < last; i++)
        {
            float distance = DistanceToSegmentSquared(
                points[i],
                points[first],
                points[last]
            );

            if (distance > greatestDistance)
            {
                greatestDistance = distance;
                greatestIndex = i;
            }
        }

        if (greatestIndex < 0 || greatestDistance <= toleranceSquared)
            return;

        keep[greatestIndex] = true;

        SimplifySection(
            points,
            first,
            greatestIndex,
            toleranceSquared,
            keep
        );

        SimplifySection(
            points,
            greatestIndex,
            last,
            toleranceSquared,
            keep
        );
    }

    private static float DistanceToSegmentSquared(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;

        if (lengthSquared < 0.000001f)
            return (point - start).sqrMagnitude;

        float value = Mathf.Clamp01(
            Vector2.Dot(point - start, segment) / lengthSquared
        );

        Vector2 closest = start + segment * value;
        return (point - closest).sqrMagnitude;
    }

    private static void RemoveDuplicateEnd(List<Vector2> contour)
    {
        if (contour.Count < 2)
            return;

        if ((contour[0] - contour[contour.Count - 1]).sqrMagnitude < 0.000001f)
            contour.RemoveAt(contour.Count - 1);
    }
}