﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CorridorLayoutHandler : MonoBehaviour
{
    public int layoutLevelNumber;
    public int layoutNumber;

    private LevelData_Loaded levelData;
    private LayoutLevelData layoutData;

    private List<string> numberPadPasswords = new List<string>();

    private string layoutId = "";

    public string LayoutID
    {
        get
        {
            if (string.IsNullOrEmpty(layoutId)) layoutId = layoutLevelNumber + "_" + layoutNumber;
            return layoutId;
        }
    }

    public Door SectionDoor
    {
        set
        {
            sectionDoor = value;
            InitateDoorStatus();
        }
    }

    public Door sectionDoor;

    public PropScript[] Props;

    public bool IsPuzzleSection { get { return PuzzleElements != null && PuzzleElements.Any(); } }

    private bool layoutIntitated;

    public List<PuzzleElementController> PuzzleElements;
    public DecalClueObject[] DecalClueObjects;

    public PickupSpawn[] Pickups;

    private void Awake()
    {
        Props = GetComponentsInChildren<PropScript>();
    }

    private void Update()
    {
        if (layoutIntitated)
        {
            for (int i = 0; i < Pickups.Length; i++)
            {
                PickupSpawn currentPickup = Pickups[i];
                if (currentPickup.PickedUp) continue;
                if (currentPickup.SpawnedPickup.ParentChanged)
                {
                    currentPickup.PickedUp = true;
                    layoutData.collectedItems.Add(i);
                }

            }
        }
    }

    public void InitiateLayout(bool sectionIsFlipped, Door sectionDoor, LevelData_Loaded levelData)
    {
        if (!layoutIntitated)
        {
            this.sectionDoor = sectionDoor;
            this.levelData = levelData;
            layoutData = levelData.CorridorLayoutData.FirstOrDefault(x => x.LayoutID == LayoutID); // Repeats can confuse this



            //Setup prop parenting
            IntiatePropParenting(sectionIsFlipped);

            int numberPadCount = 0;
            PuzzleElementController puzzleElement;

            for (int i = 0; i < PuzzleElements.Count; i++)
            {
                puzzleElement = PuzzleElements[i];

                if (puzzleElement is NumberpadController && levelData != null)
                {
                    NumberpadController numberpadElement = (NumberpadController)puzzleElement;
                    NumberpadPassword_Loaded numberpadData = levelData.NumberpadData[numberPadCount];


                    numberpadElement.password = numberpadData.NumberpadPassword;

                    if(numberpadData.MissingCharacters != null) numberpadElement.disabledButtons = numberpadData.MissingCharacters.ToList();
                    numberPadCount++;
                    numberPadPasswords.Add(numberpadElement.password);
                }


                //NEW
                PuzzleElementControllerData puzzleData = layoutData.puzzleData.FirstOrDefault(x => x.PuzzleIndex == i);
                if (puzzleData != null) puzzleElement.LoadPuzzleData(puzzleData);

                puzzleElement.LayoutHandler = this;
            }


            foreach (DecalClueObject clueObject in DecalClueObjects)
            {
                //clueObject.clue = levelData.NumberpadPasswords[clueObject.ClueNumber];
                clueObject.clue = levelData.NumberpadData[clueObject.ClueNumber].NumberpadPassword;
            }

            PlacePickupables();
            InitateDoorStatus();

            layoutIntitated = true;
        }
    }

    public void CheckPuzzleCompletion()
    {
        IEnumerable<PuzzleElementController>[] levelTriggerSplitList = PuzzleElements.Partition(x => !x.IsLevelTrigger).ToArray();

        if (!levelTriggerSplitList[0].Any(x => !x.PuzzleSolved))
        {
            sectionDoor.DoorLocked = false;
        }

        //Any on this list AND none that match
        if (levelTriggerSplitList[1].AnyAndAllMatchPredicate(x => x.PuzzleSolved)) 
        {
            print("change level trigger!");
            if (levelData.GetIfLevelTriggerOnLayoutPuzzleCompleteAndReturnLevelChange(this, out int levelChange)) 
            {
                CorridorChangeManager.current.LevelChange(levelChange);
            }
        }
    }

    public void UpdatePuzzleData(PuzzleElementController puzzleElement = null, PuzzleElementControllerData puzzleData = null)
    {
        if (puzzleElement != null && puzzleData != null)
        {
            puzzleData.PuzzleIndex = PuzzleElements.IndexOf(puzzleElement);
            int indexToReplace = layoutData.puzzleData.FindIndex(x => x.PuzzleIndex == puzzleData.PuzzleIndex);

            if (indexToReplace == -1)
            {
                layoutData.puzzleData.Add(puzzleData);
            }
            else
            {
                layoutData.puzzleData[indexToReplace] = puzzleData;
            }
        }
    }

    private void PlacePickupables()
    {
        for (int i = 0; i < Pickups.Length; i++)
        {
            PickupSpawn pickup = Pickups[i];
            //TODO: Something here to make keys spawn
            if (pickup.PickupItemPrefab != null && pickup.PotentialSpawnPositions.Any())
            {
                if (layoutData.collectedItems.Contains(i) || pickup.PickupItemPrefab.pickupType == PickupType.Momento && !InventoryManager.current.AnyFreeMomentoSlots)
                {
                    pickup.PickedUp = true;
                }
                else
                {
                    pickup.SpawnedPickup = new PickupAndParent();
                    pickup.SpawnedPickup.Parent = pickup.PotentialSpawnPositions[UnityEngine.Random.Range(0, pickup.PotentialSpawnPositions.Length)];
                    pickup.SpawnedPickup.Pickup = Instantiate(pickup.PickupItemPrefab, pickup.SpawnedPickup.Parent);
                    pickup.SpawnedPickup.Pickup.transform.localPosition = Vector3.zero;
                    pickup.SpawnedPickup.Pickup.transform.localRotation = Quaternion.Euler(Vector3.zero);

                    //This works when keys are in the same level, if they're not then it doesn't work
                    //TODO: Unify this and store all missing keys over the entire game in a single ordered list? Also add fixed missing keys option
                    if (pickup.IsGeneratedNumberKey)
                    {
                        pickup.SpawnedPickup.Pickup.ObjectName = levelData.GeneratedNumberpadPieces[pickup.PuzzlePieceId].ToString();
                    }

                }


            }
        }
    }


    private void InitateDoorStatus()
    {
        if(PuzzleElements.AnyAndAnyMatchPredicate(x => !x.PuzzleSolved && !x.IsLevelTrigger))
        //if (PuzzleElements.Any() && PuzzleElements.Any(x => !x.PuzzleSolved))
        {
            sectionDoor.DoorLocked = true;
            sectionDoor.openOnInteract = true;
        }
    }

    private void IntiatePropParenting(bool sectionIsFlipped)
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
        }
    }

}

[System.Serializable]
public class PickupSpawn
{
    public PickupableInteractable PickupItemPrefab;
    public Transform[] PotentialSpawnPositions;
    public PickupAndParent SpawnedPickup;
    public bool PickedUp;
    public bool IsGeneratedNumberKey;
    public int PuzzlePieceId = -1;
}

public struct PickupAndParent
{
    public InteractableObject Pickup;
    public Transform Parent;

    public bool ParentChanged
    {
        get
        {
            return Pickup.transform.parent != Parent;
        }
    }
}
