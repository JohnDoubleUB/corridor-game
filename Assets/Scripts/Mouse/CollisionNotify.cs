using UnityEngine;

public class CollisionNotify : MonoBehaviour
{
    public MouseEntity mouseToNotify;
    private void OnTriggerEnter(Collider other)
    {
        mouseToNotify.OnTriggerEnter(other);
    }
}
