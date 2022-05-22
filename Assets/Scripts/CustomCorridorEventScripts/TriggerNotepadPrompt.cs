using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNotepadPrompt : MonoBehaviour
{
    public bool promptShouldLinger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            GameManager.current.playerController.DisplayNotepadPrompt(promptShouldLinger);
        }
    }
}
