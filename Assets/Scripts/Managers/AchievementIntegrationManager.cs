using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementIntegrationManager : SteamIntegrationManager
{
    public static AchievementIntegrationManager current;


    [Header("Achievement Integration Data")]
    public CG_AchievementData[] CGAchievementData;
    
    protected new void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
        base.Awake();
    }

    protected override void OnSteamValid()
    {
        //Ensure that steam achievements are all synced up
        if (SaveSystem.TryLoadAchievementsDictionary(out Dictionary<string, bool> savedAchievementsData)) 
        {
            allGameAchievements = CombineAchievementDataAsDistinctFavouringTrue(allGameAchievements, savedAchievementsData);
            foreach (KeyValuePair<string, bool> pair in savedAchievementsData) if (pair.Value) SetSteamAchievement(pair.Key);
            SaveAchievementsLocally();
        }
    }
    protected override void OnSteamInvalid()
    {
        if (SaveSystem.TryLoadAchievementsDictionary(out Dictionary<string, bool> savedAchievementsData) && savedAchievementsData.Any())
        {
            allGameAchievements = savedAchievementsData;
        }
        else 
        {
            Dictionary<string, bool> newDict = new Dictionary<string, bool>();
            foreach (CG_AchievementData aD in CGAchievementData) newDict.Add(aD.Identifier, false);
            allGameAchievements = newDict;
            SaveAchievementsLocally();
        }
    }
    private void SaveAchievementsLocally() 
    {
        SaveSystem.SaveAchievementsDictionary(allGameAchievements);
    }

    public void SetAchievement(string Identifier, bool Achieved = true) 
    {
        SetSteamAchievement(Identifier, Achieved);
        if (allGameAchievements.TryGetValue(Identifier, out bool result) && result != Achieved) allGameAchievements[Identifier] = Achieved;
    }

    public void ClearAllAchievements() 
    {
        foreach (CG_AchievementData aD in CGAchievementData) 
        {
            SetSteamAchievement(aD.Identifier, false);
            allGameAchievements[aD.Identifier] = false;
        }
        SaveAchievementsLocally();
    }

    private Dictionary<string, bool> CombineAchievementDataAsDistinctFavouringTrue(Dictionary<string, bool> dict1, Dictionary<string, bool> dict2)
    {
        Dictionary<string, bool> newDict;
        Dictionary<string, bool> dictToAdd;

        int dict1Length = dict1.Count;
        int dict2Length = dict2.Count;

        if (dict1Length == dict2Length || dict1Length > dict2Length)
        {
            newDict = dict1;
            dictToAdd = dict2;

        }
        else
        {
            newDict = dict2;
            dictToAdd = dict1;
        }

        foreach (KeyValuePair<string, bool> pair in dictToAdd)
        {
            if (newDict.ContainsKey(pair.Key))
            {
                if (pair.Value) newDict[pair.Key] = true;
            }
            else
            {
                newDict.Add(pair.Key, pair.Value);
            }
        }

        return newDict;
    }

    public bool IsAchieved(string Identifier) 
    {
        return allGameAchievements.TryGetValue(Identifier, out bool result) && result;
    }

    private new void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        SaveAchievementsLocally();
    }

    //private void Start()
    //{
    //    SetAchievement("ACH_WIN_ONE_GAME", false);
    //}
}

