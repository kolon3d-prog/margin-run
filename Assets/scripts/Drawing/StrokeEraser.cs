using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StrokeEraser : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private RectTransform eraserTip;
    [SerializeField] private Transform linesParent;
    [SerializeField] private SpriteRenderer paperRenderer;
    [SerializeField] private PaperDrawer paperDrawer;

    [SerializeField] private float eraserRadius = 0.35f;
    [SerializeField] private float sampleSpacing = 0.04f;

    private readonly List<GameObject> eraseBuffer =
        new List<GameObject>();

    private bool erasingEnabled;
    private bool eraseDragStarted;

    private void Update()
    {
        if (!erasingEnabled || Mouse.current == null)
            return;

        if (eraserTip == null ||
            drawingCamera == null ||
            linesParent == null ||
            paperRenderer == null ||
            paperDrawer == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        Vector3 eraserPosition = GetEraserWorldPosition();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            eraseDragStarted =
                IsInsidePaper(eraserPosition) &&
                !IsPointerOverUI();

            if (eraseDragStarted)
                paperDrawer.BeginAction();
        }

        if (mouse.leftButton.isPressed &&
            eraseDragStarted &&
            IsInsidePaper(eraserPosition))
        {
            EraseAt(eraserPosition);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            FinishEraseDrag();
    }

    public void SetErasingEnabled(bool enabled)
    {
        erasingEnabled = enabled;

        if (!enabled)
            FinishEraseDrag();
    }

    private void FinishEraseDrag()
    {
        if (!eraseDragStarted)
            return;

        eraseDragStarted = false;

        if (paperDrawer != null)
            paperDrawer.CommitAction();
    }

    private void EraseAt(Vector3 eraserPosition)
    {
        eraseBuffer.Clear();
        eraseBuffer.AddRange(paperDrawer.ActiveLines);

        foreach (GameObject lineObject in eraseBuffer)
        {
            if (lineObject == null || !lineObject.activeSelf)
                continue;

            LineRenderer line =
                lineObject.GetComponent<LineRenderer>();

            if (line != null)
                TryEraseLine(line, eraserPosition);
        }
    }

    private void TryEraseLine(
        LineRenderer originalLine,
        Vector3 eraserPosition
    )
    {
        if (originalLine == null ||
            originalLine.positionCount < 2)
        {
            return;
        }

        Vector3[] originalPoints =
            new Vector3[originalLine.positionCount];

        originalLine.GetPositions(originalPoints);

        List<Vector3> sampledPoints =
            CreateSampledPoints(originalPoints);

        List<List<Vector3>> remainingSegments =
            SplitPointsAroundEraser(
                sampledPoints,
                eraserPosition,
                out bool erasedAnything
            );

        if (!erasedAnything)
            return;

        paperDrawer.NotifyLineRemoved(originalLine.gameObject);

        foreach (List<Vector3> segment in remainingSegments)
        {
            if (segment.Count < 2)
                continue;

            LineRenderer newLine =
                CreateLineSegment(originalLine, segment);

            paperDrawer.NotifyLineCreated(newLine.gameObject);
        }
    }

    private List<Vector3> CreateSampledPoints(
        Vector3[] originalPoints
    )
    {
        List<Vector3> result = new List<Vector3>();

        result.Add(originalPoints[0]);

        for (int i = 0; i < originalPoints.Length - 1; i++)
        {
            Vector3 start = originalPoints[i];
            Vector3 end = originalPoints[i + 1];

            float distance = Vector3.Distance(start, end);

            int steps = Mathf.Max(
                1,
                Mathf.CeilToInt(distance / sampleSpacing)
            );

            for (int step = 1; step <= steps; step++)
            {
                float progress = step / (float)steps;

                result.Add(
                    Vector3.Lerp(start, end, progress)
                );
            }
        }

        return result;
    }

    private List<List<Vector3>> SplitPointsAroundEraser(
        List<Vector3> points,
        Vector3 eraserPosition,
        out bool erasedAnything
    )
    {
        List<List<Vector3>> segments =
            new List<List<Vector3>>();

        List<Vector3> currentSegment =
            new List<Vector3>();

        erasedAnything = false;
        float radiusSquared = eraserRadius * eraserRadius;

        foreach (Vector3 point in points)
        {
            Vector2 difference = new Vector2(
                point.x - eraserPosition.x,
                point.y - eraserPosition.y
            );

            bool pointErased =
                difference.sqrMagnitude <= radiusSquared;

            if (pointErased)
            {
                erasedAnything = true;

                AddSegmentIfValid(
                    segments,
                    currentSegment
                );

                currentSegment = new List<Vector3>();
            }
            else
            {
                currentSegment.Add(point);
            }
        }

        AddSegmentIfValid(segments, currentSegment);

        return segments;
    }

    private void AddSegmentIfValid(
        List<List<Vector3>> segments,
        List<Vector3> segment
    )
    {
        if (segment.Count >= 2)
            segments.Add(segment);
    }

    private LineRenderer CreateLineSegment(
        LineRenderer source,
        List<Vector3> points
    )
    {
        GameObject lineObject =
            new GameObject("PencilLine");

        lineObject.transform.SetParent(linesParent);

        LineRenderer newLine =
            lineObject.AddComponent<LineRenderer>();

        newLine.sharedMaterial = source.sharedMaterial;
        newLine.useWorldSpace = true;
        newLine.textureMode = source.textureMode;

        newLine.startWidth = source.startWidth;
        newLine.endWidth = source.endWidth;

        newLine.numCapVertices = source.numCapVertices;
        newLine.numCornerVertices = source.numCornerVertices;

        newLine.sortingLayerID = source.sortingLayerID;
        newLine.sortingOrder = source.sortingOrder;

        newLine.positionCount = points.Count;
        newLine.SetPositions(points.ToArray());

        return newLine;
    }

    private Vector3 GetEraserWorldPosition()
    {
        Vector2 tipScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                eraserTip.position
            );

        Vector3 screenPosition = new Vector3(
            tipScreenPosition.x,
            tipScreenPosition.y,
            Mathf.Abs(drawingCamera.transform.position.z)
        );

        Vector3 worldPosition =
            drawingCamera.ScreenToWorldPoint(screenPosition);

        worldPosition.z = -0.1f;

        return worldPosition;
    }

    private bool IsInsidePaper(Vector3 position)
    {
        Bounds bounds = paperRenderer.bounds;

        return
            position.x >= bounds.min.x &&
            position.x <= bounds.max.x &&
            position.y >= bounds.min.y &&
            position.y <= bounds.max.y;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}