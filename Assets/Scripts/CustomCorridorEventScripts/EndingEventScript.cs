using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingEventScript : CustomCorridorEventScript
{
    public Door[] endingDoors;
    public Animator cutsceneAnimator;

    private void Start()
    {
        foreach (Door endingDoor in endingDoors) endingDoor.DoorLocked = true;
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
}
