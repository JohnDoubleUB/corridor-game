using UnityEngine;

public class EndingTriggerVolume_Leave : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            print("End game!");
            AchievementIntegrationManager.current.SetAchievement("ACH_LEAVE");
            CorridorChangeManager.current.CreateNewSave();
            Application.Quit();
        }
    }
}
