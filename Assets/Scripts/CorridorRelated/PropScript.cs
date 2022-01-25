using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : FakeParentScript
{
    public bool flipXZIfCorridorFlip;
    public bool rotateY180IfCorridorFlip;

    public void AccountForCorridorFlip(bool corridorIsFlipped = false) 
    {
        if (corridorIsFlipped && flipXZIfCorridorFlip) transform.localScale = new Vector3(-1, 1, -1);
        else transform.localScale = Vector3.one;

        if (corridorIsFlipped && rotateY180IfCorridorFlip) transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, 180, 0));
    }
}
