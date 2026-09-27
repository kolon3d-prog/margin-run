using UnityEngine;
using UnityEngine.SceneManagement;

public class modesketch : MonoBehaviour
{
    public GameObject drawingRoot;
    public GameObject player;
    public runExtrude extrude;
    GameObject ui;
    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Bind()
    {
        if (drawingRoot == null) drawingRoot = GameObject.Find("Player");
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
        if (cam != null) cam.enabled = on;
        if (on) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoSketch()
    {
        ShowDraw(true);
        if (player != null) player.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Bind();
            if (extrude != null) extrude.ExtrudeAllLines();
            ShowDraw(false);
            if (player != null) player.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.T)) GoSketch();
    }
}
