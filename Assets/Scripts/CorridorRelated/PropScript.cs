using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : MonoBehaviour
{
    public Transform FakeParent;
    public bool flipXZIfCorridorFlip;
    public bool rotateY180IfCorridorFlip;

    private void Start()
    {
        if(GameManager.current != null && GameManager.current.GameParent != null)
        transform.SetParent(GameManager.current.GameParent.transform);
    }
    // Update is called once per frame
    void Update()
    {
        if (FakeParent != null && FakeParent.position != transform.position) transform.position = FakeParent.position;
    }

    public void AccountForCorridorFlip(bool corridorIsFlipped = false) 
    {
        if (corridorIsFlipped && flipXZIfCorridorFlip) transform.localScale = new Vector3(-1, 1, -1);
        if (corridorIsFlipped && rotateY180IfCorridorFlip) transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, 180, 0));
        else transform.localScale = Vector3.one;
    }
}
