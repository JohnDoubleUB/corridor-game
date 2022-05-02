using System.Collections;
using System.Collections.Generic;
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
            AudioManager.current.SetTracksActive(TrackState.EnabledTracks);
            triggeredThisInstance = true;
        }
    }
}

