using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
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

    private static void _SaveDataToXMLFile<TSerializableObject>(TSerializableObject serializableObject, string path)
    {
        path += ".xml";
        XmlSerializer serializer = new XmlSerializer(typeof(TSerializableObject));

        //File.Exists ensures that we replace the file if it already exists and don't encounter errors
        FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew);

        serializer.Serialize(stream, serializableObject);
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
            Debug.LogWarning("Save file not found in " + path + " , Please create a save before trying to load.");
            serializableObject = default(TSerializableObject);
            return false;
        }
    }

    private static bool _TryLoadDataFromXMLFile<TSerializableObject>(string path, out TSerializableObject serializableObject)
    {
        path += ".xml";

        if (File.Exists(path))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TSerializableObject));

            FileStream stream = new FileStream(path, FileMode.Open);

            serializableObject = (TSerializableObject)serializer.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogWarning("Save file not found in " + path + " , Please create a save before trying to load.");
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
        _SaveDataToXMLFile(notepadData, NotepadSaveLocation);
    }

    public static NotepadData LoadNotepad()
    {
        TryLoadNotepad(out NotepadData notepadData);
        return notepadData;
    }

    public static bool TryLoadNotepad(out NotepadData notepadData)
    {
        return _TryLoadDataFromXMLFile(NotepadSaveLocation, out notepadData);
    }

    //Test data saving
    public static void SaveTestingData(CG_TestingData testingData)
    {
        _SaveDataToFile(testingData.Serialize(), DebugSaveLocation);
    }

    public static bool TryLoadTestingData(out CG_TestingData testingData)
    {
        bool dataLoaded = _TryLoadDataFromFile(DebugSaveLocation, out CG_TestingData_Serialized testingDataSerialized);
        testingData = testingDataSerialized != null ? testingDataSerialized.Deserialize() : null;
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



public class SimpleTestData 
{
    public string testOne;
    public int testTwo;
}