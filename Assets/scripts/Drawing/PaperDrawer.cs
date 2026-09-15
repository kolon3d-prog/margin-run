using UnityEngine;
using UnityEngine.InputSystem;

public class PaperDrawer : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.18f;
    [SerializeField] private float minPointDistance = 0.05f;

    private LineRenderer currentLine;
    private Vector3 lastPoint;

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector3 mousePosition = GetMouseWorldPosition();

        if (mouse.leftButton.wasPressedThisFrame)
            StartLine(mousePosition);

        if (mouse.leftButton.isPressed && currentLine != null)
            AddPoint(mousePosition);

        if (mouse.leftButton.wasReleasedThisFrame)
            currentLine = null;
    }

    private void StartLine(Vector3 position)
    {
        GameObject lineObject = new GameObject("PencilLine");
        lineObject.transform.SetParent(transform);

        currentLine = lineObject.AddComponent<LineRenderer>();

        currentLine.material = lineMaterial;
        currentLine.useWorldSpace = true;
        currentLine.textureMode = LineTextureMode.Tile;

        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;

        currentLine.numCapVertices = 6;
        currentLine.numCornerVertices = 6;
        currentLine.sortingOrder = 0;

        currentLine.positionCount = 2;
        currentLine.SetPosition(0, position);
        currentLine.SetPosition(1, position);

        lastPoint = position;
    }

    private void AddPoint(Vector3 position)
    {
        if (Vector3.Distance(lastPoint, position) < minPointDistance)
            return;

        currentLine.positionCount++;
        currentLine.SetPosition(
            currentLine.positionCount - 1,
            position
        );

        lastPoint = position;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 screenPosition = new Vector3(
            mouseScreenPosition.x,
            mouseScreenPosition.y,
            Mathf.Abs(drawingCamera.transform.position.z)
        );

        Vector3 worldPosition =
            drawingCamera.ScreenToWorldPoint(screenPosition);

        worldPosition.z = -0.1f;

        return worldPosition;
    }
}