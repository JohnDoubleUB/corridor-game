using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicTrackState", menuName = "ScriptableObjects/MusicTrackState", order = 2)]
public class MusicTrackState : ScriptableObject
{
    public EnabledMusicTracks EnabledTracks;
}

[System.Serializable]
public struct EnabledMusicTracks //TODO: does this need to be a class?
{
    public bool Track0, Track1, Track2, Track3, Track4, Track5;

    //public EnabledMusicTracks() 
    //{
    //    Track0 = false;
    //    Track1 = false;
    //    Track2 = false;
    //    Track3 = false;
    //    Track4 = false;
    //    Track5 = false;
    //}

    public EnabledMusicTracks(bool[] enabledTracks) 
    {
        int trackLength = enabledTracks.Length;

        Track0 = trackLength > 0 && enabledTracks[0];
        Track1 = trackLength > 1 && enabledTracks[1];
        Track2 = trackLength > 2 && enabledTracks[2];
        Track3 = trackLength > 3 && enabledTracks[3];
        Track4 = trackLength > 4 && enabledTracks[4];
        Track5 = trackLength > 5 && enabledTracks[5];

        //bool[] trackList = TrackList;

        //for (int i = 0; i < trackList.Length && i < enabledTracks.Length; i++) 
        //{
        //    trackList[i] = enabledTracks[i];
        //}
    }

    private void AssignAllTracks() 
    {
        Track0 = false;
        Track1 = false;
        Track2 = false;
        Track3 = false;
        Track4 = false;
        Track5 = false;
    }
    public bool[] TrackList 
    {
        get 
        {
            return new bool[] { Track0, Track1, Track2, Track3, Track4, Track5 };
        } 
    }
}