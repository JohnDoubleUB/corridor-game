using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    //public static AchievementManager current;

    //public CG_AchievementButton[] AchievementButtons;
    ////public AchievementSaveData AchievementSaveData;
    //private Dictionary<string, AchievementData> LoadedAchievements = new Dictionary<string, AchievementData>();

    ////private List<KeyValuePair<string, CG_AchievementButton>> loadedButtons = new List<KeyValuePair<string, CG_AchievementButton>>();
    ////public Dictionary<string, CG_AchievementButton> LoadedButtons = new Dictionary<string, CG_AchievementButton>();

    //public bool IsAchieved(string AchievementIdentifer) 
    //{
    //    return LoadedAchievements.ContainsKey(AchievementIdentifer) && LoadedAchievements[AchievementIdentifer].Achieved;
    //}

    //private void Awake()
    //{
    //    if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
    //    current = this;
    //}

    //public void TriggerAchievement(string AchievementIdentifer) 
    //{
    //    //Check this is an achievement
    //    if (LoadedAchievements.ContainsKey(AchievementIdentifer)) 
    //    {
    //        //SteamIntegrationManager.SetAchievement(AchievementIdentifer);
    //        //AchievementSaveData.UpdateAchievementByIdentifier(AchievementIdentifer);
    //        LoadedAchievements[AchievementIdentifer] = new AchievementData() { Identifier = AchievementIdentifer, Achieved = true };
    //        //SaveSystem.SaveAchievements(AchievementSaveData);

    //    }
    //}

    //private void Start()
    //{
    //    if (AchievementButtons != null && AchievementButtons.Any())
    //    {
    //        if (Steamworks.SteamClient.IsValid)
    //        {
    //            if (SaveSystem.TryLoadAchievements(out AchievementSaveData loadedData))
    //            {
    //                //Ensure that any achievements that need to be set in steam from local are
    //                //SteamIntegrationManager.SetAchievements(loadedData.AchievementData.Where(x => x.Achieved).Select(x => x.Identifier));
    //                loadedData.UpdateAchievementDataFromSteam(Steamworks.SteamUserStats.Achievements);
    //                AchievementSaveData = loadedData;
    //            }
    //            else //No local achievements save found
    //            {
    //                AchievementSaveData = new AchievementSaveData(Steamworks.SteamUserStats.Achievements);
    //            }
    //        }
    //        else if (SaveSystem.TryLoadAchievements(out AchievementSaveData loadedData))// This is where we'd check for saved achievementdata
    //        {
    //            AchievementSaveData = loadedData;
    //        }
    //        else
    //        {
    //            AchievementSaveData = new AchievementSaveData(AchievementButtons.Select(x => x.Achievement.Identifier));
    //        }

    //        LoadedAchievements = AchievementSaveData.GetAchievementDictionary();
    //    }
    //}

}

