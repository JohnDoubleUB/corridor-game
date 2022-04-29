using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTriggerVolume_Stay : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CorridorChangeManager.current.SaveGameOnLevel(20);
            GameManager.current.RestartCurrentScene();
        }
    }
}
