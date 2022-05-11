using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager current;
    
    public CG_AchievementButton[] AchievementButtons;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        //SaveSystem
    }

    private void Start()
    {
        if (Steamworks.SteamClient.IsValid)
        {





        }
        else if (true )// This is where we'd check for saved achievementdata
        {

        }
    }


    private 
}

