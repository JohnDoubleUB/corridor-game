using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static GameLoadType LoadType = GameLoadType.Existing;
    public static GameLoadType NotepadLoadType = GameLoadType.Existing;
    public static readonly string SaveExtension = "bathosave";
    public static readonly string NoteSaveExtension = "bathonotes";
    public static readonly string DebugSaveExtension = "bathodebug";
    public static readonly string AchievementSaveExtension = "bathoach";
    public static string SaveName = "PlayerData";
    public static string DebugSaveName = "DebugData";
    public static string AchievementSaveName = "Achievements";

    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;
    private static string NotepadSaveLocation => Application.persistentDataPath + "/" + SaveName + "." + NoteSaveExtension;
    private static string DebugSaveLocation => Application.persistentDataPath + "/" + DebugSaveName + "." + DebugSaveExtension;
    private static string AchievementSaveLocation => Application.persistentDataPath + "/" + AchievementSaveName + "." + AchievementSaveExtension;


    private static void _SaveDataToFile<TSerializableObject>(TSerializableObject serializableObject, string path) 
    {
        BinaryFormatter formatter = new BinaryFormatter();

        //File.Exists ensures that we replace the file if it already exists and don't encounter errors
        FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew);


        formatter.Serialize(stream, serializableObject);
        stream.Close();

        Debug.Log(serializableObject.GetType().ToString() + " File saved: " + path);
    }
    private static bool _TryLoadDataFromFile<TSerializableObject>(string path, out TSerializableObject serializableObject)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            serializableObject = (TSerializableObject)formatter.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            serializableObject = default(TSerializableObject);
            return false;
        }
    }

    //Main Game saves
    public static void SaveGame(SaveData saveData)
    {
        _SaveDataToFile(saveData, SaveLocation);
    }

    public static SaveData LoadGame()
    {
        TryLoadGame(out SaveData savedData);
        return savedData;
    }

    public static bool TryLoadGame(out SaveData savedData)
    {
        return _TryLoadDataFromFile(SaveLocation, out savedData);
    }

    //Notepad saves
    public static void SaveNotepad(NotepadData notepadData)
    {

        _SaveDataToFile(notepadData, NotepadSaveLocation);
    }

    public static NotepadData LoadNotepad()
    {
        TryLoadNotepad(out NotepadData notepadData);
        return notepadData;
    }

    public static bool TryLoadNotepad(out NotepadData notepadData)
    {
        return _TryLoadDataFromFile(NotepadSaveLocation, out notepadData);
    }

    //Test data saving
    public static void SaveTestingData(CG_TestingData testingData)
    {
        _SaveDataToFile(testingData.Serialize(), DebugSaveLocation);
    }

    public static bool TryLoadTestingData(out CG_TestingData testingData)
    {
        bool dataLoaded = _TryLoadDataFromFile(DebugSaveLocation, out CG_TestingData_Serialized testingDataSerialized);
        testingData = testingDataSerialized.Deserialize();
        return dataLoaded;
    }

    public static CG_TestingData LoadTestingData()
    {
        TryLoadTestingData(out CG_TestingData testingData);
        return testingData;
    }

    //Achievement saving
    public static void SaveAchievements(AchievementSaveData achievementSaveData) 
    {
        _SaveDataToFile(achievementSaveData, AchievementSaveLocation);
    }

    public static AchievementSaveData LoadAchievements() 
    {
        TryLoadAchievements(out AchievementSaveData achievementSaveData);
        return achievementSaveData;
    }

    public static bool TryLoadAchievements(out AchievementSaveData achievementSaveData) 
    {
        return _TryLoadDataFromFile(AchievementSaveLocation, out achievementSaveData);
    }
}




public enum GameLoadType
{
    New,
    Existing
}

[System.Serializable]
public class SaveData
{
    public int CurrentLevel;
    public LevelData_Serialized[] LoadedLevels;
    public PlayerData PlayerData;
    public InventoryData InventoryData;
    public TVManData TVManData;
    public EnabledMusicTracks EnabledTracks;
    public string[] EventTags;
    public SaveData(IEnumerable<LevelData_Loaded> LoadedLevels)
    {
        this.LoadedLevels = LoadedLevels.Select(x => new LevelData_Serialized(x)).ToArray();
    }

    public SaveData(IEnumerable<LevelData_Loaded> LoadedLevels, PlayerData PlayerData) : this(LoadedLevels)
    {
        this.PlayerData = PlayerData;
    }

    public SaveData(IEnumerable<LevelData_Loaded> LoadedLevels, PlayerData PlayerData, InventoryData InventoryData, int CurrentLevel = 0) : this(LoadedLevels, PlayerData)
    {
        this.InventoryData = InventoryData;
        this.CurrentLevel = CurrentLevel;
    }

    public SaveData(IEnumerable<LevelData_Loaded> LoadedLevels, PlayerData PlayerData, InventoryData InventoryData, TVManData TVManData, EnabledMusicTracks EnabledTracks, IEnumerable<string> EventTags, int CurrentLevel = 0) : this(LoadedLevels, PlayerData, InventoryData, CurrentLevel)
    {
        this.TVManData = TVManData;
        this.EventTags = EventTags.ToArray();
        this.EnabledTracks = EnabledTracks;
    }
}

[System.Serializable]
public class PlayerData
{
    public bool NotepadPickedUp; //Stores whether the player has picked up the notepad

    public PlayerData(CG_CharacterController characterController)
    {
        NotepadPickedUp = characterController.NotepadPickedUp;
    }
}

[System.Serializable]
public class TVManData
{
    public bool MomentoDelayActive;
    public float CurrentMomentoDelayTimer;

    public TVManData(TVManController tvManController) 
    {
        MomentoDelayActive = tvManController.MomentoEffectActive;
        CurrentMomentoDelayTimer = tvManController.CurrentMomentoDelayTimer;
    }
}

[System.Serializable]
public class NotepadData
{
    public NotepadLineData[] LineData;

    public NotepadData(IEnumerable<LineRenderer> lineRenderers)
    {
        LineData = lineRenderers.Where(x => x != null).Select(lineRenderer =>
        {
            Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(linePoints);
            return new NotepadLineData(linePoints.Select(x => x.Serialized()), lineRenderer.transform.localRotation.eulerAngles.Serialized(), lineRenderer.transform.localScale.Serialized());
        }).ToArray();
    }
}

[System.Serializable]
public struct Vector3Serialized
{
    public float x;
    public float y;
    public float z;

    public Vector3 Deserialized()
    {
        return new Vector3(x, y, z);
    }

    public Vector3Serialized(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}

[System.Serializable]
public struct NotepadLineData
{
    public Vector3Serialized[] Positions;
    public Vector3Serialized LocalScale;
    public Vector3Serialized LocalRotationEuler;

    public NotepadLineData(IEnumerable<Vector3Serialized> Positions, Vector3Serialized LocalRotationEuler, Vector3Serialized LocalScale)
    {
        this.Positions = Positions.ToArray();
        this.LocalRotationEuler = LocalRotationEuler;
        this.LocalScale = LocalScale;
    }
}

[System.Serializable]
public struct AchievementData
{
    public string Identifier;
    public bool Achieved;
}

[System.Serializable]
public class AchievementSaveData
{
    public AchievementData[] AchievementData;

    public AchievementSaveData(IEnumerable<string> identifiers)
    {
        if (identifiers != null && identifiers.Any()) AchievementData = identifiers.Select(x => new AchievementData() { Identifier = x, Achieved = false }).ToArray();
    }

    public AchievementSaveData(IEnumerable<Steamworks.Data.Achievement> achievements)
    {
        if (achievements != null && achievements.Any()) AchievementData = ConvertSteamAchievementsToAchievementData(achievements).ToArray();
    }

    private IEnumerable<AchievementData> ConvertSteamAchievementsToAchievementData(IEnumerable<Steamworks.Data.Achievement> achievements)
    {
        return achievements.Select(x => new AchievementData() { Identifier = x.Identifier, Achieved = x.State });
    }

    public void UpdateAchievementDataFromSteam(IEnumerable<Steamworks.Data.Achievement> achievements)
    {
        if (achievements != null && achievements.Any() && achievements.Count() > AchievementData.Length)
        {
            AchievementData = ConvertSteamAchievementsToAchievementData(achievements).Union(AchievementData, new AchievementDataComparer()).ToArray();
        }
    }

    public void UpdateAchievementByIdentifier(string identifier, bool setAchieved = true) 
    {
        if (AchievementData != null)
        {
            for (int i = 0; i < AchievementData.Length; i++)
            {
                if (AchievementData[i].Identifier == identifier) AchievementData[i].Achieved = setAchieved;
            }
        }
    }
}

public class AchievementDataComparer : IEqualityComparer<AchievementData>
{
    public bool Equals(AchievementData x, AchievementData y)
    {
        return x.Identifier == y.Identifier;
    }

    public int GetHashCode(AchievementData obj)
    {
        return obj.Identifier.GetHashCode();
    }
}


