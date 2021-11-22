using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioInteractable : InteractableObject
{
    public Animator radioAnimatorFront;
    public Animator radioAnimatorBack;
    
    public AudioClip RadioOnOffSound;
    public AudioClip RadioDroneSound;
    
    public AudioSource RadioSpeaker;
    public float RadioSpeakerDefaultVolume = 0.5f;

    public bool radioOn;

    private void Awake()
    {
        RadioSpeaker.clip = RadioDroneSound;
        RadioSpeaker.volume = RadioSpeakerDefaultVolume;
    }
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

        AudioManager.current.PlayClipAt(RadioOnOffSound, transform.position, 1f, true);

        if (radioOn)
        {
            RadioSpeaker.Play();
        }
        else 
        {
            RadioSpeaker.Stop();
        }
    }
}
