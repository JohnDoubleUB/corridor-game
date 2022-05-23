using UnityEngine;

public class CG_CameraShaker : MonoBehaviour
{
    public float shakeAmount = 0.1f;

    [Range(0f, 1f)]
    public float shakeEffect = 0f;
    private void Update()
    {
        Vector2 newShake = new Vector2(Random.insideUnitSphere.x * shakeAmount, Random.insideUnitSphere.y * shakeAmount) * shakeEffect;
        transform.localPosition = new Vector3(newShake.x, newShake.y, 0f);
        transform.localRotation = Quaternion.Euler(newShake.x, newShake.y, 0f);
    }
}
