using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class PencilOutline : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    private GameObject outlineObject;

    public void SetMaterial(Material material)
    {
        outlineMaterial = material;
        Rebuild();
    }

    private void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        RemoveOutline();

        if (outlineMaterial == null)
            return;

        MeshFilter sourceFilter = GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = GetComponent<MeshRenderer>();

        if (sourceFilter == null || sourceRenderer == null || sourceFilter.sharedMesh == null)
            return;

        outlineObject = new GameObject("PencilOutline");
        outlineObject.transform.SetParent(transform, false);
        outlineObject.layer = gameObject.layer;

        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = sourceFilter.sharedMesh;

        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = outlineMaterial;
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
        outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
        outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private void RemoveOutline()
    {
        if (outlineObject == null)
        {
            Transform existing = transform.Find("PencilOutline");

            if (existing != null)
                outlineObject = existing.gameObject;
        }

        if (outlineObject == null)
            return;

        if (Application.isPlaying)
            Destroy(outlineObject);
        else
            DestroyImmediate(outlineObject);

        outlineObject = null;
    }

    private void OnDestroy()
    {
        RemoveOutline();
    }
}