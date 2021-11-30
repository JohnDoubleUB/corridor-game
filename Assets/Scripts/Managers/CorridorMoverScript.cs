using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorMoverScript : MonoBehaviour
{
    public List<CorridorSection> corridorSections;
    public List<Door> corridorDoorSegments;

    public CorridorLayoutHandler[] corridorRandomPrefabs;
    public bool OnlyUseRandomAssortedCorridorLayouts;

    public List<LevelData> Levels;
    private List<LevelData_Loaded> LoadedLevels;

    public int CurrentLevel = 1;
    private int currentLevelChangeTracking;

    public CorridorLayoutHandler[] levelCorridorPrefabs;
    public CorridorLayoutHandler[] levelCorridorBackwardPrefabs;

    CorridorSection[] GetOrderedSections { get { return corridorSections.OrderByDescending(x => x.transform.position.x).ToArray(); } }

    Door[] GetOrderedDoorSegments { get { return corridorDoorSegments.OrderByDescending(x => x.transform.position.x).ToArray(); } }

    private Transform playerTransform;
    private float stretchTimer;
    private float stretchTarget = 2;
    private float initialStretch = 1;
    private float stretchSpeed = 0.5f;
    private CorridorSection sectionToStretch;

    private float wavyTimer;
    private float wavyTarget = 1;
    private float intitialWavy = 0;
    private float wavySpeed = 1;
    private float currentWavy;
    private CorridorSection sectionToMakeWavy;
    private Door[] doorsToMakeWavy = new Door[0];

    private GameObject corridorGameParent;
    private Vector3 playerInitialPosition;
    private List<Transform> corridorGameChildren = new List<Transform>();
    private float playerMaxDistance = 1000;
    private bool playerReachedMaxDistance;

    public int sectionsTraveledOnCurrentLevel = 0;



    private void Awake()
    {
        //Load levels
        LoadedLevels = Levels.Select(x => (LevelData_Loaded)x).ToList();

        UpdateLevel();

        for (int i = 0; i < corridorSections.Count; i++)
        {
            CorridorSection section = corridorSections[i];
            section.toNotifyOnPlayerEnter = this;
            section.name = "corridor(" + i + ")";
            section.CorridorNumber = i;

            if (i == 0) section.sectionType = SectionType.Back;
            else if (i == corridorSections.Count - 1) section.sectionType = SectionType.Front;

            CreateCorridorPrafabForSection(section, InitialMappingsForDoorSegments(i), i);
        }
    }

    private void UpdateLevel()
    {
        if (Levels != null && Levels.Any())
        {
            LevelData_Loaded temp = GetCurrentLevelData;
            levelCorridorPrefabs = temp.CorridorLayouts; //TODO: Make the check a little more nice for CurrentLevelData?
            levelCorridorBackwardPrefabs = temp.BackwardOnlyLayouts;
        }

        currentLevelChangeTracking = CurrentLevel;
    }

    private LevelData_Loaded GetCurrentLevelData
    {
        get 
        {
            cachedCurrentLevelData = LoadedLevels.FirstOrDefault(x => x.LevelNumber == CurrentLevel);
            return cachedCurrentLevelData;
        }
    }

    private LevelData_Loaded cachedCurrentLevelData;

    private void RenumberSections()
    {
        CorridorSection[] orderedSections = GetOrderedSections;

        for (int i = 0; i < orderedSections.Length; i++)
        {
            CorridorSection oSection = orderedSections[i];
            oSection.name = "corridor(" + i + ")";
            oSection.CorridorNumber = i;
        }
    }

    private Door InitialMappingsForDoorSegments(int corridorPiece)
    {
        if (corridorPiece == 0) return corridorDoorSegments[0];
        else if (corridorPiece == 1) return corridorDoorSegments[2];
        else return corridorDoorSegments[3];
    }

    private void Start()
    {
        playerTransform = GameManager.current.player.transform;
        corridorGameParent = GameManager.current.GameParent;

        //track player position inital
        playerInitialPosition = playerTransform.position;

        //parent everything under it!
        corridorGameChildren.Add(playerTransform);
        corridorGameChildren.AddRange(corridorSections.Select(x => x.transform).Concat(corridorDoorSegments.Select(x => x.transform)).ToArray());
        foreach (Transform child in corridorGameChildren) child.SetParent(corridorGameParent.transform);
    }

    private void CreateCorridorPrafabForSection(CorridorSection section, Door sectionDoor, int index, bool directionPositive = true)
    {
        CorridorLayoutHandler layoutGameObj = null;

        if (OnlyUseRandomAssortedCorridorLayouts && corridorRandomPrefabs.Any())
        {
            layoutGameObj = Instantiate(corridorRandomPrefabs[Random.Range(0, corridorRandomPrefabs.Length)], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
        }
        else if (levelCorridorPrefabs != null && levelCorridorPrefabs.Any())
        {
            if (directionPositive || !levelCorridorBackwardPrefabs.Any())
            {
                layoutGameObj = Instantiate(levelCorridorPrefabs[Mathf.Abs(index) % levelCorridorPrefabs.Length], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
            }
            else 
            {
                layoutGameObj = Instantiate(levelCorridorBackwardPrefabs[Random.Range(0, levelCorridorBackwardPrefabs.Length)], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
                section.CorridorNumber = 0;
            }
        }

        if (layoutGameObj != null)
        {
            //layoutGameObj.SectionDoor = sectionDoor;
            layoutGameObj.InitiateLayout(section.FlipSection, sectionDoor, cachedCurrentLevelData);
            section.CurrentLayout = layoutGameObj;
        }
    }

    private void Update()
    {
        if (sectionToStretch != null)
        {

            if (Mathf.Abs(sectionToStretch.transform.localScale.x) < Mathf.Abs(stretchTarget))
            {
                float currentStretch = Mathf.SmoothStep(initialStretch, stretchTarget, stretchTimer);
                stretchTimer += Time.deltaTime * stretchSpeed;

                sectionToStretch.SetCorridorStretch(currentStretch);
            }
            else
            {
                stretchTimer = 0;
                sectionToStretch = null;
            }
        }

        if (sectionToMakeWavy != null)
        {
            if (currentWavy < wavyTarget)
            {
                currentWavy = Mathf.SmoothStep(intitialWavy, wavyTarget, wavyTimer);
                wavyTimer += Time.deltaTime * wavySpeed;

                sectionToMakeWavy.SetAllWavyness(currentWavy);
                foreach (Door secDoor in doorsToMakeWavy) secDoor.SetWavyness(currentWavy);

            }
            else
            {
                currentWavy = 0;
                wavyTimer = 0;
                sectionToMakeWavy = null;
                doorsToMakeWavy = new Door[0];
            }
        }


        if (currentLevelChangeTracking != CurrentLevel)
        {
            RenumberSections();
            UpdateLevel();
        }

        //Because I'm lazy
        if (CurrentLevel == 2 && !GameManager.current.tvMan.moveTowardPlayer)
        {
            GameManager.current.tvMan.moveTowardPlayer = true;
        }

    }

    private void LateUpdate()
    {
        if (playerReachedMaxDistance)
        {
            Vector3 playerCurrentPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            Vector3 playerIntialPos = new Vector3(playerInitialPosition.x, 0, playerInitialPosition.z);

            corridorGameParent.transform.position = playerIntialPos - playerCurrentPos;
            foreach (Transform child in corridorGameChildren) child.SetParent(null);
            corridorGameParent.transform.position = Vector3.zero;
            foreach (Transform child in corridorGameChildren) child.SetParent(corridorGameParent.transform);

            playerReachedMaxDistance = false;
        }
    }

    public void OnPlayerEnter(CorridorSection currentSection)
    {
        //Check if player has traveled too far from world origin
        CheckPlayerDistance();

        //Get furthest away corridor piece
        Door[] currentSectionDoors = new Door[0];

        //Stop all wavyness on doors
        foreach (Door corrDoor in corridorDoorSegments) corrDoor.SetWavyness(0);


        if (currentSection.sectionType != SectionType.Middle)
        {
            sectionsTraveledOnCurrentLevel++;
            //Check if we are in a trigger section or not
            LevelData_Loaded currentLevelDataTemp = GetCurrentLevelData;
            int levelChange = -1;
            if (!OnlyUseRandomAssortedCorridorLayouts && currentSection.CurrentLayout != null && currentLevelDataTemp.GetIfLevelTriggerAndReturnLevelChange(currentSection.CurrentLayout, out levelChange) || currentLevelDataTemp.GetIfLevelCountTriggerAndReturnLevelChange(sectionsTraveledOnCurrentLevel, out levelChange))
            {
                sectionsTraveledOnCurrentLevel = 0;
                RenumberSections();
                CurrentLevel = levelChange;
                UpdateLevel();
            }


            bool sectionToMoveWasFront = currentSection.sectionType == SectionType.Front;
            SectionType sectionToMoveType = sectionToMoveWasFront ? SectionType.Back : SectionType.Front;
            CorridorSection[] orderedSections = GetOrderedSections;
            CorridorSection sectionToMove = orderedSections.Single(x => x.sectionType == sectionToMoveType);
            CorridorSection newEndSection = orderedSections.Single(x => x.sectionType == SectionType.Middle);


            currentSectionDoors = new Door[] {
                corridorDoorSegments.OrderBy(x => Vector3.Distance(currentSection.CorridorStartEnd[0].position, x.transform.position)).First(),
                corridorDoorSegments.OrderBy(x => Vector3.Distance(currentSection.CorridorStartEnd[1].position, x.transform.position)).First()
            };

            currentSectionDoors[0].fakeParent = currentSection.CorridorStartEnd[0];
            currentSectionDoors[1].fakeParent = currentSection.CorridorStartEnd[1];

            Door newEndDoor = corridorDoorSegments.OrderBy(x => Vector3.Distance(newEndSection.CorridorStartEnd[0].position, x.transform.position)).First();
            newEndDoor.fakeParent = null;

            CreateNextSection(
                sectionToMove,
                currentSection,
                sectionToMoveWasFront,
                corridorDoorSegments.OrderBy(x => Vector3.Distance(sectionToMove.CorridorStartEnd[1].position, x.transform.position)).First()
            );

            newEndSection.sectionType = sectionToMoveType; //set old middle to be new front/back
            currentSection.sectionType = SectionType.Middle; //Set current section to middle

            currentSection.FakeParent = null;
            newEndSection.FakeParent = null;

            if (sectionToMoveWasFront != newEndSection.FlipSection) newEndSection.TogglePropFlip();

            newEndSection.FlipSection = sectionToMoveWasFront; //flip back section to be facing away
            newEndSection.FakeParent = currentSection.CorridorStartEnd[0]; //move to link up with last section
        }

        //Initiate weird extendo times;
        if (false && currentSection.CorridorNumber % 2 == 0)
        {
            if (Random.Range(0f, 1f) > 0.3 && !currentSection.HasWarped)
            {
                //this.playerTransform = playerTransform;
                stretchTimer = 0;
                sectionToStretch = currentSection;
                stretchTarget = Random.Range(1, 5);
            }

            if (Random.Range(0f, 1f) > 0.3)
            {
                wavyTimer = 0;
                sectionToMakeWavy = currentSection;
                currentWavy = 0;
                doorsToMakeWavy = currentSectionDoors;
            }

            currentSection.HasWarped = true;
        }
    }

    private void CreateNextSection(CorridorSection sectionToMove, CorridorSection middleSection, bool directionPositive, Door newSectionEndDoor)
    {
        newSectionEndDoor.fakeParent = sectionToMove.CorridorStartEnd[1];
        newSectionEndDoor.ResetDoor();

        sectionToMove.SetPropSectionFlip(false);
        sectionToMove.SetAllWavyness(0);
        sectionToMove.FakeParent = middleSection.CorridorStartEnd[1]; //parent the new section to the front of the front section

        //These change based on if the section is moving in a positive direction or not
        sectionToMove.FlipSection = !directionPositive; //flip it so its facing forward
        sectionToMove.sectionType = directionPositive ? SectionType.Front : SectionType.Back;  //set to be new front
        int corridorNumberChange = middleSection.CorridorNumber == 0 ? 2 : 1;
        sectionToMove.CorridorNumber = directionPositive ? middleSection.CorridorNumber + corridorNumberChange : middleSection.CorridorNumber - corridorNumberChange;


        sectionToMove.name = "corridor(" + sectionToMove.CorridorNumber + ")";
        sectionToMove.SetCorridorStretch(1);
        sectionToMove.HasWarped = false;
        sectionToMove.FlipCorridorX = Random.Range(0f, 1f) > 0.5f;
        sectionToMove.FlipCorridorZ = Random.Range(0f, 1f) > 0.5f;

        //Remove old objects and add new
        CorridorLayoutHandler sectionCorridorPrefab = sectionToMove.corridorProps.GetComponentInChildren<CorridorLayoutHandler>();
        foreach (PropScript p in sectionCorridorPrefab.Props) Destroy(p.gameObject);
        Destroy(sectionCorridorPrefab.gameObject);

        //TODO: Change this so that it does some fancy creation stuff
        CreateCorridorPrafabForSection(sectionToMove, newSectionEndDoor, sectionToMove.CorridorNumber, directionPositive);
    }

    public void CheckPlayerDistance()
    {
        if (Vector3.Distance(playerTransform.position, playerInitialPosition) > playerMaxDistance) playerReachedMaxDistance = true;
    }

    public void OnPlayerExit(CorridorSection section)
    {
        print("Player exited section " + section.name);
    }
}
