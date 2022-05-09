using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamIntegration : MonoBehaviour
{
    void Awake()
    {

        if (!Steamworks.SteamClient.IsValid)
        {
            print("steamworks not intiallized");
            try
            {
                Steamworks.SteamClient.Init(480/*1981590*/); //Achievments can only be accessed once a game is published, We're using spacewar rn
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
        Steamworks.SteamFriends.OnGameOverlayActivated -= OnOverlayActivated;
        Steamworks.SteamClient.Shutdown();
    }
}
