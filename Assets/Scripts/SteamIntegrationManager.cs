using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SteamIntegrationManager : MonoBehaviour
{
    [Header("Steam Integration Settings")]
    public bool EnableSteamIntegration = true;
    public uint AppId;
    public bool UseTestAppId = true;

    private Dictionary<string, Steamworks.Data.Achievement> _achievements;
    protected Dictionary<string, bool> allGameAchievements;

    protected void Awake()
    {
        if (EnableSteamIntegration)
        {
            if (!Steamworks.SteamClient.IsValid)
            {
                try
                {
                    Steamworks.SteamClient.Init(UseTestAppId ? 480 : AppId); //Achievments can only be accessed once a game is published, We're using spacewar rn
                    print("Client name: " + Steamworks.SteamClient.Name);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);

                }
            }
            else
            {
                print("steamworks initialized");
            }

            if (Steamworks.SteamClient.IsValid)
            {
                print("doint this");
                _OnBeingValid();
                OnSteamValid();
            }
            else
            {
                OnSteamInvalid();
            }
        }
        else
        {
            OnSteamInvalid();
        }
    }

    private void _OnBeingValid()
    {
        //Store achievements in a nicer way to access them
        _achievements = Steamworks.SteamUserStats.Achievements.ToDictionary(x => x.Identifier);
        allGameAchievements = ConvertSteamAchievementToDictionary(Steamworks.SteamUserStats.Achievements);
        Steamworks.SteamFriends.OnGameOverlayActivated += OnOverlayActivated;

    }

    protected virtual void OnSteamValid()
    {
    }

    protected virtual void OnSteamInvalid()
    {
    }

    protected void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    protected void OnApplicationQuit()
    {
        SteamShutdownProcedure();
    }

    private void OnOverlayActivated(bool activated)
    {
        if (activated && !GameManager.current.IsPaused) GameManager.current.TogglePauseGame();
    }

    protected void OnDisable()
    {
        if (Application.isEditor) SteamShutdownProcedure();
    }

    private void SteamShutdownProcedure()
    {
        print("shut down steam");
        if (EnableSteamIntegration)
        {
            Steamworks.SteamFriends.OnGameOverlayActivated -= OnOverlayActivated;
            Steamworks.SteamClient.Shutdown();
        }
    }


    //Achievement Commands

    //Set achievement, allows you to set achieved or unachieved, returns true if successful
    protected bool SetSteamAchievement(string Identifier, bool achieved = true)
    {
        if (_achievements != null && _achievements.TryGetValue(Identifier, out Steamworks.Data.Achievement achievement)) 
        {
            if (achieved) achievement.Trigger();
            else achievement.Clear();

            return true;
        }

        return false;
    }

    private Dictionary<string, bool> ConvertSteamAchievementToDictionary(IEnumerable<Steamworks.Data.Achievement> Achievements)
    {
        Dictionary<string, bool> newDict = new Dictionary<string, bool>();
        foreach (Steamworks.Data.Achievement ach in Achievements)
        {
            newDict.Add(ach.Identifier, ach.State);
        }
        return newDict;
    }
}
