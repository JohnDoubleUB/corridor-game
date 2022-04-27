using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingEventScript : CustomCorridorEventScript
{
    public Door[] endingDoors;
    public Animator cutsceneAnimator;
    public RadioInteractable cutsceneAudio;

    private void Start()
    {
        //foreach (Door endingDoor in endingDoors) endingDoor.DoorLocked = true;
        if (cutsceneAudio != null) cutsceneAudio.OnEndOfDialogue += OnRadioDialogueEnd;
    }
    public override void TriggerCustomEvent()
    {
        if (TriggerEvent()) 
        {
            //Lock all doors
            LockAllDoors();
            GameManager.current.playerController.EnableCutsceneMode(true);
            GameManager.current.ForceTVManEffect = true;
            if (cutsceneAnimator != null) cutsceneAnimator.Play("FinalCutscene", 0);
        }
    }

    private void LockAllDoors() 
    {
        foreach (Door sectionDoor in CorridorChangeManager.current.corridorDoorSegments) 
        {
            sectionDoor.ResetDoor();
            sectionDoor.DoorLocked = true;
            sectionDoor.openOnInteract = true;
        }
    }

    public void PlayCutsceneDialogue() 
    {
        if (cutsceneAudio != null) cutsceneAudio.ForceInteract(); 
    }

    private void OnRadioDialogueEnd() 
    {
        if (cutsceneAnimator != null) cutsceneAnimator.Play("FinalCutscene2", 0);
    }

    private void OnDestroy()
    {
        if (cutsceneAudio != null) cutsceneAudio.OnEndOfDialogue -= OnRadioDialogueEnd;
    }

    public void EndCutscene() 
    {
        GameManager.current.playerController.EnableCutsceneMode(false);
        GameManager.current.ForceTVManEffect = false;
    }
}
