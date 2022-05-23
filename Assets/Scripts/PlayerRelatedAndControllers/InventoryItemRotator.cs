using UnityEngine;

public class InventoryItemRotator : MonoBehaviour
{
    public float speed = 2f;
    public float maxRotation = 45f;

    void Update()
    {
        transform.localRotation = Quaternion.Euler(0f, maxRotation * Mathf.Sin(Time.time * speed), 0f);
    }
}
