using UnityEngine;

public class EndingTriggerVolume_Stay : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            AchievementIntegrationManager.current.SetAchievement("ACH_STAY");
            CorridorChangeManager.current.SaveGameOnLevel(20, new EnabledMusicTracks().AssignAllTracksAs(true));
            GameManager.current.RestartCurrentScene();
        }
    }
}
