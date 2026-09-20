using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class runExtrude : MonoBehaviour
{
  [Header("sketch scene")]
  public PaperDrawer paperDrawer;

  [Header("3d")]
  public Transform paperOrigin;
  public float height = 1.2f;
  public float width = 0.35f;
  public Material wallMaterial;
  public Transform spawnParent;

  readonly List<GameObject> spawned = new List<GameObject>();

  public void ExtrudeAllLines()
  {
    ClearSpawned();

    if (paperDrawer == null)
      paperDrawer = FindFirstObjectByType<PaperDrawer>();
      Debug.Log(paperDrawer.ActiveLines.Count);

    if (paperDrawer == null)
      return;
    
    foreach (GameObject lineGo in paperDrawer.ActiveLines)
    {
      if (lineGo == null || !lineGo.activeInHierarchy)
        continue;
      
      LineRenderer lr = lineGo.GetComponent<LineRenderer>();
      if (lr == null || lr.positionCount < 2)
        continue;

      List<Vector3> worldPts = new List<Vector3>(lr.positionCount);
      for (int i = 0; i < lr.positionCount; i++)
      {
        Vector3 p = lr.GetPosition(i);
        worldPts.Add(p);
      }

      Mesh mesh = LineExtruder.BuildPrism(worldPts, width, height);
      GameObject go = new GameObject("Extruded_" + lineGo.name);
      if (spawnParent != null)
        go.transform.SetParent(spawnParent, true);
      
      MeshFilter mf = go.AddComponent<MeshFilter>();
      mf.sharedMesh = mesh;

      MeshRenderer mr = go.AddComponent<MeshRenderer>();
      mr.sharedMaterial = wallMaterial;

      MeshCollider mc = go.AddComponent<MeshCollider>();
      mc.sharedMesh = mesh;

      spawned.Add(go);
    }
  }

  public void ClearSpawned()
  {
    for (int i = 0; i < spawned.Count; i++)
    {
      if (spawned[i] != null)
        Destroy(spawned[i]);
    }
    spawned.Clear();
  }

  Vector3 PaperToWorld(Vector3 paperPoint)
  {
    Vector3 local = new Vector3(paperPoint.x, 0f, paperPoint.y);
    if (paperOrigin != null)
      return paperOrigin.TransformPoint(local);
    return local;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.E))
      ExtrudeAllLines();
  }
}