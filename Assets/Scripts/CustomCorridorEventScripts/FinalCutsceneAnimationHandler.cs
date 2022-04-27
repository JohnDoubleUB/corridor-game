using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalCutsceneAnimationHandler : MonoBehaviour
{
    [Range(0, 1f)]
    public float tvManEffectValue;
    public EndingEventScript endingEvent;
    public InteractableCandle candle;

    public InteractableCandle[] afterCutsceneCandles;

    [Range(0, 1f)]
    public float playerScreenBrightness;
    public bool setPlayerScreenBrightness;

    public GameObject[] invisibleGameObjects;

    private void Update()
    {
        if (GameManager.current != null) GameManager.current.forcedEffectValue = tvManEffectValue;
        
        if (setPlayerScreenBrightness) 
        {
            GameManager.current.playerController.pSXMaterial.SetFloat("_FadeToWhite", playerScreenBrightness);
        }
    }

    public void ToggleCandle() 
    {
        if (candle != null) candle.ForceInteract();
    }

    public void StartDialogue() 
    {
        if (endingEvent != null) endingEvent.PlayCutsceneDialogue();
    }

    public void EndCutscene() 
    {
        if (endingEvent != null) endingEvent.EndCutscene();
    }

    public void RenderEndSigns() 
    {
        foreach (GameObject obj in invisibleGameObjects) obj.SetActive(true);
    }

    public void LightAfterCutsceneCandles() 
    {
        foreach (InteractableCandle afterCCandle in afterCutsceneCandles) afterCCandle.ForceInteract();
    }
}
