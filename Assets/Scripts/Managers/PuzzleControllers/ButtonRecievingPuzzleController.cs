using UnityEngine;

public abstract class ButtonRecievingPuzzleController : PuzzleElementController
{
    public abstract void InputCharacter(char character, Transform inputLocation = null);
}
