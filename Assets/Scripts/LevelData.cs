using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public CorridorLayoutHandler[] CorridorLayouts;
    public LevelSwitchTrigger[] CompleteLevelTriggerOnLayoutNumber;

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
}

[System.Serializable]
public class LevelSwitchTrigger 
{
    public string Name = "LevelChangeTrigger";
    public int LayoutNumberTrigger;
    public int LevelChange;
}