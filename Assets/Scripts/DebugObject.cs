using UnityEngine;
using UnityEngine.UI;

public class DebugObject : MonoBehaviour
{
    public Text TextObject;
    public GameObject RelatedObject;
    private void Update()
    {
        //if (TextObject != null) TextObject.gameObject.transform.LookAt(GameManager.current.trueCamera.transform.position);
        TextObject.gameObject.transform.rotation = Quaternion.LookRotation(TextObject.gameObject.transform.position - GameManager.current.trueCamera.transform.position);
    }
}
