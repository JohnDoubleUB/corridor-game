using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public CorridorLayoutHandler[] CorridorLayouts;
    public CorridorLayoutHandler[] BackwardOnlyLayouts;
    public NumberpadPassword[] NumberpadPasswords;

    public bool SectionOrdersAreRandom;

    [Header("Room Effects - Random")]
    public bool AllowRandomScaling;
    public bool AllowRandomWaving;
    public int MaxScaleEffectCount;
    public int MaxWaveEffectCount;

    [Header("LevelChanging")]
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber;
    public LevelSectionCountTrigger[] CompleteLevelOnTraveledSectionCount;
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutPuzzleComplete;

    public bool GetIfLevelTriggerAndReturnLevelChange(CorridorLayoutHandler corridorLayout, out int LevelToChangeTo)
    {
        if (CompleteLevelTriggerOnLayoutNumber.Any())
        {
            LevelSwitchTrigger switchTrigger = CompleteLevelTriggerOnLayoutNumber.FirstOrDefault(x => x.LayoutNumberTrigger == corridorLayout.layoutNumber && corridorLayout.layoutLevelNumber == LevelNumber);

            if (switchTrigger != null)
            {
                LevelToChangeTo = switchTrigger.LevelChange;
                return true;
            }
        }

        LevelToChangeTo = -1;
        return false;
    }

    public bool GetIfLevelTriggerOnLayoutPuzzleCompleteAndReturnLevelChange(CorridorLayoutHandler corridorLayout, out int LevelToChangeTo) 
    {
        if (CompleteLevelTriggerOnLayoutPuzzleComplete.Any())
        {
            LevelSwitchTrigger switchTrigger = CompleteLevelTriggerOnLayoutPuzzleComplete.FirstOrDefault(x => x.LayoutNumberTrigger == corridorLayout.layoutNumber && corridorLayout.layoutLevelNumber == LevelNumber);

            if (switchTrigger != null)
            {
                LevelToChangeTo = switchTrigger.LevelChange;
                return true;
            }
        }

        LevelToChangeTo = -1;
        return false;
    }

    public bool GetIfLevelCountTriggerAndReturnLevelChange(int currentSectionCount, out int LevelToChangeTo)
    {
        if (CompleteLevelOnTraveledSectionCount.Any()) 
        {

            LevelSectionCountTrigger countTrigger = CompleteLevelOnTraveledSectionCount.FirstOrDefault(cT => 
            {
                int generatedSectionCount = cT.RandomSectionRange > 0 ? Mathf.Clamp(Random.Range(cT.SectionCount - cT.RandomSectionRange, cT.SectionCount + -cT.RandomSectionRange), 1, 100) : cT.SectionCount;
                return generatedSectionCount <= currentSectionCount; 
            });

            if (countTrigger != null) 
            {
                LevelToChangeTo = countTrigger.LevelChange;
                return true;
            }
        }


        LevelToChangeTo = -1;
        return false;
    }

    public static implicit operator LevelData_Loaded(LevelData levelData) 
    {

        return new LevelData_Loaded(levelData);
    }
}

public class LevelData_Loaded
{
    private LevelData levelData;
    public int LevelNumber { get { return levelData.LevelNumber; } }
    public CorridorLayoutHandler[] CorridorLayouts { get {return levelData.CorridorLayouts; } }
    public CorridorLayoutHandler[] BackwardOnlyLayouts { get { return levelData.BackwardOnlyLayouts; } }
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber { get { return levelData.CompleteLevelTriggerOnLayoutNumber; } }
    public LevelSectionCountTrigger[] CompleteLevelOnTraveledSectionCount { get { return levelData.CompleteLevelOnTraveledSectionCount; } }

    public bool SectionOrdersAreRandom { get { return levelData.SectionOrdersAreRandom; } }
    public bool AllowRandomScaling { get { return levelData.AllowRandomScaling; } }
    public bool AllowRandomWaving { get { return levelData.AllowRandomWaving; } }
    public int MaxScaleEffectCount { get { return levelData.MaxScaleEffectCount; } }
    public int MaxWaveEffectCount { get { return levelData.MaxWaveEffectCount; } }

    public string[] NumberpadPasswords;

    public LayoutLevelData[] CorridorLayoutData;

    public bool GetIfLevelTriggerAndReturnLevelChange(CorridorLayoutHandler corridorLayout, out int LevelToChangeTo)
    {
        return levelData.GetIfLevelTriggerAndReturnLevelChange(corridorLayout, out LevelToChangeTo);
    }

    public bool GetIfLevelTriggerOnLayoutPuzzleCompleteAndReturnLevelChange(CorridorLayoutHandler corridorLayout, out int LevelToChangeTo) 
    {
        return levelData.GetIfLevelTriggerOnLayoutPuzzleCompleteAndReturnLevelChange(corridorLayout, out LevelToChangeTo);
    }

    public bool GetIfLevelCountTriggerAndReturnLevelChange(int currentSectionCount, out int LevelToChangeTo) 
    {
        return levelData.GetIfLevelCountTriggerAndReturnLevelChange(currentSectionCount, out LevelToChangeTo);
    }

    public LevelData_Loaded(LevelData levelData) 
    {
        this.levelData = levelData;
        //generate all the passwords for this level

        
        NumberpadPasswords = levelData.NumberpadPasswords.Select(x => 
        {
            string randomPassword = x.GenerateRandomPassword();

            return randomPassword;
        }).ToArray();

        //Generate LevelLayoutData for all the layouts
        CorridorLayoutData = CorridorLayouts.Union(BackwardOnlyLayouts).Select(x => new LayoutLevelData(x.LayoutID)).ToArray();

    }
}

[System.Serializable]
public class LevelSwitchTrigger 
{
    public string Name = "LevelChangeTrigger";
    public int LayoutNumberTrigger;
    public int LevelChange;
}

[System.Serializable]
public class LevelSectionCountTrigger 
{
    public string Name = "LevelSectionCountTrigger";
    public int SectionCount;
    public int LevelChange;
    public int RandomSectionRange;
}

[System.Serializable]
public class NumberpadPassword
{
    public string possibleCharacters = "0123456789";
    public int passwordLength = 6;
    public int missingKeyCount = 0;

    public string GenerateRandomPassword() 
    {
        return GenerateRandomPasswordOfLength(passwordLength, possibleCharacters);
    }

    private string GenerateRandomPasswordOfLength(int passwordLength, string possibleCharacters)
    {
        string newPassword = "";
        for (int i = 0; i < passwordLength; i++) newPassword += possibleCharacters[Random.Range(0, possibleCharacters.Length)];
        return newPassword;
    }
}

public class LayoutLevelData 
{
    public string LayoutID;
    public List<int> collectedItems = new List<int>();
    public List<int> completedPuzzles = new List<int>();

    public List<PuzzleElementControllerData> puzzleData = new List<PuzzleElementControllerData>();

    public LayoutLevelData(string layoutID) 
    {
        LayoutID = layoutID;
    }
}