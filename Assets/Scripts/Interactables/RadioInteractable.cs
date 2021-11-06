using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioInteractable : InteractableObject
{
    public Animator radioAnimatorFront;
    public Animator radioAnimatorBack;

    public bool radioOn;
    protected override void OnInteract()
    {
        radioOn = !radioOn;

        if (radioAnimatorFront != null) 
        {
            radioAnimatorFront.Play(radioOn ? "On" : "Off");
        }

        if (radioAnimatorBack != null) 
        {
            radioAnimatorBack.Play(radioOn ? "On" : "Off");
        }
    }
}
