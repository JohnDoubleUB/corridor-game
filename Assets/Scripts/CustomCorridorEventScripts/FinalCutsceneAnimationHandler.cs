using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalCutsceneAnimationHandler : MonoBehaviour
{
    [Range(0, 1f)]
    public float tvManEffectValue;
    public EndingEventScript endingEvent;
    public InteractableCandle candle;
    private void Update()
    {
        if (GameManager.current != null) GameManager.current.forcedEffectValue = tvManEffectValue;
    }

    public void ToggleCandle() 
    {
        if (candle != null) candle.ForceInteract();
    }
}
