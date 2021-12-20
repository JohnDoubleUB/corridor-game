using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadlockController : PuzzleElementController
{
    public PuzzleElementNotifier PadlockNotifier;
    public InteractableObject RequiredObject;
    float pickupSpeedMultiplier = 1.5f;

    private void Awake()
    {
        if (PadlockNotifier != null) PadlockNotifier.puzzleToNotify = this;
    }


    public override void Notify(PuzzleElementNotifier notifier = null)
    {
        print("lalala");
    }
}
