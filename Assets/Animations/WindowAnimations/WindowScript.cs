using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowScript : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float ChanceToTrigger = 0.5f;
    public Animator WindowAnimator;
    public AudioClip LightningAudio;
    private AudioSource source;


    public bool currentlyPlaying;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (!currentlyPlaying && Random.value < ChanceToTrigger) TriggerLightning();
        }
    }

    private void TriggerLightning() 
    {
        currentlyPlaying = true;
        WindowAnimator.Play("Lightning");
        transform.PlayClipAtTransform(LightningAudio);
    }

    private void Update()
    {
        if (currentlyPlaying && source == null) currentlyPlaying = false;
    }

}
