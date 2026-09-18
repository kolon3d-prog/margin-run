using UnityEngine;
using UnityEngine.InputSystem;
using UIButton = UnityEngine.UI.Button;

public enum DrawingTool
{
    None,
    Pencil,
    Eraser
}

public class ToolManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private UIButton pencilButton;
    [SerializeField] private UIButton eraserButton;

    [Header("Cursors")]
    [SerializeField] private RectTransform pencilCursor;
    [SerializeField] private RectTransform eraserCursor;

    [Header("Drawing")]
    [SerializeField] private PaperDrawer paperDrawer;

    [Header("Cursor offsets")]
    [SerializeField] private Vector2 pencilOffset =
        new Vector2(90f, -90f);

    [SerializeField] private Vector2 eraserOffset =
        new Vector2(40f, -40f);

    private DrawingTool activeTool = DrawingTool.None;

    public DrawingTool ActiveTool => activeTool;

    private void Start()
    {
        pencilButton.onClick.AddListener(TogglePencil);
        eraserButton.onClick.AddListener(ToggleEraser);

        SelectTool(DrawingTool.None);
    }

    private void Update()
    {
        CheckKeyboardShortcuts();
        UpdateToolCursor();
    }

    private void CheckKeyboardShortcuts()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            TogglePencil();

        if (keyboard.digit2Key.wasPressedThisFrame)
            ToggleEraser();
    }

    private void TogglePencil()
    {
        if (activeTool == DrawingTool.Pencil)
            SelectTool(DrawingTool.None);
        else
            SelectTool(DrawingTool.Pencil);
    }

    private void ToggleEraser()
    {
        if (activeTool == DrawingTool.Eraser)
            SelectTool(DrawingTool.None);
        else
            SelectTool(DrawingTool.Eraser);
    }

    private void SelectTool(DrawingTool newTool)
    {
        activeTool = newTool;

        pencilCursor.gameObject.SetActive(
            activeTool == DrawingTool.Pencil
        );

        eraserCursor.gameObject.SetActive(
            activeTool == DrawingTool.Eraser
        );

        paperDrawer.SetDrawingEnabled(
            activeTool == DrawingTool.Pencil
        );

        bool toolSelected = activeTool != DrawingTool.None;

        Cursor.visible = !toolSelected;

        Cursor.lockState = toolSelected
            ? CursorLockMode.Confined
            : CursorLockMode.None;
    }

    private void UpdateToolCursor()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if (activeTool == DrawingTool.Pencil)
        {
            pencilCursor.position =
                mousePosition + pencilOffset;
        }

        if (activeTool == DrawingTool.Eraser)
        {
            eraserCursor.position =
                mousePosition + eraserOffset;
        }
    }

    private void OnDestroy()
    {
        if (pencilButton != null)
            pencilButton.onClick.RemoveListener(TogglePencil);

        if (eraserButton != null)
            eraserButton.onClick.RemoveListener(ToggleEraser);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}