using UnityEngine;

public class billboard : MonoBehaviour
{
    public Transform cameraTransform;
    public float speed = 10;
    private Vector3 movement;

    void Start()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        movement = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        if (movement.sqrMagnitude < 0.01f)
            movement = Vector3.right;
        movement.Normalize();
    }

    void Update()
    {
        if (cameraTransform == null)
            return;

        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        transform.position += movement * speed * Time.deltaTime;
    }
}
