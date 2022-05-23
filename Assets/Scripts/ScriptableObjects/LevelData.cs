using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public bool IsCheckpoint;
    public bool CheckpointOnDelay;
    public bool TriggerEnableVisibilityPrompt;
    public CorridorLayoutHandler[] CorridorLayouts;
    public CorridorLayoutHandler[] BackwardOnlyLayouts;
    public CorridorLayoutHandler[] ForwardOnlyLayouts;
    public NumberpadPassword[] NumberpadPasswords;

    public bool SectionOrdersAreRandom;

    [Header("Room Effects - Random")]
    public bool AllowRandomScaling;
    public bool AllowRandomWaving;
    public int MaxScaleEffectCount;
    public int MaxWaveEffectCount;

    [Header("Level Changing")]
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber;
    public LevelSectionCountTrigger[] CompleteLevelOnTraveledSectionCount;
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutPuzzleComplete;

    [Header("Spawning And Patrol")]
    public int MaxMouseCount = 1;
    public bool EnableTVMan;

    [Header("Achievement Triggers")]
    public string[] AcheivementTriggers;

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
    public bool IsCheckpoint { get { return levelData.IsCheckpoint; } }
    public bool CheckPointOnDelay { get { return levelData.CheckpointOnDelay; } }
    public bool TriggerEnableVisibilityPrompt { get { return levelData.TriggerEnableVisibilityPrompt; } }
    public CorridorLayoutHandler[] CorridorLayouts { get { return levelData.CorridorLayouts; } }
    public CorridorLayoutHandler[] BackwardOnlyLayouts { get { return levelData.BackwardOnlyLayouts; } }

    public CorridorLayoutHandler[] ForwardOnlyLayouts { get { return levelData.ForwardOnlyLayouts; } }
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber { get { return levelData.CompleteLevelTriggerOnLayoutNumber; } }
    public LevelSectionCountTrigger[] CompleteLevelOnTraveledSectionCount { get { return levelData.CompleteLevelOnTraveledSectionCount; } }

    public bool SectionOrdersAreRandom { get { return levelData.SectionOrdersAreRandom; } }
    public bool AllowRandomScaling { get { return levelData.AllowRandomScaling; } }
    public bool AllowRandomWaving { get { return levelData.AllowRandomWaving; } }
    public int MaxScaleEffectCount { get { return levelData.MaxScaleEffectCount; } }
    public int MaxWaveEffectCount { get { return levelData.MaxWaveEffectCount; } }

    public string[] AchievementTriggers { get { return levelData.AcheivementTriggers; } }

    public int ScaleEffectCount;
    public int WaveEffectCount;

    public string[] NumberpadPasswords;

    public NumberpadPassword_Loaded[] NumberpadData;

    public LayoutLevelData[] CorridorLayoutData;

    public char[] GeneratedNumberpadPieces;
    public int MaxMouseCount { get { return levelData.MaxMouseCount; } }
    public bool EnableTVMan { get { return levelData.EnableTVMan; } }

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
        //I forgot to include it in layout data!
        CorridorLayoutData = CorridorLayouts.Union(BackwardOnlyLayouts).Union(ForwardOnlyLayouts).Select(x => new LayoutLevelData(x)).ToArray();

    }

    public LevelData_Loaded(LevelData levelData, LevelData_Serialized levelDataSerialized) 
    {
        this.levelData = levelData;

        //Load all passwords for this level

        NumberpadPasswords = levelDataSerialized.NumberpadPasswords;
        NumberpadData = levelDataSerialized.NumberpadData;
        GeneratedNumberpadPieces = levelDataSerialized.GeneratedNumberpadPieces;
        CorridorLayoutData = levelDataSerialized.CorridorLayoutData.Select(x => x.Deserialized).ToArray();
    }
}

[System.Serializable]
public class LevelData_Serialized 
{
    public int LevelNumber; //This is used to identify matching level data
    public int ScaleEffectCount;
    public int WaveEffectCount;
    public string[] NumberpadPasswords;
    public NumberpadPassword_Loaded[] NumberpadData;
    public LayoutLevelDataSerialized[] CorridorLayoutData;
    public char[] GeneratedNumberpadPieces;

    public LevelData_Serialized() { }
    public LevelData_Serialized(LevelData_Loaded levelDataLoaded) 
    {
        LevelNumber = levelDataLoaded.LevelNumber;
        ScaleEffectCount = levelDataLoaded.ScaleEffectCount;
        WaveEffectCount = levelDataLoaded.WaveEffectCount;
        NumberpadPasswords = levelDataLoaded.NumberpadPasswords;
        NumberpadData = levelDataLoaded.NumberpadData;
        CorridorLayoutData = levelDataLoaded.CorridorLayoutData.Select(x => new LayoutLevelDataSerialized(x)).ToArray();
        GeneratedNumberpadPieces = levelDataLoaded.GeneratedNumberpadPieces;
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

[System.Serializable]
public class NumberpadPassword_Loaded
{
    public string NumberpadPassword;
    public char[] MissingCharacters;

    public NumberpadPassword_Loaded() { }

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
    public bool HasWarped;

    public List<PuzzleElementControllerData> puzzleData = new List<PuzzleElementControllerData>();
    public List<bool> spawnableItems = new List<bool>();

    public LayoutLevelData(string layoutID)
    {
        LayoutID = layoutID;
    }

    public LayoutLevelData(CorridorLayoutHandler layoutHandler)
    {
        LayoutID = layoutHandler.LayoutID;
        List<bool> spawnablesTemp = new List<bool>();
        for (int i = 0; i < layoutHandler.SpawnableItems.Length; i++) spawnablesTemp.Add(false);
        spawnableItems = spawnablesTemp;
        
        //ensure items set as already picked up in the prefab are that way when the game is first saved
        var itemsNotToSpawn = layoutHandler.Pickups.Select((item, index) => new { item, index }).Where(x => x.item.PickedUp);
        if (itemsNotToSpawn != null) 
        {
            collectedItems.AddRange(itemsNotToSpawn.Select(x => x.index));
        }
    }

    public LayoutLevelData(string LayoutID, IEnumerable<int> collectedItems, IEnumerable<int> completedPuzzles, bool HasWarped, IEnumerable<PuzzleElementControllerData> puzzleData, IEnumerable<bool> spawnableItems) 
    {
        this.LayoutID = LayoutID;
        this.collectedItems = collectedItems.ToList();
        this.completedPuzzles = completedPuzzles.ToList();
        this.HasWarped = HasWarped;
        this.puzzleData = puzzleData.ToList();
        this.spawnableItems = spawnableItems.ToList();
    }
}

[System.Serializable]
public class LayoutLevelDataSerialized
{
    public string LayoutID;
    [XmlArrayAttribute]
    public int[] collectedItems;
    public int[] completedPuzzles;
    public bool HasWarped;
    public PuzzleElementControllerData[] puzzleData;
    public bool[] spawnableItems;

    public LayoutLevelData Deserialized
    {
        get
        {
            return new LayoutLevelData(LayoutID, collectedItems, completedPuzzles, HasWarped, puzzleData, spawnableItems);
        }
    }

    public LayoutLevelDataSerialized() { }

    public LayoutLevelDataSerialized(LayoutLevelData layoutLevelData)
    {
        LayoutID = layoutLevelData.LayoutID;
        collectedItems = layoutLevelData.collectedItems.ToArray();
        completedPuzzles = layoutLevelData.completedPuzzles.ToArray();
        HasWarped = layoutLevelData.HasWarped;
        puzzleData = layoutLevelData.puzzleData.ToArray();
        spawnableItems = layoutLevelData.spawnableItems.ToArray();
    }
}