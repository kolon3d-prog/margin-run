using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PencilToolUI : MonoBehaviour
{
    [SerializeField] private Button pencilButton;
    [SerializeField] private RectTransform pencilCursor;
    [SerializeField] private Canvas canvas;
    [SerializeField] private PaperDrawer paperdrawer;
    [SerializeField] private Vector2 cursorOffset = new Vector2(90f, -90f);

    private bool pencilEquipped;
    private void Start()
    {
        pencilButton.onClick.AddListener(TogglePencil);
        SetPencilEquipped(false);
    }

    private void Update()
    {
        if (!pencilEquipped||Mouse.current == null)
        return;
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePosition,
            null,
            out Vector2 localPosition
        );

        pencilCursor.anchoredPosition = localPosition + cursorOffset;
    }

    private void TogglePencil()
    {
        SetPencilEquipped(!pencilEquipped);
    }

    private void SetPencilEquipped(bool equipped)
    {
        pencilEquipped = equipped;
        
        pencilCursor.gameObject.SetActive(equipped);
        Cursor.visible = !equipped;

        if (paperdrawer != null)
            paperdrawer.SetDrawingEnabled(equipped);
    }
    private void OnDisable()
    {
        Cursor.visible = true;
    }
}
