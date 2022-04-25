using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager current;

    public delegate void EntityNoiseAlertAction(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin = NoiseOrigin.Unspecified);
    public static event EntityNoiseAlertAction OnEntityNoiseAlert;

    public AudioClip AmbientCreaking;

    public AudioSource FirstPersonPlayerSource;

    public AudioMixer AmbientTrackMixer;

    public List<MusicMixerTrack> AmbientTracks;

    public int enabledTracksAtStart = 4;

    public float pitchVariation = 0.2f;

    private bool initialUpdate = true;

    private bool playFromFirstPersonAudioSource;

    private List<AudioSource> tempAudios = new List<AudioSource>();

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        AmbientTracks = AmbientTrackMixer.FindMatchingGroups("Master").Where(x => x.name != "Master").Select(x => new MusicMixerTrack(x.name, AmbientTrackMixer)).ToList();

        for (int i = 0; i < AmbientTracks.Count; i++)
        {
            if (i >= enabledTracksAtStart) AmbientTracks[i].trackOn = false;
        }
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

    public MusicMixerTrack GetAmbientTrackByIndex(int index) 
    {
        return AmbientTracks[index];
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


        foreach (MusicMixerTrack track in AmbientTracks)
        {
            if (track.trackOn != track.trackOn_LastFrame)
            {
                track.trackOn_LastFrame = track.trackOn;
                track.SetTrackActive(track.trackOn, !initialUpdate);
                //track.SetTrackTo(track.trackOn ? 0 : -80f);
            }
        }

        if (initialUpdate) initialUpdate = false;

    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, bool withPitchVariation = true, float delayInSeconds = 0f, bool noiseCanBeHeardByEntities = true, float noiseAlertRadius = 10f, NoiseOrigin noiseOrigin = NoiseOrigin.Unspecified)
    {
        return PlayClipAt(clip, pos, volume, withPitchVariation ? Random.Range(1f - pitchVariation, 1f + pitchVariation) : 1, delayInSeconds, noiseCanBeHeardByEntities, noiseAlertRadius, noiseOrigin);
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, float pitch, float delayInSeconds, bool noiseCanBeHeardByEntities = true, float noiseAlertRadius = 10f, NoiseOrigin noiseOrigin = NoiseOrigin.Unspecified)
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = pos; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.spatialBlend = 0.5f;
        aSource.clip = clip; // define the clip
        aSource.pitch = pitch;
        aSource.volume = volume;

        aSource.PlayDelayed(delayInSeconds); // start the sound
        Destroy(tempGO, clip.length + delayInSeconds); // destroy object after clip duration

        if (noiseCanBeHeardByEntities) GenerateNoiseAlert(pos, noiseAlertRadius, noiseOrigin); //OnEntityNoiseAlert?.Invoke(pos, noiseAlertRadius, noiseOrigin);

        //Added this to try and stop issues where temp audio isn't cleaned up
        //ClearNullTempAudiosFromList();
        //tempAudios.Add(aSource);

        return aSource; // return the AudioSource reference
    }

    private void ClearNullTempAudiosFromList() 
    {
        tempAudios = tempAudios.Where(x => x != null).ToList();
    }

    public void GenerateNoiseAlert(Vector3 position, float noiseAlertRadius = 10f, NoiseOrigin noiseOrigin = NoiseOrigin.Unspecified) 
    {
        OnEntityNoiseAlert?.Invoke(position, noiseAlertRadius, noiseOrigin);
    }

    private void OnDestroy()
    {
    }
}

public enum NoiseOrigin 
{
    Unspecified,
    TVMan,
    Mouse
}

public enum AudioSourceType
{
    FirstPersonPlayer
}

[System.Serializable]
public class MusicMixerTrack
{
    private AsyncAudioEaser currentMixerTask;


    [HideInInspector]
    public float audioMax = 0;
    [HideInInspector]
    public float audioMin = -80f;

    [HideInInspector]
    public string AmbientTrackName;
    private AudioMixer Mixer;
    public bool trackOn = true;

    [HideInInspector]
    public bool trackOn_LastFrame = true;

    public MusicMixerTrack(string ambientTrackName, AudioMixer mixer)
    {
        AmbientTrackName = ambientTrackName;
        Mixer = mixer;
    }

    public void SetTrackActive(bool trackActive, bool easeAudioInOut = true)
    {
        float audioTarget = trackActive ? audioMax : audioMin;
        if (easeAudioInOut)
        {
            if (currentMixerTask != null && !currentMixerTask.IsCompleted)
            {
                currentMixerTask.Stop();
            }
            currentMixerTask = new AsyncAudioEaser(audioTarget, AmbientTrackName, Mixer);
            currentMixerTask.EaseTrackAsync();
        }
        else
        {
            if (currentMixerTask != null && !currentMixerTask.IsCompleted) currentMixerTask.Stop();
            Mixer.SetFloat(AmbientTrackName, audioTarget);
        }
    }

    public void SetTrackTo(float value)
    {
        if (currentMixerTask != null && !currentMixerTask.IsCompleted) currentMixerTask.Stop();
        Mixer.SetFloat(AmbientTrackName, value);
    }

    class AsyncAudioEaser
    {
        private bool IsCanceled = false;
        private float audioTarget;
        private AudioMixer audioMixer;
        private string trackName;

        private Task task;

        public bool IsCompleted
        {
            get
            {
                if (task != null) return task.IsCompleted;
                else return true;
            }
        }

        public AsyncAudioEaser(float audioTarget, string trackName, AudioMixer audioMixer)
        {
            this.audioTarget = audioTarget;
            this.audioMixer = audioMixer;
            this.trackName = trackName;
        }


        public async void EaseTrackAsync()
        {
            task = /*Task.Run(() => */_EaseTrack();/*);*/
            await task;
        }

        public void Stop()
        {
            IsCanceled = true;
        }

        private async Task _EaseTrack()
        {
            if (audioMixer.GetFloat(trackName, out float trackCurrent))
            {
                //figure out where we are in terms of percentage complete already
                float positionValue = 0;

                while (true && positionValue < 1)
                {
                    positionValue += Time.deltaTime * 0.5f;
                    audioMixer.SetFloat(trackName, Mathf.Lerp(trackCurrent, audioTarget, positionValue));

                    if (IsCanceled)
                    {
                        break;
                    }

                    await Task.Yield();
                }

                if (!IsCanceled) audioMixer.SetFloat(trackName, audioTarget);

            }
        }
    }

}