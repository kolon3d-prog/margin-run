using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;
    public float MaxSpeed = 10f;
    public float MouseSensitivity = 0.12f;
    public float JumpForce = 10f;
    public float Gravity = -30f;

    private float _rotationY;
    private float _verticalVelocity;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 movementVector)
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += Gravity * Time.deltaTime;

        Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        move = move * MaxSpeed + Vector3.up * _verticalVelocity;
        _characterController.Move(move * Time.deltaTime);
    }

    public void Rotate(Vector2 rotationVector)
    {
        _rotationY += rotationVector.x * MouseSensitivity;
        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
    }

    public void Jump()
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = JumpForce;
        }
    }
}
