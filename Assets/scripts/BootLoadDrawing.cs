using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoadDrawing : MonoBehaviour
{
    void Start()
    {
        if (!SceneManager.GetSceneByName("Drawing").isLoaded)
            SceneManager.LoadScene("Drawing", LoadSceneMode.Additive);
    }
}