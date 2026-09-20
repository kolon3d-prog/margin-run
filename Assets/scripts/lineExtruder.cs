using System.Collections.Generic;
using UnityEngine;

public static class LineExtruder
{
  public static Mesh BuildPrism(IList<Vector3> points, float width, float height)
  {
    Mesh mesh = new Mesh();
    mesh.name = "ExtrudedLine";

    if (points == null || points.Count < 2)
      return mesh;
    
    float halfW = width * 0.5f;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();

    for (int i = 0; i < points.Count - 1; i++)
    {
      Vector3 a = points[i];
      Vector3 b = points[i + 1];
      Vector3 dir = b - a;
      dir.y = 0f;
      if (dir.sqrMagnitude < 0.0001f)
        continue;
      dir.Normalize();
      
      Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
      if (right.sqrMagnitude < 0.0001f)
        right = Vector3.right;
      Vector3 aL = a - right * halfW;
      Vector3 aR = a + right * halfW;
      Vector3 bL = b - right * halfW;
      Vector3 bR = b + right * halfW;

      int v = verts.Count;

      verts.Add(aL);
      verts.Add(aR);
      verts.Add(bR);
      verts.Add(bL);

      verts.Add(aL + Vector3.up * height);
      verts.Add(aR + Vector3.up * height);
      verts.Add(bR + Vector3.up * height);
      verts.Add(bL + Vector3.up * height);

      AddQuad(tris, v + 0, v + 1, v + 2, v + 3);
      AddQuad(tris, v + 4, v + 7, v + 6, v + 5);
      AddQuad(tris, v + 0, v + 3, v + 7, v + 4);
      AddQuad(tris, v + 1, v + 5, v + 6, v + 2);
      AddQuad(tris, v + 0, v + 4, v + 5, v + 1);
      AddQuad(tris, v + 3, v + 2, v + 6, v + 7);
    }

    mesh.SetVertices(verts);
    mesh.SetTriangles(tris, 0);
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    return mesh;
  }

  static void AddQuad(List<int> tris, int a, int b, int c, int d)
  {
    tris.Add(a);
    tris.Add(b);
    tris.Add(c);
    tris.Add(a);
    tris.Add(c);
    tris.Add(d);
  }
}