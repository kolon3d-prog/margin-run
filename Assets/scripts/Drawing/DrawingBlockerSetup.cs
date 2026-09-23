using UnityEngine;

public class DrawingBlockerSetup : MonoBehaviour
{
    [SerializeField] private string blockerLayerName = "DrawingBlocker";
    [SerializeField] private bool addMeshColliders = true;

    private void Awake()
    {
        int layer = LayerMask.NameToLayer(blockerLayerName);

        if (layer < 0)
        {
            Debug.LogError("DrawingBlockerSetup: Create the DrawingBlocker layer first.");
            return;
        }

        Apply(transform, layer);
    }

    private void Apply(Transform target, int layer)
    {
        target.gameObject.layer = layer;
        MeshFilter filter = target.GetComponent<MeshFilter>();
        Collider collider3D = target.GetComponent<Collider>();
        Collider2D collider2D = target.GetComponent<Collider2D>();

        if (addMeshColliders && filter != null && filter.sharedMesh != null && collider3D == null && collider2D == null)
        {
            MeshCollider meshCollider = target.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
        }

        foreach (Transform child in target)
        {
            if (child.name != "PencilOutline")
                Apply(child, layer);
        }
    }
}
