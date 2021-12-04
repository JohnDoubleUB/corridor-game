using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleElementNotifier : InteractableObject
{
    public int NotifierId;
    public Transform AssociatedTransform;
    public PuzzleElementController puzzleToNotify;

    protected override void OnInteract()
    {
        if (puzzleToNotify != null) puzzleToNotify.Notify(this);
    }
}
