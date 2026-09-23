using UnityEngine;

public class PencilOutlineInstaller : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private bool includeInactive = true;

    private void Start()
    {
        Install();
    }

    [ContextMenu("Install Pencil Outlines")]
    public void Install()
    {
        if (outlineMaterial == null)
        {
            Debug.LogError("PencilOutlineInstaller: Outline material is missing.");
            return;
        }

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(includeInactive);

        foreach (MeshFilter filter in filters)
        {
            if (filter == null || filter.sharedMesh == null || filter.gameObject.name == "PencilOutline")
                continue;

            if (filter.GetComponent<MeshRenderer>() == null)
                continue;

            PencilOutline outline = filter.GetComponent<PencilOutline>();

            if (outline == null)
                outline = filter.gameObject.AddComponent<PencilOutline>();

            outline.SetMaterial(outlineMaterial);
        }
    }
}
