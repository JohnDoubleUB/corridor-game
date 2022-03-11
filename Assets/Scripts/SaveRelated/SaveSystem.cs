using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static GameLoadType LoadType;
    public static readonly string SaveExtension = "bathosave";
    public static string SaveName = "PlayerData";

    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;

    public static void SaveGame(SaveData saveData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = SaveLocation;

        //File.Exists ensures that we replace the file if it already exists and don't encounter errors
        FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew);


        formatter.Serialize(stream, saveData);
        stream.Close();

        Debug.Log("File saved: " + path);
    }

    public static SaveData LoadGame()
    {
        string path = SaveLocation;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData savedData = (SaveData)formatter.Deserialize(stream);
            stream.Close();

            return savedData;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
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
    public LevelData_Serialized[] LoadedLevels;

    public SaveData(IEnumerable<LevelData_Loaded> LoadedLevels) 
    {
        this.LoadedLevels = LoadedLevels.Select(x => new LevelData_Serialized(x)).ToArray();
    }

}