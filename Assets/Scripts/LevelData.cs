using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public CorridorLayoutHandler[] CorridorLayouts;
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber;
    public NumberpadPassword[] NumberpadPasswords;

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
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber { get { return levelData.CompleteLevelTriggerOnLayoutNumber; } }
    public string[] NumberpadPasswords;
    public bool GetIfLevelTriggerAndReturnLevelChange(CorridorLayoutHandler corridorLayout, out int LevelToChangeTo)
    {
        return levelData.GetIfLevelTriggerAndReturnLevelChange(corridorLayout, out LevelToChangeTo);
    }

    public LevelData_Loaded(LevelData levelData) 
    {
        this.levelData = levelData;
        //generate all the passwords for this level
        NumberpadPasswords = levelData.NumberpadPasswords.Select(x => x.GenerateRandomPassword()).ToArray();
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
public class NumberpadPassword
{
    public string possibleCharacters = "0123456789";
    public int passwordLength = 6;

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