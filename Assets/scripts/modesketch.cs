using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class modesketch : MonoBehaviour
{
    public GameObject drawingRoot;
    public GameObject player;
    public GameObject charSprite;
    public runExtrude extrude;
    public Transform sketchView;
    public float camTime = 1.2f;
    GameObject ui;
    Camera cam;
    bool started;
    bool flying;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Bind()
    {
        if (drawingRoot == null) drawingRoot = GameObject.Find("DrawingRoot");
        if (player == null) player = GameObject.Find("Player");
        if (extrude == null) extrude = FindFirstObjectByType<runExtrude>();
        if (ui == null) ui = GameObject.Find("Canvas");
        if (cam != null) return;
        var scene = SceneManager.GetSceneByName("Drawing");
        if (!scene.IsValid()) return;
        foreach (var root in scene.GetRootGameObjects())
            if (root.CompareTag("MainCamera"))
                cam = root.GetComponent<Camera>();
    }

    void ShowDraw(bool on)
    {
        Bind();
        if (drawingRoot != null) drawingRoot.SetActive(on);
        if (ui != null) ui.SetActive(on);
        if (charSprite != null) charSprite.SetActive(on);
        if (cam != null) cam.enabled = on;

        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = on;
    }

    public void GoSketch()
    {
        ShowDraw(true);
        if (player != null) player.SetActive(false);
    }

    public void GoRun()
    {
        if (flying) return;
        Bind();
        if (extrude != null) extrude.ExtrudeAllLines();
        ShowDraw(false);
        if (player != null) player.SetActive(true);
        if (sketchView != null) StartCoroutine(FlyToPlayer());
    }

    IEnumerator FlyToPlayer()
    {
        flying = true;
        PlayerMovement mv = player.GetComponent<PlayerMovement>();
        Transform camT = mv.playerCamera.transform;
        Transform head = camT.parent;
        Vector3 endPos = camT.localPosition;
        Quaternion endRot = camT.localRotation;

        mv.enabled = false;
        camT.SetParent(null);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / camTime;
            float k = Mathf.SmoothStep(0f, 1f, t);
            camT.position = Vector3.Lerp(sketchView.position, head.TransformPoint(endPos), k);
            camT.rotation = Quaternion.Slerp(sketchView.rotation, head.rotation * endRot, k);
            mv.playerCamera.fieldOfView = Mathf.Lerp(20f, 60f, k);
            yield return null;
        }
        camT.SetParent(head);
        camT.localPosition = endPos;
        camT.localRotation = endRot;
        mv.playerCamera.fieldOfView = 60f;
        mv.enabled = true;
        flying = false;
    }

    void Update()
    {
        if (!started && SceneManager.GetSceneByName("Drawing").isLoaded)
        {
            started = true;
            GoSketch();
        }
        if (Input.GetKeyDown(KeyCode.R)) GoRun();
        if (Input.GetKeyDown(KeyCode.T)) GoSketch();
    }
}
