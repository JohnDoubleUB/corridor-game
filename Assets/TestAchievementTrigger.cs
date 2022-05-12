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

            //AchievementManager.current.TriggerAchievement("ACH_WIN_ONE_GAME");
            hasOccured = true;
        }

    }
}
