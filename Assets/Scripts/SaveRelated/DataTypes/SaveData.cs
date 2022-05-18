using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

[System.Serializable] [XmlRootAttribute("SaveData")]
public class SaveData
{
    public int CurrentLevel;
    public LevelData_Serialized[] LoadedLevels; //Check checked
    public PlayerData PlayerData;
    public InventoryData InventoryData; //Check
    public TVManData TVManData;
    public EnabledMusicTracks EnabledTracks;
    public string[] EventTags;

    public SaveData() { }

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
