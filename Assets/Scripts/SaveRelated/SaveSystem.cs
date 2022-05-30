using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml;
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
    public static bool VersioningMustMatch = true;
    public static bool PrefixAllXml = false;
    private static Guid encryptionGuid = Guid.Parse("ed1708258c5c43b688afc1aa926d5189");


    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;
    private static string NotepadSaveLocation => Application.persistentDataPath + "/" + SaveName + "." + NoteSaveExtension;
    private static string DebugSaveLocation => Application.persistentDataPath + "/" + DebugSaveName + "." + DebugSaveExtension;
    private static string AchievementSaveLocation => Application.persistentDataPath + "/" + AchievementSaveName + "." + AchievementSaveExtension;

    private static void _SaveDataToFile<TSerializableObject>(TSerializableObject serializableObject, string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();


        //File.Exists ensures that we replace the file if it already exists and don't encounter errors

        using (FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew))
        {
            formatter.Serialize(stream, serializableObject);
        }

        Debug.Log(serializableObject.GetType().ToString() + " File saved: " + path);
    }

    private static void _SaveDataToXMLFile<TSerializableObject>(TSerializableObject serializableObject, string path, bool useEncryption = false)
    {
        if (PrefixAllXml) path += ".xml";

        XmlSerializer serializer = new XmlSerializer(typeof(SerializedXmlWithMetaData<TSerializableObject>));

        using (Stream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew))
        {
            if (useEncryption)
            {
                using (Stream encryptedStream = _GetEncryptedXMLStream(stream, CryptoStreamMode.Write)) 
                {
                    serializer.Serialize(encryptedStream, new SerializedXmlWithMetaData<TSerializableObject>(serializableObject));
                }
            }
            else 
            {
                serializer.Serialize(stream, new SerializedXmlWithMetaData<TSerializableObject>(serializableObject));
            }

            //serializer.Serialize(stream, new SerializedXmlWithMetaData<TSerializableObject>(serializableObject));
        }

        Debug.Log(serializableObject.GetType().ToString() + " File saved: " + path);
    }

    private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
    {
        return attributes & ~attributesToRemove;
    }

    private static Stream _GetEncryptedXMLStream(Stream unEncryptedStream, CryptoStreamMode cryptoStreamMode)
    {
        byte[] encryptionGuidAsBytes = encryptionGuid.ToByteArray();

        AesCryptoServiceProvider aes = new AesCryptoServiceProvider()
        {
            Key = encryptionGuidAsBytes,
            IV = encryptionGuidAsBytes
        };


        ICryptoTransform cryptoTransform = cryptoStreamMode == CryptoStreamMode.Write ? aes.CreateEncryptor() : aes.CreateDecryptor();

        CryptoStream cryptoStream = new CryptoStream(
            unEncryptedStream,
            cryptoTransform,
            cryptoStreamMode
            );

        return cryptoStream; //return to use
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

    private static bool _TryLoadDataFromXMLFile<TSerializableObject>(string path, out TSerializableObject serializableObject, bool? versioningMustMatchOverride = null, bool useEncryption = false)
    {
        if (PrefixAllXml) path += ".xml";

        //Check if versioning is going to need to match
        bool versioningMustMatch = versioningMustMatchOverride != null && versioningMustMatchOverride.HasValue ? versioningMustMatchOverride.Value : VersioningMustMatch;

        if (File.Exists(path) && ReadValidObjectWithMetaData(out SerializedXmlWithMetaData<TSerializableObject> deserializedObjectWithMetaData))
        {
            serializableObject = deserializedObjectWithMetaData.SerializedObject;
            return true;
        }
        else
        {
            Debug.LogWarning("Valid Save File not found in " + path + ". VersioningMustMatch is set to: " + versioningMustMatch.ToString());
            serializableObject = default(TSerializableObject);
            return false;
        }

        //Returns true if the metadata meets the metadata requirements, i.e. matching versions
        bool ReadValidObjectWithMetaData(out SerializedXmlWithMetaData<TSerializableObject> deserializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedXmlWithMetaData<TSerializableObject>));

            bool success = false;

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                try
                {
                    if (useEncryption)
                    {
                        try
                        {
                            using (Stream encryptedStream = _GetEncryptedXMLStream(stream, CryptoStreamMode.Read))
                            {
                                deserializedObject = (SerializedXmlWithMetaData<TSerializableObject>)serializer.Deserialize(encryptedStream);
                            }

                            success = versioningMustMatch ? deserializedObject.Version == Application.version : true;
                        }
                        catch (Exception e) 
                        {
                            Debug.LogWarning("Failed to decrypt file: " + path + ". Could be that this file wasn't encrypted? New File will be created. Error: " + e);
                            deserializedObject = default(SerializedXmlWithMetaData<TSerializableObject>);
                        }
                    }
                    else 
                    {
                        deserializedObject = (SerializedXmlWithMetaData<TSerializableObject>)serializer.Deserialize(stream);
                        success = versioningMustMatch ? deserializedObject.Version == Application.version : true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to Deserialize file: " + path + " but exception was handled, Invalid XML, could be an old save file?, Error: " + e);
                    deserializedObject = default(SerializedXmlWithMetaData<TSerializableObject>);
                }
            }

            return success;
        }
    }

    //Main Game saves
    public static void SaveGame(SaveData saveData)
    {
        _SaveDataToXMLFile(saveData, SaveLocation, true);
    }

    public static SaveData LoadGame()
    {
        TryLoadGame(out SaveData savedData);
        return savedData;
    }

    public static bool TryLoadGame(out SaveData savedData)
    {
        return _TryLoadDataFromXMLFile(SaveLocation, out savedData, true, true);
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
        _SaveDataToXMLFile(testingData.Serialize(), DebugSaveLocation);
    }

    public static bool TryLoadTestingData(out CG_TestingData testingData)
    {
        bool dataLoaded = _TryLoadDataFromXMLFile(DebugSaveLocation, out CG_TestingData_Serialized testingDataSerialized);
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
        _SaveDataToXMLFile(new SerializableDictionary<string, bool>(achievementsData), AchievementSaveLocation, true);
    }

    public static Dictionary<string, bool> LoadAchievementsDictionary()
    {
        TryLoadAchievementsDictionary(out Dictionary<string, bool> achievementsData);
        return achievementsData;
    }

    public static bool TryLoadAchievementsDictionary(out Dictionary<string, bool> achievementsData)
    {
        bool result = _TryLoadDataFromXMLFile(AchievementSaveLocation, out SerializableDictionary<string, bool> serializedAchievementsData, true, true);
        achievementsData = result && serializedAchievementsData != null ? serializedAchievementsData.Deserialize() : null;
        return result;
    }
}

public enum GameLoadType
{
    New,
    Existing
}

public struct SerializedXmlWithMetaData<TSerializableObject>
{
    public string Version;
    public TSerializableObject SerializedObject;

    public SerializedXmlWithMetaData(TSerializableObject SerializedObject)
    {
        this.SerializedObject = SerializedObject;
        Version = Application.version;
    }
}