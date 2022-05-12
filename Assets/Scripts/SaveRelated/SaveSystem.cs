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



    //Achievement saving V2
    public static void SaveAchievementsDictionary(Dictionary<string, bool> achievementsData) 
    {
        _SaveDataToFile(new SerializableDictionary<string, bool>(achievementsData), AchievementSaveLocation);
    }

    public static Dictionary<string, bool> LoadAchievementsDictionary() 
    {
        TryLoadAchievementsDictionary(out Dictionary<string, bool> achievementsData);
        return achievementsData;
    }

    public static bool TryLoadAchievementsDictionary(out Dictionary<string, bool> achievementsData)
    {
        bool result = _TryLoadDataFromFile(AchievementSaveLocation, out SerializableDictionary<string, bool> serializedAchievementsData);
        achievementsData = result && serializedAchievementsData != null ? serializedAchievementsData.Deserialize() : null;
        return result;
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
public class SerializableDictionary<TSerializableKey, TSerializableValue>
{
    private _SerializedPair<TSerializableKey, TSerializableValue>[] serializedPairs;

    public Dictionary<TSerializableKey, TSerializableValue> Deserialize()
    {
        Dictionary<TSerializableKey, TSerializableValue> deserializedDict = new Dictionary<TSerializableKey, TSerializableValue>();
        foreach (_SerializedPair<TSerializableKey, TSerializableValue> pair in serializedPairs) deserializedDict.Add(pair.Key, pair.Value);
        return deserializedDict;
    }

    public SerializableDictionary(Dictionary<TSerializableKey, TSerializableValue> DictionaryToSerialize)
    {
        serializedPairs = DictionaryToSerialize.Select(x => new _SerializedPair<TSerializableKey, TSerializableValue>() { Key = x.Key, Value = x.Value }).ToArray();
    }

    [System.Serializable]
    struct _SerializedPair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}


