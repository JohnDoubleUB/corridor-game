using UnityEngine;

public class EndingEventScript : CustomCorridorEventScript
{
    public Door[] endingDoors;
    public Animator cutsceneAnimator;
    public RadioInteractable cutsceneAudio;
    public TVManCutsceneScript tvManCutsceneScript;

    private void Start()
    {
        //foreach (Door endingDoor in endingDoors) endingDoor.DoorLocked = true;
        if (cutsceneAudio != null) 
        { 
            cutsceneAudio.OnEndOfDialogue += OnRadioDialogueEnd;
            cutsceneAudio.OnNextDialoguePart += OnNextDialoguePart;
        }
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

    private void OnNextDialoguePart(int partNo) 
    {
        int animationNumber = (partNo % 3) + 1;
        tvManCutsceneScript.PlayAnimation((CutsceneTVManAnimation)animationNumber);
    }

    private void OnRadioDialogueEnd() 
    {
        if (cutsceneAnimator != null) cutsceneAnimator.Play("FinalCutscene2", 0);
    }

    private void OnDestroy()
    {
        if (cutsceneAudio != null) 
        { 
            cutsceneAudio.OnEndOfDialogue -= OnRadioDialogueEnd;
            cutsceneAudio.OnNextDialoguePart -= OnNextDialoguePart;
        }
    }

    public void EndCutscene() 
    {
        GameManager.current.playerController.EnableCutsceneMode(false);
        GameManager.current.ForceTVManEffect = false;
        AchievementIntegrationManager.current.SetAchievement("ACH_THE_END");
    }

    public void PlayAnimation(CutsceneTVManAnimation cutsceneAnimation) 
    {
        if (tvManCutsceneScript != null) tvManCutsceneScript.PlayAnimation(cutsceneAnimation);
    }
}
