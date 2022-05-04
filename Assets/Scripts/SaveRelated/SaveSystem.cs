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
    public static string SaveName = "PlayerData";
    public static string DebugSaveName = "DebugData";

    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;
    private static string NotepadSaveLocation => Application.persistentDataPath + "/" + SaveName + "." + NoteSaveExtension;

    private static string DebugSaveLocation => Application.persistentDataPath + "/" + DebugSaveName + "." + DebugSaveExtension;

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

    public static void SaveTestingData(CG_TestingData testingData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = DebugSaveLocation;

        //File.Exists ensures that we replace the file if it already exists and don't encounter errors
        FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew);


        formatter.Serialize(stream, testingData.Serialize());
        stream.Close();

        Debug.Log("Debug File saved: " + path);
    }

    public static bool TryLoadTestingData(out CG_TestingData testingData)
    {
        string path = DebugSaveLocation;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            testingData = ((CG_TestingData_Serialized)formatter.Deserialize(stream)).Deserialize();
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            testingData = null;
            return false;
        }
    }

    public static CG_TestingData LoadTestingData()
    {
        TryLoadTestingData(out CG_TestingData testingData);
        return testingData;
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


