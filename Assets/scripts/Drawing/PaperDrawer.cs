using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PaperDrawer : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private RectTransform pencilTip;
    [SerializeField] private SpriteRenderer paperRenderer;
    [SerializeField] private float lineWidth = 0.18f;
    [SerializeField] private float minPointDistance = 0.05f;

    private readonly List<GameObject> undoHistory = new List<GameObject>();
    private readonly List<GameObject> redoHistory = new List<GameObject>();

    private LineRenderer currentLine;
    private Vector3 lastPoint;
    private bool drawingEnabled;

    public bool CanUndo => undoHistory.Count > 0;
    public bool CanRedo => redoHistory.Count > 0;

    private void Update()
    {
        CheckHistoryShortcuts();

        if (!drawingEnabled)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null || pencilTip == null||paperRenderer == null)
            return;

        Vector3 drawingPosition = GetPencilTipWorldPosition();
        bool pointerOverUI = IsPointerOverUI();
        bool pointerInsidePaper = IsInsidePaper(drawingPosition);
        bool canDrawHere = pointerInsidePaper && !pointerOverUI;

        if (mouse.leftButton.wasPressedThisFrame)
        {    
            if (canDrawHere)
                StartLine(drawingPosition);
        }

        if (mouse.leftButton.isPressed && currentLine != null)
        {
            if (canDrawHere)
                AddPoint(drawingPosition);
            else
                currentLine = null;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            currentLine = null;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
        
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsInsidePaper(Vector3 position)
    {
        Bounds paperBounds = paperRenderer.bounds;

        bool insideHorizontal = 
            position.x >= paperBounds.min.x &&
            position.x <= paperBounds.max.x;
        
        bool insideVertical = 
            position.y >= paperBounds.min.y &&
            position.y <= paperBounds.max.y;
        
        return insideHorizontal && insideVertical;
    }
    private void CheckHistoryShortcuts()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        bool ctrlPressed =
            keyboard.leftCtrlKey.isPressed ||
            keyboard.rightCtrlKey.isPressed;

        if (!ctrlPressed)
            return;

        bool shiftPressed =
            keyboard.leftShiftKey.isPressed ||
            keyboard.rightShiftKey.isPressed;

        if (keyboard.zKey.wasPressedThisFrame)
        {
            if (shiftPressed)
                RedoLastLine();
            else
                UndoLastLine();
        }
        else if (keyboard.yKey.wasPressedThisFrame)
        {
            RedoLastLine();
        }
    }

    public void ClearAllLines()
    {
        foreach (GameObject Line in undoHistory)
        {
            if (Line != null)
                Destroy(Line);
        }
        
        foreach (GameObject Line in redoHistory)
        {
            if (Line != null)
                Destroy(Line);
        }
        undoHistory.Clear();
        redoHistory.Clear();
        currentLine = null;
    }
    public void UndoLastLine()
    {
        if (!CanUndo)
            return;

        int lastIndex = undoHistory.Count - 1;
        GameObject lastLine = undoHistory[lastIndex];

        undoHistory.RemoveAt(lastIndex);
        redoHistory.Add(lastLine);

        if (currentLine != null &&
            currentLine.gameObject == lastLine)
        {
            currentLine = null;
        }

        lastLine.SetActive(false);
    }

    public void RedoLastLine()
    {
        if (!CanRedo)
            return;

        int lastIndex = redoHistory.Count - 1;
        GameObject restoredLine = redoHistory[lastIndex];

        redoHistory.RemoveAt(lastIndex);
        undoHistory.Add(restoredLine);

        restoredLine.SetActive(true);
    }

    private void StartLine(Vector3 position)
    {
        ClearRedoHistory();

        GameObject lineObject = new GameObject("PencilLine");
        lineObject.transform.SetParent(transform);

        undoHistory.Add(lineObject);

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

    private void ClearRedoHistory()
    {
        foreach (GameObject line in redoHistory)
        {
            if (line != null)
                Destroy(line);
        }

        redoHistory.Clear();
    }

    private Vector3 GetPencilTipWorldPosition()
    {
        Vector2 tipScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                pencilTip.position
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

    public void RemoveLineFromHistory(GameObject lineObject)
    {
        if (lineObject == null)
            return;

        undoHistory.Remove(lineObject);
        redoHistory.Remove(lineObject);

        if (currentLine != null &&
            currentLine.gameObject == lineObject)
        {
            currentLine = null;
        }
    }

    public void RegisterGeneratedLine(GameObject lineObject)
    {
        if (lineObject == null)
            return;

        ClearRedoHistory();
        undoHistory.Add(lineObject);        
    }

    public void SetDrawingEnabled(bool enabled)
    {
        drawingEnabled = enabled;

        if (!enabled)
            currentLine = null;
    }
}