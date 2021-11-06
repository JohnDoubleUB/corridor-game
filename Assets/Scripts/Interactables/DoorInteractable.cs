using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : InteractableObject
{
    public Door door;
    protected override void OnInteract()
    {
        if (door != null) door.InteractOpenClose();
    }
}
