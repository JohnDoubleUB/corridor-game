using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ObjectName = "";
    public virtual void IntiateInteract() 
    {
        print("Interaction initiated but no override present for this object!");
    }
}
