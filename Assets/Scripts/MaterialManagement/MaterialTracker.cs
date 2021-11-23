using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialTracker : MonoBehaviour
{
    public List<Renderer> otherObjectRenderers = new List<Renderer>();

    private MeshRenderer meshRenderer;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        if (meshRenderer != null && MaterialManager.current != null) 
        {
            MaterialManager.current.TrackMaterials(meshRenderer.sharedMaterials);
        }

        if (spriteRenderer != null && MaterialManager.current != null)
        {
            MaterialManager.current.TrackMaterials(spriteRenderer.sharedMaterials);
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
            MaterialManager.current.UntrackMaterials(meshRenderer.sharedMaterials);
        }

        if (spriteRenderer != null && MaterialManager.current != null)
        {
            MaterialManager.current.UntrackMaterials(spriteRenderer.sharedMaterials);
        }

        if (otherObjectRenderers.Any() && MaterialManager.current != null)
        {
            MaterialManager.current.UntrackMaterials(otherObjectRenderers.SelectMany(x => x.sharedMaterials).ToArray());
        }
    }
}
