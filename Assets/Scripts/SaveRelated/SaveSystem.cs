using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static GameLoadType LoadType = GameLoadType.Existing;
    public static readonly string SaveExtension = "bathosave";
    public static readonly string NoteSaveExtension = "bathonotes";
    public static string SaveName = "PlayerData";

    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;
    private static string NotepadSaveLocation => Application.persistentDataPath + "/" + SaveName + "." + NoteSaveExtension;

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
        TryLoadGame(out SaveData savedData);
        return savedData;
    }

    public static bool TryLoadGame(out SaveData savedData)
    {
        string path = SaveLocation;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            savedData = (SaveData)formatter.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            savedData = null;
            return false;
        }
    }



    public static void SaveNotepad(NotepadData notepadData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = NotepadSaveLocation;

        //File.Exists ensures that we replace the file if it already exists and don't encounter errors
        FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew);


        formatter.Serialize(stream, notepadData);
        stream.Close();

        Debug.Log("File saved: " + path);
    }

    public static NotepadData LoadNotepad()
    {
        TryLoadNotepad(out NotepadData notepadData);
        return notepadData;
    }

    public static bool TryLoadNotepad(out NotepadData notepadData)
    {
        string path = NotepadSaveLocation;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            notepadData = (NotepadData)formatter.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            notepadData = null;
            return false;
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
    public int CurrentLevel;
    public LevelData_Serialized[] LoadedLevels;
    public PlayerData PlayerData;
    public InventoryData InventoryData;
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
public class NotepadData
{
    public NotepadLineData[] LinePositions;

    public NotepadData(IEnumerable<LineRenderer> lineRenderers) 
    {
        //Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
        //lineRenderer.GetPositions(linePoints);
        //NotepadLineData resulttest = new NotepadLineData(linePoints.Select(x => x.Serialized()));
        
        //IEnumerable<LineRenderer> lineRenderers;

        LinePositions = lineRenderers.Select(lineRenderer =>
        {
            Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(linePoints);
            return new NotepadLineData(linePoints.Select(x => x.Serialized()));
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

    public NotepadLineData(IEnumerable<Vector3Serialized> Positions) 
    {
        this.Positions = Positions.ToArray();
    }
}


