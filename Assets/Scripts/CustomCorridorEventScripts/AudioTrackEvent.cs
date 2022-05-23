using UnityEngine;

public class AudioTrackEvent : CustomCorridorEventScript
{
    public MusicTrackState TrackState;
    public bool OnlyTriggerOnce;
    private bool triggeredThisInstance;
    public override void TriggerCustomEvent()
    {
        if (OnlyTriggerOnce ? TriggerEvent() : !triggeredThisInstance)
        {
            Debug.Log("AudioTrackEvent Activated - Tag: " + EventTag);
            AudioManager.current.SetTracksActive(TrackState.EnabledTracks);
            triggeredThisInstance = true;
        }
    }
}

