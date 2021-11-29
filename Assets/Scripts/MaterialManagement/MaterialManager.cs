using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager current;

    public List<Material> worldMaterials = new List<Material>();
    public float alternateBlend = 0;

    // Update is called once per frame
    private void Update()
    {
        foreach (Material mat in worldMaterials) 
        {
            if(mat.HasProperty("_AlternateTextureSwitch")) mat.SetFloat("_AlternateTextureSwitch", alternateBlend);
        }
    }

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void TrackMaterials(params Material[] materials) 
    {
        if(materials.Any()) worldMaterials = worldMaterials.Union(materials).ToList();
    }

    public void UntrackMaterials(params Material[] materials) 
    {
        //if(materials.Any()) worldMaterials = worldMaterials.Where(x => !materials.Contains(x)).ToList();
    }

    public void TrackMaterials(IEnumerable<Material> materials)
    {
        TrackMaterials(materials.ToArray());
    }

    public void UntrackMaterials(IEnumerable<Material> materials)
    {
        UntrackMaterials(materials.ToArray());
    }
}
