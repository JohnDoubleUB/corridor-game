using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleElementController : MonoBehaviour
{
    public CorridorLayoutHandler LayoutHandler;
    public bool IsLevelTrigger;

    private bool puzzleSolved;
    public bool PuzzleSolved 
    {
        get 
        { 
            return puzzleSolved; 
        }
        set 
        { 
            puzzleSolved = value;
            if (puzzleSolved && LayoutHandler != null) 
            {
                OnPuzzleUpdated();
                LayoutHandler.CheckPuzzleCompletion();
            }
        }
    }

    public virtual void Notify() 
    {
        print("Not implemented!");
    }

    public virtual void LoadPuzzleData(PuzzleElementControllerData puzzleData) 
    {
        PuzzleSolved = puzzleData.PuzzleSolved;
    }

    public virtual void OnPuzzleUpdated() 
    {
        LayoutHandler.UpdatePuzzleData(this, this);
    }
}

public class PuzzleElementControllerData
{
    public int PuzzleIndex;
    public bool PuzzleSolved;

    public PuzzleElementControllerData(PuzzleElementController puzzleElementController) 
    {
        PuzzleSolved = puzzleElementController.PuzzleSolved;
    }

    public static implicit operator PuzzleElementControllerData(PuzzleElementController puzzleElementController) 
    {
        return new PuzzleElementControllerData(puzzleElementController);
    }
}