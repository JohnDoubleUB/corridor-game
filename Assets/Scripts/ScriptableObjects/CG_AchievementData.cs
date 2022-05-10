using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AchievementData", menuName = "ScriptableObjects/AchievementDataObject", order = 1)]
public class CG_AchievementData : ScriptableObject
{
    public string Identifier;
    public string NiceName;
    public Sprite UnlockedImage;
    public Sprite LockedImage;
    [TextArea(6, 10)]
    public string Description;
}
