using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ObjectName = "";
    public bool IsInteractable = true;

    public SignificantObjectController puzzleObjectToNotifyOnInteract;

    public void IntiateInteract() 
    {
        if (IsInteractable) OnInteract();
    }

    protected virtual void OnInteract() 
    {
        print("Interaction initiated but no override present for this object!");
    }

    protected void OnSuccessfulInteract() 
    {
        if (puzzleObjectToNotifyOnInteract != null) puzzleObjectToNotifyOnInteract.RegisterInteract();
    }
}
