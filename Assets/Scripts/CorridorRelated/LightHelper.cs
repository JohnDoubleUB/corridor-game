using UnityEngine;

public class LightHelper : MonoBehaviour
{
    public Light lightSource;
    public float lightSourceStrength = 0.2f;

    private void Update()
    {
        if (lightSource != null) lightSource.intensity = lightSourceStrength;
    }
}
