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

    [Header("Drawing blockers")]
    [SerializeField] private LayerMask drawingBlockerMask;

    [Tooltip("   . 0   .")]
    [SerializeField] private int maxHistorySteps = 100;

    private readonly List<DrawingAction> undoHistory =
        new List<DrawingAction>();

    private readonly List<DrawingAction> redoHistory =
        new List<DrawingAction>();

    private readonly HashSet<GameObject> activeLines =
        new HashSet<GameObject>();

    private DrawingAction pendingAction;

    private LineRenderer currentLine;
    private Vector3 lastPoint;
    private bool drawingEnabled;

    public bool CanUndo => undoHistory.Count > 0;
    public bool CanRedo => redoHistory.Count > 0;
    public bool CanClear => activeLines.Count > 0;

    public IReadOnlyCollection<GameObject> ActiveLines => activeLines;

    private void Update()
    {
        CheckHistoryShortcuts();

        if (!drawingEnabled)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null || pencilTip == null || paperRenderer == null)
            return;

        Vector2 tipScreenPosition = GetPencilTipScreenPosition();
        Vector3 drawingPosition = GetPencilTipWorldPosition(tipScreenPosition);
        bool pointerOverUI = IsPointerOverUI();
        bool pointerInsidePaper = IsInsidePaper(drawingPosition);
        bool pointerOverPlatform = IsPointerOverDrawingBlocker(tipScreenPosition);
        bool canDrawHere = pointerInsidePaper && !pointerOverUI && !pointerOverPlatform;

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
                FinishCurrentLine();
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            FinishCurrentLine();
        }
    }

    public void BeginAction()
    {
        if (pendingAction != null)
            CommitAction();

        pendingAction = new DrawingAction();
    }

    public void CommitAction()
    {
        if (pendingAction == null)
            return;

        DrawingAction action = pendingAction;
        pendingAction = null;

        CoalesceAction(action);

        if (!action.HasChanges)
            return;

        ClearRedoHistory();
        undoHistory.Add(action);

        TrimHistory();
    }

    public void NotifyLineCreated(GameObject lineObject)
    {
        if (lineObject == null)
            return;

        if (pendingAction == null)
            BeginAction();

        activeLines.Add(lineObject);
        pendingAction.Added.Add(lineObject);
    }

    public void NotifyLineRemoved(GameObject lineObject)
    {
        if (lineObject == null)
            return;

        if (pendingAction == null)
            BeginAction();

        if (currentLine != null &&
            currentLine.gameObject == lineObject)
                            {
            currentLine = null;
                            }

        activeLines.Remove(lineObject);
        pendingAction.Removed.Add(lineObject);

        lineObject.SetActive(false);
    }

    private void CoalesceAction(DrawingAction action)
    {
        for (int i = action.Added.Count - 1; i >= 0; i--)
        {
            GameObject lineObject = action.Added[i];

            if (lineObject == null)
            {
                action.Added.RemoveAt(i);
                continue;
            }

            if (action.Removed.Remove(lineObject))
            {
                action.Added.RemoveAt(i);
                Destroy(lineObject);
            }
        }

        for (int i = action.Removed.Count - 1; i >= 0; i--)
        {
            if (action.Removed[i] == null)
                action.Removed.RemoveAt(i);
        }
    }

    public void UndoLastLine()
    {
        CommitAction();

        if (!CanUndo)
            return;

        int lastIndex = undoHistory.Count - 1;
        DrawingAction action = undoHistory[lastIndex];
        undoHistory.RemoveAt(lastIndex);

        foreach (GameObject lineObject in action.Added)
            SetLineVisible(lineObject, false);

        foreach (GameObject lineObject in action.Removed)
            SetLineVisible(lineObject, true);

        redoHistory.Add(action);
    }

    public void RedoLastLine()
    {
        CommitAction();

        if (!CanRedo)
            return;

        int lastIndex = redoHistory.Count - 1;
        DrawingAction action = redoHistory[lastIndex];
        redoHistory.RemoveAt(lastIndex);

        foreach (GameObject lineObject in action.Removed)
            SetLineVisible(lineObject, false);

        foreach (GameObject lineObject in action.Added)
            SetLineVisible(lineObject, true);

        undoHistory.Add(action);
    }

    private void SetLineVisible(GameObject lineObject, bool visible)
    {
        if (lineObject == null)
            return;

        if (!visible &&
            currentLine != null &&
                                currentLine.gameObject == lineObject)
        {
            currentLine = null;
        }

        lineObject.SetActive(visible);

        if (visible)
            activeLines.Add(lineObject);
        else
            activeLines.Remove(lineObject);
    }

    public void ClearAllLines()
    {
        currentLine = null;
        CommitAction();

        if (!CanClear)
            return;

        BeginAction();

        List<GameObject> linesToRemove =
            new List<GameObject>(activeLines);

        foreach (GameObject lineObject in linesToRemove)
            NotifyLineRemoved(lineObject);

        CommitAction();
    }

    private void ClearRedoHistory()
    {
        foreach (DrawingAction action in redoHistory)
        {
            foreach (GameObject lineObject in action.Added)
            {
                if (lineObject != null)
                    Destroy(lineObject);
            }
        }

        redoHistory.Clear();
    }

    private void TrimHistory()
    {
        if (maxHistorySteps <= 0)
            return;

        while (undoHistory.Count > maxHistorySteps)
        {
            DrawingAction oldest = undoHistory[0];
            undoHistory.RemoveAt(0);

            foreach (GameObject lineObject in oldest.Removed)
            {
                if (lineObject != null)
                    Destroy(lineObject);
            }
        }
    }

    private void FinishCurrentLine()
    {
        if (currentLine == null)
            return;

        currentLine = null;
        CommitAction();
    }

    private void StartLine(Vector3 position)
    {
        BeginAction();

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

        NotifyLineCreated(lineObject);
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

    public void SetDrawingEnabled(bool enabled)
    {
        drawingEnabled = enabled;

        if (!enabled)
        {
            currentLine = null;
            CommitAction();
        }
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

    private Vector2 GetPencilTipScreenPosition()
    {
        return RectTransformUtility.WorldToScreenPoint(null, pencilTip.position);
    }

    private bool IsPointerOverDrawingBlocker(Vector2 screenPosition)
    {
        Ray ray = drawingCamera.ScreenPointToRay(screenPosition);
        bool hit3D = Physics.Raycast(ray, Mathf.Infinity, drawingBlockerMask, QueryTriggerInteraction.Collide);

        if (hit3D)
            return true;

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, drawingBlockerMask);
        return hit2D.collider != null;
    }

    private Vector3 GetPencilTipWorldPosition(Vector2 tipScreenPosition)
    {
        Vector3 screenPosition = new Vector3(
            tipScreenPosition.x,
            tipScreenPosition.y,
            Mathf.Abs(drawingCamera.transform.position.z)
        );

        Vector3 worldPosition = drawingCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = -0.1f;
        return worldPosition;
    }

}