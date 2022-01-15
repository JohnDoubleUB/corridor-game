using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialTracker : MonoBehaviour
{
    public List<Renderer> otherObjectRenderers = new List<Renderer>();

    public int[] trackSpecificMaterialElements;

    private MeshRenderer meshRenderer;
    private SpriteRenderer spriteRenderer;
    private Material[] trackedMeshMaterials;
    private Material[] trackedSpriteMaterials;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        if (meshRenderer != null && MaterialManager.current != null)
        {
            trackedMeshMaterials =
                trackSpecificMaterialElements != null && trackSpecificMaterialElements.Any() ?
                meshRenderer.sharedMaterials.Where((x, i) => trackSpecificMaterialElements.Contains(i)).ToArray() :
                meshRenderer.sharedMaterials;

            MaterialManager.current.TrackMaterials(trackedMeshMaterials);
        }

        if (spriteRenderer != null && MaterialManager.current != null)
        {
            trackedSpriteMaterials =
                trackSpecificMaterialElements != null && trackSpecificMaterialElements.Any() ?
                spriteRenderer.sharedMaterials.Where((x, i) => trackSpecificMaterialElements.Contains(i)).ToArray() :
                spriteRenderer.sharedMaterials;

            MaterialManager.current.TrackMaterials(trackedSpriteMaterials);
        }

        if (otherObjectRenderers.Any() && MaterialManager.current != null)
        {
            MaterialManager.current.TrackMaterials(otherObjectRenderers.SelectMany(x => x.sharedMaterials).ToArray());
        }

    }

    private void OnDestroy()
    {
        if (meshRenderer != null && MaterialManager.current != null)
        {
            MaterialManager.current.UntrackMaterials(trackedMeshMaterials);
        }

        if (spriteRenderer != null && MaterialManager.current != null)
        {
            MaterialManager.current.UntrackMaterials(trackedSpriteMaterials);
        }

        if (otherObjectRenderers.Any() && MaterialManager.current != null)
        {
            MaterialManager.current.UntrackMaterials(otherObjectRenderers.SelectMany(x => x.sharedMaterials).ToArray());
        }
    }
}
