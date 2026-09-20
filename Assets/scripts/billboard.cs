using UnityEngine;

public class billboard : MonoBehaviour
{
    public Transform cameraTransform;
    public float speed = 10;
    
    Vector3 home;
    float phase;

    void Start()
    {
        home = transform.position;
        phase = Random.Range(0f, 20f);

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (cameraTransform == null)
            return;

        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, -90f);

        float x = Mathf.Sin(Time.time * speed * phase) * 1f;
        float y = Mathf.Sin(Time.time * speed * 0.4f + phase) * 0.4f;
        transform.position = home + new Vector3(x, y, 0f);
    }
}
