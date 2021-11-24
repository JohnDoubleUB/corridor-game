using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager current;


    public AudioClip AmbientCreaking;

    public AudioSource FirstPersonPlayerSource;

    public float pitchVariation = 0.2f;

    private bool playFromFirstPersonAudioSource;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void SetCreakingVolumeAt(AudioSourceType audioSource, float volume) 
    {
        AudioSource selectedSource;

        switch (audioSource) 
        {
            default:
            case AudioSourceType.FirstPersonPlayer:
                selectedSource = FirstPersonPlayerSource;
                break;
        }

        if (volume == 0f)
        {
            playFromFirstPersonAudioSource = false;
        }
        else if (selectedSource.clip != AmbientCreaking || !selectedSource.isPlaying) 
        {
            selectedSource.clip = AmbientCreaking;
            playFromFirstPersonAudioSource = true;
        }

        selectedSource.volume = playFromFirstPersonAudioSource ? volume : 1f;
    }

    private void Update()
    {
        if (playFromFirstPersonAudioSource && !FirstPersonPlayerSource.isPlaying)
        {
            FirstPersonPlayerSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            FirstPersonPlayerSource.Play();
        }
        else if (!playFromFirstPersonAudioSource) 
        {
            FirstPersonPlayerSource.Stop();
        }
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, bool withPitchVariation = true, float delayInSeconds = 0f) 
    {
        return PlayClipAt(clip, pos, volume, withPitchVariation ? Random.Range(1f - pitchVariation, 1f + pitchVariation) : 1, delayInSeconds);
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, float pitch, float delayInSeconds)
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = pos; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.clip = clip; // define the clip
        aSource.pitch = pitch;
        aSource.volume = volume;

        aSource.PlayDelayed(delayInSeconds); // start the sound
        Destroy(tempGO, clip.length); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}

public enum AudioSourceType 
{
    FirstPersonPlayer
}