using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CorridorLayoutHandler : MonoBehaviour
{
    public int layoutLevelNumber;
    public int layoutNumber;

    public Door SectionDoor
    { 
        set 
        { 
            sectionDoor = value;
            if (PuzzleElements.Any() && PuzzleElements.Any(x => !x.PuzzleSolved)) 
            {
                sectionDoor.doorLocked = true;
                sectionDoor.openOnInteract = true;
            } 
        } 
    }
    public PropScript[] Props;

    public Door sectionDoor;

    public bool IsPuzzleSection { get { return PuzzleElements != null && PuzzleElements.Any(); } }

    private bool layoutIntitated;

    public PuzzleElementController[] PuzzleElements;
    private void Awake()
    {
        Props = GetComponentsInChildren<PropScript>();
        //InitiateLayout();
        foreach (PuzzleElementController puzzleElement in PuzzleElements) puzzleElement.LayoutHandler = this;
    }

    public void InitiateLayout(bool sectionIsFlipped = false) 
    {
        if (!layoutIntitated)
        {
            foreach (PropScript prop in Props)
            {
                GameObject trueChild = new GameObject(prop.name + "_TrueChild");
                trueChild.transform.SetParent(transform);
                trueChild.transform.position = prop.transform.position;
                prop.name = prop.name + "_FakeChild";
                prop.transform.SetParent(null);
                prop.FakeParent = trueChild.transform;
                prop.AccountForCorridorFlip(sectionIsFlipped); //Ensure meshes account for flip if needed
                //prop.transform.localScale = Vector3.one; //Ensure meshes aren't reversed
            }
            layoutIntitated = true;
        }
    }

    public void CheckPuzzleCompletion() 
    {
        if (!PuzzleElements.Any(x => !x.PuzzleSolved)) 
        {
            sectionDoor.doorLocked = false;
        }
    }
}
