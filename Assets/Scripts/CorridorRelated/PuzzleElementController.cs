using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleElementController : MonoBehaviour
{
    public CorridorLayoutHandler LayoutHandler;
   // public Door DoorToLock;

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
            if(puzzleSolved && LayoutHandler != null) LayoutHandler.CheckPuzzleCompletion(this);
        }
    }

    public virtual void GenerateRandomSolution() 
    {
        print("Not implemented!");
    }
}
