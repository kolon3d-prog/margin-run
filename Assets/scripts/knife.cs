using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class knife : MonoBehaviour
{
  public Image img;
  public Sprite[] frames;
  public float fps = 14f;
  bool playing;

  void OnEnable()
  {
    playing = false;
    img.sprite = frames[0];
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.F) && !playing)
      StartCoroutine(Inspect());
  }

  IEnumerator Inspect()
  {
    playing = true;
    for (int i = 0; i < frames.Length; i++)
    {
      img.sprite = frames[i];
      yield return new WaitForSeconds(1f / fps);
    }
    img.sprite = frames[0];
    playing = false;
  }
}