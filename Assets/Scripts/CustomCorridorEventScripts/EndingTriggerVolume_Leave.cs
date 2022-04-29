using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTriggerVolume_Leave : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            print("End game!");
            CorridorChangeManager.current.CreateNewSave();
            Application.Quit();
        }
    }
}
