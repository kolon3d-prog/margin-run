using UnityEngine;

public class billboard : MonoBehaviour
{
    public Transform cameraTransform;
    public float speed = 10f;

    [SerializeField] private Vector3 movement;

    void Start()
    {
        if (cameraTransform == null)
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = FindFirstObjectByType<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }

        movement = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        if (movement.sqrMagnitude < 0.01f)
            movement = Vector3.right;
        movement.Normalize();
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // Face camera, keep upright (no pitch/roll).
        Vector3 toCam = cameraTransform.position - transform.position;
        toCam.y = 0f;
        if (toCam.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);

        transform.position += movement * speed * Time.deltaTime;
    }
}
