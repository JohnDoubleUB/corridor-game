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
    public CorridorLayoutHandler[] CorridorLayouts { get { return levelData.CorridorLayouts; } }
    public CorridorLayoutHandler[] BackwardOnlyLayouts { get { return levelData.BackwardOnlyLayouts; } }
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber { get { return levelData.CompleteLevelTriggerOnLayoutNumber; } }
    public LevelSectionCountTrigger[] CompleteLevelOnTraveledSectionCount { get { return levelData.CompleteLevelOnTraveledSectionCount; } }

    public bool SectionOrdersAreRandom { get { return levelData.SectionOrdersAreRandom; } }
    public bool AllowRandomScaling { get { return levelData.AllowRandomScaling; } }
    public bool AllowRandomWaving { get { return levelData.AllowRandomWaving; } }
    public int MaxScaleEffectCount { get { return levelData.MaxScaleEffectCount; } }
    public int MaxWaveEffectCount { get { return levelData.MaxWaveEffectCount; } }

    public string[] NumberpadPasswords;

    public NumberpadPassword_Loaded[] NumberpadData;

    public LayoutLevelData[] CorridorLayoutData;

    public char[] GeneratedNumberpadPieces;

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


        NumberpadPasswords = levelData.NumberpadPasswords.Select(x => x.GenerateRandomPassword()).ToArray();
        NumberpadData = levelData.NumberpadPasswords.Select(x => (NumberpadPassword_Loaded)x).ToArray();

        char[][] allMissingCharacters = NumberpadData.Select(x => x.MissingCharacters).Where(x => x != null && x.Any()).ToArray();
        if (allMissingCharacters != null && allMissingCharacters.Any()) GeneratedNumberpadPieces = allMissingCharacters.SelectMany(x => x).ToArray();

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
    public bool makePasswordFixed;
    public int passwordLength = 6;
    public int missingKeyCount = 0;


    public string GenerateRandomPassword()
    {
        return GenerateRandomPasswordOfLength(passwordLength, possibleCharacters);
    }

    private string GenerateRandomPasswordOfLength(int passwordLength, string possibleCharacters)
    {
        string newPassword = "";

        if (makePasswordFixed)
        {
            newPassword = possibleCharacters;
        }
        else
        {
            //Incase we need them
            List<int> repeatedCharacterIndexes = new List<int>();
            string uniqueCharacters = "";

            //Initially generate password
            for (int i = 0; i < passwordLength; i++)
            {
                char newCharacter = possibleCharacters[Random.Range(0, possibleCharacters.Length)];

                if (missingKeyCount > 0 && newPassword.Contains(newCharacter))
                {
                    repeatedCharacterIndexes.Add(newPassword.Length);
                }
                else
                {
                    uniqueCharacters += newCharacter;
                }

                newPassword += newCharacter;
            }

            //If there are missing keys, ensure that the password has enough unique characters to have this many missing keys
            if (missingKeyCount > 0)
            {
                int uniqueKeysRequired = missingKeyCount + 1;
                int uniqueCharactersLength = uniqueCharacters.Length;

                if (uniqueCharactersLength < uniqueKeysRequired)
                {
                    Debug.Log("Not enough unique keys for puzzle, regenerating " + uniqueKeysRequired + " repeated password characters.");

                    int keysToChange = uniqueKeysRequired - uniqueCharactersLength; //Keys to change

                    char[] unusedCharacters = possibleCharacters.Where(x => !uniqueCharacters.Contains(x)).Shuffle().ToArray(); //Unused characters, randomly shuffled
                    int[] repeatedIndexesShuffled = repeatedCharacterIndexes.Shuffle().ToArray(); //Indexes randomly shuffled
                    char[] newPasswordArray = newPassword.ToArray(); //Password converted to char array for easier manipulation


                    for (int i = 0; i < keysToChange && i < unusedCharacters.Length && i < repeatedCharacterIndexes.Count; i++)
                    {
                        newPasswordArray[repeatedIndexesShuffled[i]] = unusedCharacters[i];
                    }

                    newPassword = new string(newPasswordArray);
                }
            }

        }

        return newPassword;
    }
}

public class NumberpadPassword_Loaded
{
    public string NumberpadPassword;
    public char[] MissingCharacters;

    public NumberpadPassword_Loaded(NumberpadPassword numberpadData)
    {
        NumberpadPassword = numberpadData.GenerateRandomPassword();
        if (numberpadData.missingKeyCount > 0)
        {
            char[] uniqueCharacters = NumberpadPassword.Distinct().ToArray();
            MissingCharacters = uniqueCharacters.Shuffle().Take(Mathf.Clamp(numberpadData.missingKeyCount, 1, uniqueCharacters.Length)).ToArray();

            if (MissingCharacters == null) MissingCharacters = new char[0];
        }

    }


    public static implicit operator NumberpadPassword_Loaded(NumberpadPassword numberpadData)
    {

        return new NumberpadPassword_Loaded(numberpadData);
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