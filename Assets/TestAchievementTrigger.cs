using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAchievementTrigger : MonoBehaviour
{
    private bool hasOccured = false;
    private void OnTriggerEnter(Collider other)
    {
        print("hiii");
        if (!hasOccured)
        {
            AchievementIntegrationManager.current.SetAchievement("ACH_WIN_ONE_GAME");
            hasOccured = true;
        }
        else 
        {
            AchievementIntegrationManager.current.SetAchievement("ACH_WIN_ONE_GAME", false);
            hasOccured = false;
        }

    }
}
