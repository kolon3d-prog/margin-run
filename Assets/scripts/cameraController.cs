using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform FollowTarget, LookTarget;
    public float FollowSpeed = 10f;

    private void LateUpdate()
    {
        Vector3 targetPosition = FollowTarget.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, FollowSpeed * Time.deltaTime);

        transform.LookAt(LookTarget);
    }
}
