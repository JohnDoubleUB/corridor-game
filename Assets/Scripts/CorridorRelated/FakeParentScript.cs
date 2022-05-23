using UnityEngine;

public class FakeParentScript : MonoBehaviour
{
    public Transform FakeParent;

    private void Start()
    {
        if (GameManager.current != null && GameManager.current.GameParent != null)
            transform.SetParent(GameManager.current.GameParent.transform);
    }
    // Update is called once per frame
    void Update()
    {
        if (FakeParent != null && FakeParent.position != transform.position) transform.position = FakeParent.position;
    }
}