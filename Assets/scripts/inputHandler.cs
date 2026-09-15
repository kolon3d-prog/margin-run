using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public enum GameMode { Sketch, Run }

    public PlayerController CharacterController;
    public CameraController Camera;
    public GameMode Mode = GameMode.Sketch;

    private InputAction _moveAction, _lookAction, _jumpAction;

    void Start()
    {
        if (CharacterController == null)
            CharacterController = GetComponent<PlayerController>();
        if (Camera == null)
            Camera = GetComponentInChildren<CameraController>();

        var actions = InputSystem.actions;
        _moveAction = actions.FindAction("Move");
        _lookAction = actions.FindAction("Look");
        _jumpAction = actions.FindAction("Jump");

        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();

        ApplyCursor();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Mode = Mode == GameMode.Sketch ? GameMode.Run : GameMode.Sketch;
            ApplyCursor();
        }

        if (Mode == GameMode.Sketch)
            return;

        Vector2 look = _lookAction.ReadValue<Vector2>();
        CharacterController.Rotate(look);
        Camera.Look(look.y);
        CharacterController.Move(_moveAction.ReadValue<Vector2>(), _jumpAction.IsPressed());
    }

    void ApplyCursor()
    {
        bool sketch = Mode == GameMode.Sketch;
        Cursor.lockState = sketch ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = sketch;
    }
}
