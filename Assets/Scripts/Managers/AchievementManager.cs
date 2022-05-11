using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager current;

    public CG_AchievementButton[] AchievementButtons;
    public AchievementSaveData AchievementSaveData;

    //private List<KeyValuePair<string, CG_AchievementButton>> loadedButtons = new List<KeyValuePair<string, CG_AchievementButton>>();
    public Dictionary<string, CG_AchievementButton> LoadedButtons = new Dictionary<string, CG_AchievementButton>();

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void TriggerAchievement(string AchievementIdentifer) 
    {
        //Check this is an achievement
        if (LoadedButtons.ContainsKey(AchievementIdentifer)) 
        {
            SteamIntegration.SetAchievement(AchievementIdentifer);
            AchievementSaveData.UpdateAchievementByIdentifier(AchievementIdentifer);
            SaveSystem.SaveAchievements(AchievementSaveData);
        }
    }

    private void Start()
    {
        if (AchievementButtons != null && AchievementButtons.Any())
        {
            LoadedButtons = AchievementButtons.ToDictionary(x => x.Achievement.Identifier);
            if (Steamworks.SteamClient.IsValid)
            {
                if (SaveSystem.TryLoadAchievements(out AchievementSaveData loadedData))
                {
                    //Ensure that any achievements that need to be set in steam from local are
                    SteamIntegration.SetAchievements(loadedData.AchievementData.Where(x => x.Achieved).Select(x => x.Identifier));
                    loadedData.UpdateAchievementDataFromSteam(Steamworks.SteamUserStats.Achievements);
                    AchievementSaveData = loadedData;
                }
                else //No local achievements save found
                {
                    AchievementSaveData = new AchievementSaveData(Steamworks.SteamUserStats.Achievements);
                }
            }
            else if (SaveSystem.TryLoadAchievements(out AchievementSaveData loadedData))// This is where we'd check for saved achievementdata
            {
                AchievementSaveData = loadedData;
            }
            else
            {
                AchievementSaveData = new AchievementSaveData(LoadedButtons.Select(x => x.Key));
            }

            //Once we have dealt with loading data
            ////update the achievement buttons in the menu
            //if (loadedButtons != null && loadedButtons.Any() && AchievementSaveData != null && AchievementSaveData.AchievementData.Any())
            //{
            //    foreach (AchievementData ad in AchievementSaveData.AchievementData)
            //    {
            //        if (ad.Achieved)
            //        {
            //            loadedButtons[ad.Identifier].SetAchieved();
            //        }
            //        else
            //        {
            //            loadedButtons[ad.Identifier].SetUnachieved();
            //        }
            //    }
            //}
        }
    }

}

