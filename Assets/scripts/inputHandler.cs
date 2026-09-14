using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public PlayerController CharacterController;
    public CameraController Camera;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 look = _lookAction.ReadValue<Vector2>();
        CharacterController.Rotate(look);
        Camera.Look(look.y);
        CharacterController.Move(_moveAction.ReadValue<Vector2>(), _jumpAction.IsPressed());
    }
}
