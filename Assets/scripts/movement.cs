using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float jumpPower = 5f;
    public float gravity = 15f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float groundAccel = 1f;
    public float airAccel = 2f;
    public float friction = 5f;
    

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        float wishspeed = walkSpeed;

        Vector3 wishdir = Vector3.zero;
        if (canMove)
        {
            wishdir = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

            if (wishdir.sqrMagnitude > 0.001f)
                wishdir.Normalize();
            else
                wishdir = Vector3.zero;
        }

        Debug.DrawRay(transform.position, wishdir * 2f, Color.green);

        if (characterController.isGrounded)
        {
            float f = Mathf.Max(0f, 1f - friction * Time.deltaTime);
            moveDirection.x *= f;
            moveDirection.z *= f;
            Accelerate(wishdir, wishspeed, groundAccel);
        }
        else
        {
            float airWish = walkSpeed*0.3f;
            Accelerate(wishdir, airWish, airAccel);
        }

        float movementDirectionY = moveDirection.y;

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;

        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // death below

        if (transform.position.y < -20f){
            transform.position = new Vector3(0f, 2f, 0f); 
            moveDirection = Vector3.zero;
            var mode = FindObjectOfType<modesketch>();
            if (mode != null)
                mode.GoSketch();
        }
    }
    void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        if (wishdir.sqrMagnitude < 0.001f)
            return;

        float current = Vector3.Dot(moveDirection, wishdir);
        float add = wishspeed - current;
        if (add <= 0f)
            return;

        float accelSpeed = accel * wishspeed * Time.deltaTime;
        if (accelSpeed > add)
            accelSpeed = add;

        moveDirection.x += wishdir.x * accelSpeed;
        moveDirection.z += wishdir.z * accelSpeed;
    }

}