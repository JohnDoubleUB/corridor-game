using UnityEngine;

public class PencilScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(GameManager.current.trueCamera.transform);
    }
}
