﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CorridorLayoutHandler : MonoBehaviour
{
    public int layoutLevelNumber;
    public int layoutNumber;

    public CorridorMeshType corridorMeshType;
    public CorridorMatType corridorMatType;
    public CorridorMatType corridorDoorMatType;

    private LevelData_Loaded levelData;
    private IEnumerable<LevelData_Loaded> allLevelData;
    
    [HideInInspector]
    public LayoutLevelData LayoutData;

    private List<string> numberPadPasswords = new List<string>();

    private string layoutId = "";

    public float StretchAmount = 2;
    public bool ForceStretch;

    public bool ForceWave;

    private CustomCorridorEventScript[] customEvents;

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

    [ReadOnlyField]
    public Door sectionDoor;

    public bool AllowTVMan = true;
    public bool AllowMouseSpawns = true;

    public PropScript[] Props;

    public bool IsPuzzleSection { get { return PuzzleElements != null && PuzzleElements.Any(); } }

    private bool layoutIntitated;

    public List<PuzzleElementController> PuzzleElements;
    public DecalClueObject[] DecalClueObjects;

    public PickupSpawn[] Pickups;
    public PrefabAndSpawnStatus[] SpawnableItems;

    private void Awake()
    {
        Props = GetComponentsInChildren<PropScript>();
        customEvents = GetComponents<CustomCorridorEventScript>();
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
                    print("parent changed!");
                    currentPickup.PickedUp = true;
                    LayoutData.collectedItems.Add(i);
                }

            }
        }
    }

    private void SetupSpawnablePrefabs() 
    {
        if (LayoutData.spawnableItems.Any() && SpawnableItems.Any())
        {
            print("Spawning");
            for (int i = 0; i < LayoutData.spawnableItems.Count && i < SpawnableItems.Length; i++) 
            {
                if (LayoutData.spawnableItems[i]) 
                {
                    PrefabAndSpawnStatus itemToBeSpawned = SpawnableItems[i];
                    print("Spawning " + itemToBeSpawned.Prefab.name);
                    GameObject spawnedItem = Instantiate(itemToBeSpawned.Prefab, itemToBeSpawned.SpawnLocation.position, itemToBeSpawned.SpawnLocation.rotation, itemToBeSpawned.SpawnLocation);
                    spawnedItem.transform.SetParent(itemToBeSpawned.SpawnLocation);
                    //spawnedItem.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    //spawnedItem.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    public void InitiateLayout(bool sectionIsFlipped, Door sectionDoor, LevelData_Loaded currentLevelData, IEnumerable<LevelData_Loaded> allLevelData)
    {
        if (!layoutIntitated)
        {
            this.sectionDoor = sectionDoor;
            levelData = currentLevelData;
            this.allLevelData = allLevelData;
            LayoutData = currentLevelData.CorridorLayoutData.FirstOrDefault(x => x.LayoutID == LayoutID); // Repeats can confuse this
            SetupSpawnablePrefabs();
            


            //Setup prop parenting
            IntiatePropParenting(sectionIsFlipped);

            int numberPadCount = 0;
            PuzzleElementController puzzleElement;

            for (int i = 0; i < PuzzleElements.Count; i++)
            {
                puzzleElement = PuzzleElements[i];

                if (puzzleElement is NumberpadController && currentLevelData != null)
                {
                    NumberpadController numberpadElement = (NumberpadController)puzzleElement;
                    NumberpadPassword_Loaded numberpadData = currentLevelData.NumberpadData[numberPadCount];


                    numberpadElement.password = numberpadData.NumberpadPassword;

                    if(numberpadData.MissingCharacters != null) numberpadElement.disabledButtons = numberpadData.MissingCharacters.ToList();
                    numberPadCount++;
                    numberPadPasswords.Add(numberpadElement.password);
                }


                //NEW
                PuzzleElementControllerData puzzleData = LayoutData.puzzleData.FirstOrDefault(x => x.PuzzleIndex == i);
                if (puzzleData != null) puzzleElement.LoadPuzzleData(puzzleData);

                puzzleElement.LayoutHandler = this;
            }

            foreach (DecalClueObject clueObject in DecalClueObjects)
            {
                LevelData_Loaded clueLevel = !clueObject.IsForCurrentLevel && allLevelData.Any() ? allLevelData.FirstOrDefault(x => x.LevelNumber == clueObject.ClueLevel) : currentLevelData;

                //clueObject.clue = levelData.NumberpadPasswords[clueObject.ClueNumber];
                if (clueLevel != null) clueObject.clue = clueLevel.NumberpadData[clueObject.ClueNumber].NumberpadPassword;
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
            int indexToReplace = LayoutData.puzzleData.FindIndex(x => x.PuzzleIndex == puzzleData.PuzzleIndex);

            if (indexToReplace == -1)
            {
                LayoutData.puzzleData.Add(puzzleData);
            }
            else
            {
                LayoutData.puzzleData[indexToReplace] = puzzleData;
            }
        }
    }

    private void PlacePickupables()
    {
        for (int i = 0; i < Pickups.Length; i++)
        {
            PickupSpawn pickup = Pickups[i];
            if (pickup.PickupItemPrefab != null && pickup.PotentialSpawnPositions.Any())
            {
                if (LayoutData.collectedItems.Contains(i) || pickup.PickupItemPrefab.pickupType == PickupType.Momento && !InventoryManager.current.AnyFreeMomentoSlots)
                {
                    pickup.PickedUp = true;
                }
                else
                {
                    print("placing pickup " + pickup.PickupItemPrefab.name);
                    pickup.PickedUp = false; //I think this will fix the issue
                    pickup.SpawnedPickup = new PickupAndParent();
                    pickup.SpawnedPickup.Parent = pickup.PotentialSpawnPositions[UnityEngine.Random.Range(0, pickup.PotentialSpawnPositions.Length)];
                    pickup.SpawnedPickup.Pickup = Instantiate(pickup.PickupItemPrefab, pickup.SpawnedPickup.Parent);
                    pickup.SpawnedPickup.Pickup.transform.localPosition = Vector3.zero;
                    pickup.SpawnedPickup.Pickup.transform.localRotation = Quaternion.Euler(Vector3.zero);

                    //This works when keys are in the same level, if they're not then it doesn't work
                    //TODO: Add fixed missing keys option
                    if (pickup.IsGeneratedNumberKey)
                    {
                        LevelData_Loaded numberpadPieceLevel = pickup.IsForCurrentLevel ? levelData : allLevelData.FirstOrDefault(x => x.LevelNumber == pickup.LevelNumber);

                        if(numberpadPieceLevel != null) pickup.SpawnedPickup.Pickup.ObjectName = numberpadPieceLevel.GeneratedNumberpadPieces[pickup.PuzzlePieceId].ToString();
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

    public void OnEnterCustomScripts() 
    {
        if (customEvents.Any())
        {
            foreach (CustomCorridorEventScript cEvents in customEvents)
                cEvents.TriggerCustomEvent();
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
    public int LevelNumber = -1;
    public bool IsForCurrentLevel { get { return LevelNumber == -1; } }
}

[System.Serializable]
public class PrefabAndSpawnStatus 
{
    public GameObject Prefab;
    public Transform SpawnLocation;
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

public enum CorridorMeshType 
{
    Default,
    Office
}

public enum CorridorMatType 
{
    Default,
    Office1,
    Office2,
    Office3
}