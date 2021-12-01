using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleElementNotifier : InteractableObject
{
    public PuzzleElementController puzzleToNotify;

    protected override void OnInteract()
    {
        if (puzzleToNotify != null) puzzleToNotify.Notify();
    }
}
