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
            if (PuzzleElements.Any() && PuzzleElements.Any(x => !x.PuzzleSolved)) 
            {
                sectionDoor.DoorLocked = true;
                sectionDoor.openOnInteract = true;
            } 
        } 
    }

    public Door sectionDoor;

    public PropScript[] Props;

    public bool IsPuzzleSection { get { return PuzzleElements != null && PuzzleElements.Any(); } }

    private bool layoutIntitated;

    public PuzzleElementController[] PuzzleElements;
    public DecalClueObject[] DecalClueObjects;

    public PickupSpawn[] Pickups;

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
            }


            foreach (DecalClueObject clueObject in DecalClueObjects)
            {
                print("set clue to: " + levelData.NumberpadPasswords[clueObject.ClueNumber]);
                clueObject.clue = levelData.NumberpadPasswords[clueObject.ClueNumber];
            }

            PlacePickupables();

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

    private void PlacePickupables() 
    {
        foreach (PickupSpawn pickup in Pickups)
        {
            if (pickup.PickupItemPrefab != null && pickup.PotentialSpawnPositions.Any())
            {
                pickup.SpawnedPickup = new PickupAndParent();
                pickup.SpawnedPickup.Parent = pickup.PotentialSpawnPositions[UnityEngine.Random.Range(0, pickup.PotentialSpawnPositions.Length)];
                pickup.SpawnedPickup.Pickup = Instantiate(pickup.PickupItemPrefab, pickup.SpawnedPickup.Parent);
                pickup.SpawnedPickup.Pickup.transform.localPosition = Vector3.zero;
                pickup.SpawnedPickup.Pickup.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }
}

[System.Serializable]
public class PickupSpawn 
{
    public InteractableObject PickupItemPrefab;
    public Transform[] PotentialSpawnPositions;
    public PickupAndParent SpawnedPickup;
}

public struct PickupAndParent
{
    public InteractableObject Pickup;
    public Transform Parent;
}
