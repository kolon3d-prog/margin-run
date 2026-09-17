using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        pencilCursor.position = 
            mousePosition + cursorOffset;

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

        Cursor.lockState = equipped
            ? CursorLockMode.Confined
            : CursorLockMode.None;

        if (paperdrawer != null)
            paperdrawer.SetDrawingEnabled(equipped);
    }
    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
