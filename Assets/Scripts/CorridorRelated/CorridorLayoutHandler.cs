using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CorridorLayoutHandler : MonoBehaviour
{
    public int layoutLevelNumber;
    public int layoutNumber;

    private LevelData_Loaded levelData;

    private List<string> numberPadPasswords = new List<string>();

    public Door SectionDoor
    { 
        set 
        { 
            sectionDoor = value;
            if (PuzzleElements.Any() && PuzzleElements.Any(x => !x.PuzzleSolved)) 
            {
                sectionDoor.DoorLocked = true;
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
    }

    public void InitiateLayout(bool sectionIsFlipped, LevelData_Loaded levelData) 
    {
        if (!layoutIntitated)
        {
            this.levelData = levelData;
            
            //Setup prop parenting
            foreach (PropScript prop in Props)
            {
                GameObject trueChild = new GameObject(prop.name + "_TrueChild");
                trueChild.transform.SetParent(transform);
                trueChild.transform.position = prop.transform.position;
                prop.name = prop.name + "_FakeChild";
                prop.transform.SetParent(null);
                prop.FakeParent = trueChild.transform;
                prop.AccountForCorridorFlip(sectionIsFlipped); //Ensure meshes account for flip if needed
            }

            int numberPadCount = 0;

            //Setup codes
            foreach (PuzzleElementController puzzleElement in PuzzleElements)
            {
                puzzleElement.LayoutHandler = this;

                //Set the passwords and store in the layout!
                if (puzzleElement is NumberpadController && levelData != null)
                {
                    NumberpadController numberpadElement = (NumberpadController)puzzleElement;
                    numberpadElement.password = levelData.NumberpadPasswords[numberPadCount];
                    numberPadCount++;
                    print("password is: " + numberpadElement.password);
                    numberPadPasswords.Add(numberpadElement.password);
                }

                //TODO: set some new thing that displays the code somewhere!


            }


            layoutIntitated = true;
        }
    }

    public void CheckPuzzleCompletion() 
    {
        if (!PuzzleElements.Any(x => !x.PuzzleSolved)) 
        {
            sectionDoor.DoorLocked = false;
        }
    }
}
