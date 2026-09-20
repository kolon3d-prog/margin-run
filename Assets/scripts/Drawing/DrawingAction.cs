using System.Collections.Generic;
using UnityEngine;

public class DrawingAction
{
    public readonly List<GameObject> Added = new List<GameObject>();
    public readonly List<GameObject> Removed = new List<GameObject>();

    public bool HasChanges => Added.Count > 0 || Removed.Count > 0;
}