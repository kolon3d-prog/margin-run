using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MouseSensitivity = 0.12f;
    public float MinPitch = -89f;
    public float MaxPitch = 89f;

    private float _pitch;

    public void Look(float mouseY)
    {
        _pitch -= mouseY * MouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
