using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SteamIntegration : MonoBehaviour
{
    public bool EnableSteamIntegration = true;
    public uint AppId;
    public bool UseTestAppId = true;

    private static SteamIntegration current;

    public static bool SteamIntegrationEnabled 
    {
        get 
        {
            return current != null ? current.EnableSteamIntegration : false;
        }
    }

    void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;


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

            if (Steamworks.SteamClient.IsValid) OnBeingValid();
        }
    }

    private void OnBeingValid()
    {
        PrintYourName();
        Steamworks.SteamFriends.OnGameOverlayActivated += OnOverlayActivated;
    }

    private void PrintYourName()
    {
        Debug.Log(Steamworks.SteamClient.Name);
    }

    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        SteamShutdownProcedure();
    }

    private void OnOverlayActivated(bool activated) 
    {
        if (activated && !GameManager.current.IsPaused) GameManager.current.TogglePauseGame();
    }

    private void OnDisable()
    {
        if (Application.isEditor) SteamShutdownProcedure();
    }

    private void SteamShutdownProcedure() 
    {
        if (EnableSteamIntegration)
        {
            Steamworks.SteamFriends.OnGameOverlayActivated -= OnOverlayActivated;
            Steamworks.SteamClient.Shutdown();
        }
    }


    //Achievement Commands
    public static void SetAchievements(params string[] Ach_ApiNames)
    {
        if (Steamworks.SteamClient.IsValid) UpdateAchievements(Steamworks.SteamUserStats.Achievements.Where(ach => Ach_ApiNames.Contains(ach.Identifier)), true);
    }

    public static void ClearAchievements(params string[] Ach_ApiNames)
    {
        if (Steamworks.SteamClient.IsValid) UpdateAchievements(Steamworks.SteamUserStats.Achievements.Where(ach => Ach_ApiNames.Contains(ach.Identifier)), false);
    }

    public static void SetAllAchievements()
    {
        if (Steamworks.SteamClient.IsValid) UpdateAchievements(Steamworks.SteamUserStats.Achievements, true);
    }

    public static void ClearAllAchievements()
    {
        if (Steamworks.SteamClient.IsValid) UpdateAchievements(Steamworks.SteamUserStats.Achievements, false);
    }

    private static void UpdateAchievements(IEnumerable<Steamworks.Data.Achievement> Achievements, bool setAchieved = true)
    {
        if (Achievements != null)
        {
            foreach (Steamworks.Data.Achievement achievement in Achievements)
            {
                if (setAchieved) achievement.Trigger();
                else achievement.Clear();
            }
        }
    }
}
