using UnityEngine;

public class modesketch : MonoBehaviour
{
    public GameObject drawingRoot;
    public GameObject player;
    public runExtrude extrude;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (drawingRoot == null)
            drawingRoot = GameObject.Find("DrawingRoot");
        if (player == null)
            player = GameObject.Find("Player");
        if (extrude == null)
            extrude = FindFirstObjectByType<runExtrude>();
    }

    public void GoSketch()
    {
        if (drawingRoot == null)
            drawingRoot = GameObject.Find("DrawingRoot");
        if (player == null)
            player = GameObject.Find("Player");

        if (drawingRoot != null)
            drawingRoot.SetActive(true);
        if (player != null)
            player.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)){
            if (extrude != null)
                extrude.ExtrudeAllLines();
            if (drawingRoot != null) drawingRoot.SetActive(false);
            if (player == null) player = GameObject.Find("Player");
            if (player != null) player.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.T)){
            GoSketch();
        }
    }
}
