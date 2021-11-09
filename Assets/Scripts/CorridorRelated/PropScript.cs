using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : MonoBehaviour
{
    public Transform FakeParent;
    public bool flipXZIfCorridorFlip;

    private void Start()
    {
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
        else transform.localScale = Vector3.one;
    }
}
