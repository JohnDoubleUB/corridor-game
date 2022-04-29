using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CorridorChangeManager : MonoBehaviour
{
    public static CorridorChangeManager current;

    public delegate void LevelChangeAction();
    public static event LevelChangeAction OnLevelChange;

    public delegate void SectionMoveAction();
    public static event SectionMoveAction OnSectionMove;

    public delegate void NavMeshUpdateAction();
    public static event NavMeshUpdateAction OnNavMeshUpdate;

    public delegate void SaveGameAction();
    public static event SaveGameAction OnSaveGame;

    public List<CorridorSection> corridorSections;
    public List<Door> corridorDoorSegments;

    public CorridorLayoutHandler[] corridorRandomPrefabs;
    public bool OnlyUseRandomAssortedCorridorLayouts;

    public List<LevelData> Levels;
    private List<LevelData_Loaded> loadedLevels;

    public List<LevelData_Loaded> LoadedLevels 
    { 
        get { return loadedLevels; }
        private set { loadedLevels = value; }
    }

    public int CurrentLevel = 1;
    private int currentLevelChangeTracking;

    public CorridorLayoutHandler[] levelCorridorPrefabs;
    public CorridorLayoutHandler[] levelCorridorBackwardPrefabs;
    public CorridorLayoutHandler[] levelCorridorForwardPrefabs;

    CorridorSection[] GetOrderedSections { get { return corridorSections.OrderByDescending(x => x.transform.position.x).ToArray(); } }

    private Transform playerTransform;

    private GameObject corridorGameParent;
    private Vector3 playerInitialPosition;
    private List<Transform> corridorGameChildren = new List<Transform>();
    private float playerMaxDistance = 1000;
    private bool playerReachedMaxDistance;

    private bool directionPositiveOnLastCorridorPiece = true;
    private bool directionPositiveOnLevelStart = true;

    public int sectionsTraveledOnCurrentLevel = 0;

    public Mesh[] corridorMeshes;

    public CorridorMatVarient[] CorridorMatVarients;

    public MouseEntity MousePrefab;

    private int mouseCount;

    private List<IHuntableEntity> mice = new List<IHuntableEntity>();
    public List<IHuntableEntity> Mice { get { return mice; } }


    private Transform[][] tVManPatrolPoints;
    public Transform[][] TVManPatrolPoints { get { return tVManPatrolPoints; } }

    [ReadOnlyField]
    [SerializeField]
    private int sectionForwardCounter = 0;
    [ReadOnlyField]
    [SerializeField]
    private int sectionBackwardCounter = 0;

    [ReadOnlyField]
    [SerializeField]
    private List<string> eventTags = new List<string>();

    private void Awake()
    {
        //loadedCorridorResourceMeshes = CorridorMeshVarients.Select(x => Resources.Load<Mesh>(x.name)).ToArray();

        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        ////Setup of level data
        //LoadLevelData();

        ////Do things with the loaded level data
        //UpdateLevel();
    }

    private void Start()
    {
        playerTransform = GameManager.current.player.transform;
        //Setup of level data
        LoadLevelData();

        //Do things with the loaded level data
        UpdateLevel();

        SetupInitialSections();
        
        corridorGameParent = GameManager.current.GameParent;

        //track player position inital
        playerInitialPosition = playerTransform.position;

        //parent everything under it!
        corridorGameChildren.Add(playerTransform);
        corridorGameChildren.AddRange(corridorSections.Select(x => x.transform).Concat(corridorDoorSegments.Select(x => x.transform)).ToArray());
        foreach (Transform child in corridorGameChildren) child.SetParent(corridorGameParent.transform);
        tVManPatrolPoints = corridorSections.Select(x => x.TVManPatrolLocations).ToArray();
    }

    public void ReplaceLevelLayoutData(int levelIndex, int layoutIndex, LayoutLevelData replacedData) 
    {
        print("Replacing data for level layout: " + replacedData.LayoutID);
        loadedLevels[levelIndex].CorridorLayoutData[layoutIndex] = replacedData;
    }

    private void LoadLevelData()
    {
        if (SaveSystem.LoadType != GameLoadType.New && TryLoadGame(out SaveData savedData) && savedData != null)
        {
            CurrentLevel = savedData.CurrentLevel;

            IEnumerable<LevelData_Serialized> savedLevelData = savedData.LoadedLevels.OrderBy(x => x.LevelNumber);

            loadedLevels = Levels.OrderBy(x => x.LevelNumber).Select((x, index) =>
            {
                LevelData_Serialized currentSerializedLevelData = savedLevelData.ElementAtOrDefault(index);
                return currentSerializedLevelData != null && x.LevelNumber == currentSerializedLevelData.LevelNumber ? new LevelData_Loaded(x, currentSerializedLevelData) : x;
            }).ToList();

            //Check if we have associated player data and if we do then load that
            if (savedData.PlayerData != null) GameManager.current.playerController.LoadSavedPlayerData(savedData.PlayerData);

            //Check if we have associated inventory data, if we do then load it
            if (savedData.InventoryData != null) InventoryManager.current.LoadSavedInventoryData(savedData.InventoryData.InventoryItems, savedData.InventoryData.MomentoItems);

            //Check if we have associated tvmandata
            if (savedData.TVManData != null) GameManager.current.tvMan.LoadTVManData(savedData.TVManData);

            if (savedData.EventTags != null && savedData.EventTags.Any()) eventTags = savedData.EventTags.ToList();
        }
        else
        {
            loadedLevels = Levels.Select(x => (LevelData_Loaded)x).ToList();
            SaveGame(); //Save the level as the player loads in
        }



        SaveSystem.LoadType = GameLoadType.Existing;
    }

    public bool EventTagPresent(string EventTag) 
    {
        return eventTags.Contains(EventTag);
    }

    //Returns true if successful
    public bool AddEventTag(string EventTag) 
    {
        if (EventTagPresent(EventTag))
        {
            return false;
        }
        else 
        {
            eventTags.Add(EventTag);
            return true;
        }
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(new SaveData(loadedLevels, new PlayerData(GameManager.current.playerController), new InventoryData(InventoryManager.current), new TVManData(GameManager.current.tvMan), eventTags, CurrentLevel));
        OnSaveGame?.Invoke();
    }

    public void SaveGameOnLevel(int level)
    {
        SaveSystem.SaveGame(new SaveData(loadedLevels, new PlayerData(GameManager.current.playerController), new InventoryData(InventoryManager.current), new TVManData(GameManager.current.tvMan), eventTags, level));
        OnSaveGame?.Invoke();
    }

    public void CreateNewSave() 
    {
        SaveSystem.SaveGame(new SaveData(Levels.Select(x => (LevelData_Loaded)x).ToList(), null, null, null, new List<string>(), 1));
    }

    public async void SaveAfterTimePassedDelta(float timeSeconds = 1f) 
    {
        float timer = 0;

        while (timer < timeSeconds) 
        {
            timer += Time.deltaTime;
            await Task.Yield();
        }


        SaveGame();
        //SaveSystem.SaveGame(new SaveData(loadedLevels, new PlayerData(GameManager.current.playerController), new InventoryData(InventoryManager.current), CurrentLevel));
        //OnSaveGame?.Invoke();
    }

    private bool TryLoadGame(out SaveData savedData)
    {
        return SaveSystem.TryLoadGame(out savedData);
    }
    private void UpdateLevel()
    {
        if (Levels != null && Levels.Any())
        {
            LevelData_Loaded temp = GetCurrentLevelData;
            levelCorridorPrefabs = temp.CorridorLayouts;
            levelCorridorBackwardPrefabs = temp.BackwardOnlyLayouts;
            levelCorridorForwardPrefabs = temp.ForwardOnlyLayouts;
        }

        currentLevelChangeTracking = CurrentLevel;
    }

    public void RemoveMouseFromList(MouseEntity mouse)
    {
        if (mice.Contains(mouse)) mice.Remove(mouse);
    }

    private LevelData_Loaded GetCurrentLevelData
    {
        get
        {
            cachedCurrentLevelData = loadedLevels.FirstOrDefault(x => x.LevelNumber == CurrentLevel);
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

    private CorridorLayoutHandler CreateCorridorPrefabForSection(CorridorSection section, Door sectionDoor, int index, bool directionPositive = true)
    {
        CorridorLayoutHandler layoutGameObj = null;

        if (OnlyUseRandomAssortedCorridorLayouts && corridorRandomPrefabs.Any())
        {
            layoutGameObj = Instantiate(corridorRandomPrefabs[Random.Range(0, corridorRandomPrefabs.Length)], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
        }
        else if (levelCorridorPrefabs != null && levelCorridorPrefabs.Any())
        {
            if (directionPositive == directionPositiveOnLevelStart || !levelCorridorBackwardPrefabs.Any())
            {

                if(directionPositive == directionPositiveOnLevelStart && levelCorridorForwardPrefabs.Any())//Added forward only layouts
                {
                    /*Random.Range(0, levelCorridorBackwardPrefabs.Length)]*/
                    layoutGameObj = Instantiate(levelCorridorForwardPrefabs[Mathf.Abs(sectionForwardCounter) % levelCorridorForwardPrefabs.Length], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
                    sectionForwardCounter++;
                    sectionBackwardCounter = 0;
                    section.CorridorNumber = 0;
                }
                else
                {
                    layoutGameObj = Instantiate(levelCorridorPrefabs[Mathf.Abs(index) % levelCorridorPrefabs.Length], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
                    sectionForwardCounter = 0;
                    sectionBackwardCounter = 0;
                }

            }
            else
            {
                /*levelCorridorBackwardPrefabs[Random.Range(0, levelCorridorBackwardPrefabs.Length)]*/
                layoutGameObj = Instantiate(levelCorridorBackwardPrefabs[Mathf.Abs(sectionBackwardCounter) % levelCorridorBackwardPrefabs.Length], section.corridorProps.transform.position, GameManager.current != null ? GameManager.current.GameParent.transform.rotation : Quaternion.identity, section.corridorProps.transform);
                sectionBackwardCounter++;
                sectionForwardCounter = 0;
                section.CorridorNumber = 0;
            }
        }
        if (layoutGameObj != null)
        {
            layoutGameObj.InitiateLayout(section.FlipSection, sectionDoor, cachedCurrentLevelData, loadedLevels);
            section.CurrentLayout = layoutGameObj;

            int layoutMeshType = (int)layoutGameObj.corridorMeshType;

            Mesh corridorMesh = corridorMeshes[layoutMeshType]; //The issue is with this bit?

            if (corridorMesh != null)
            {
                section.ChangeMesh(corridorMesh, layoutMeshType);
            }

            if (layoutMeshType != 0)
            {
                section.FlipCorridorX = false;
                section.FlipCorridorZ = false;
                layoutGameObj.sectionDoor.SetDoorVisible(false);
            }

            section.SetMaterialVarient(CorridorMatVarients[(int)layoutGameObj.corridorMatType]);
            sectionDoor.SetMaterialVarient(CorridorMatVarients[(int)layoutGameObj.corridorDoorMatType]);
        }
        else
        {
            section.ChangeMesh(corridorMeshes[0]);

            section.SetMaterialVarient(CorridorMatVarients[0]);
            sectionDoor.SetMaterialVarient(CorridorMatVarients[0]);
        }

        //StartCoroutine(HandleSpawningForSectionAfterTime(section));

        return layoutGameObj;
    }

    IEnumerator HandleSpawningForSectionAfterTime(CorridorSection section, float timeSeconds = 1f, bool roomEffectPresent = false)
    {
        CorridorLayoutHandler layoutGameObj = section.CurrentLayout;
        LevelData_Loaded currentLoadedLevelData = cachedCurrentLevelData;



        yield return new WaitForSeconds(timeSeconds);

        HandleTVManSpawningForSection(section, layoutGameObj, currentLoadedLevelData, roomEffectPresent);
        HandleMouseSpawningForSection(section, layoutGameObj, currentLoadedLevelData);
    }

    private void HandleTVManSpawningForSection(CorridorSection section, CorridorLayoutHandler layoutGameObj, LevelData_Loaded currentLoadedLevelData, bool putInPlayNow = false)
    {
        if (currentLoadedLevelData.EnableTVMan && section.sectionType != SectionType.Middle && layoutGameObj.AllowTVMan && !GameManager.current.tvMan.MomentoEffectActive && 
            (GameManager.current.tvMan.CurrentBehaviour == TVManBehaviour.None || 
            Vector3.Distance(GameManager.current.tvMan.transform.position, GameManager.current.player.transform.position) > GameManager.current.tvMan.MaxDistanceFromTarget))
        {
            Transform furthestFromPlayer = section.TVManPatrolLocations.OrderByDescending(x => Vector3.Distance(x.position, playerTransform.position)).FirstOrDefault();
            GameManager.current.tvMan.PutInPlayOnSectionMove(furthestFromPlayer != null ? furthestFromPlayer : section.TVManPatrolLocations[Random.Range(0, section.TVManPatrolLocations.Length)]);
        }
    }

    private void HandleMouseSpawningForSection(CorridorSection section, CorridorLayoutHandler layoutGameObj, LevelData_Loaded currentLoadedLevelData)
    {
        //Check if mouse can be spawned here
        if (layoutGameObj.AllowMouseSpawns && mice.Count < currentLoadedLevelData.MaxMouseCount && !section.EntityTracker.TVManIsInArea)
        {
            MouseEntity tempMouse = Instantiate(MousePrefab, section.GetMouseSpawnLocations(1)[0], Quaternion.identity, null);
            mouseCount++;
            tempMouse.name = "Mouse -" + mouseCount;
            section.EntityTracker.AddDistinctEntities(tempMouse.gameObject);
            mice.Add(tempMouse);
        }
    }

    private void SetupInitialSections()
    {
        for (int i = 0; i < corridorSections.Count; i++)
        {
            CorridorSection section = corridorSections[i];
            section.toNotifyOnPlayerEnter = this;
            section.name = "corridor(" + i + ")";
            section.CorridorNumber = i;

            if (i == 0) section.sectionType = SectionType.Back;
            else if (i == corridorSections.Count - 1) section.sectionType = SectionType.Front;

            CreateCorridorPrefabForSection(section, InitialMappingsForDoorSegments(i), i);
        }
    }

    private void Update()
    {
        if (currentLevelChangeTracking != CurrentLevel)
        {
            RenumberSections();
            UpdateLevel();
        }

        //if (Input.GetKeyDown(KeyCode.V)) 
        //{
        //    print("saving game!");
        //    SaveGame();
        //    print("game saved!");
        //}
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

        currentSection.CurrentLayout.OnEnterCustomScripts(); //Run any custom scripts


        //Get furthest away corridor piece
        Door[] currentSectionDoors = new Door[0];

        //Stop all wavyness on doors
        foreach (Door corrDoor in corridorDoorSegments)
        {
            corrDoor.SetWavyness(0);
        }


        if (currentSection.sectionType != SectionType.Middle)
        {
            sectionsTraveledOnCurrentLevel++;

            //Check if we are in a trigger section or not
            LevelData_Loaded currentLevelDataTemp = GetCurrentLevelData;
            int levelChange = -1;
            //Check if section we are moving is front (useful for other parts of the code)
            bool sectionToMoveWasFront = currentSection.sectionType == SectionType.Front;
            directionPositiveOnLastCorridorPiece = sectionToMoveWasFront;

            if (!OnlyUseRandomAssortedCorridorLayouts
                && currentSection.CurrentLayout != null
                && currentLevelDataTemp.GetIfLevelTriggerAndReturnLevelChange(currentSection.CurrentLayout, out levelChange)
                || currentLevelDataTemp.GetIfLevelCountTriggerAndReturnLevelChange(sectionsTraveledOnCurrentLevel, out levelChange))
            {
                directionPositiveOnLevelStart = directionPositiveOnLastCorridorPiece;
                LevelChange(levelChange);
            }

            ////Check if section we are moving is front (useful for other parts of the code)
            //bool sectionToMoveWasFront = currentSection.sectionType == SectionType.Front;

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

            //Handle spawning for section
            StartCoroutine(HandleSpawningForSectionAfterTime(sectionToMove));

            //Apply the effects to the current section
            ApplyCorridorEffects(currentSection, currentSectionDoors, currentLevelDataTemp);
            //after delay notify objects that piece has moved
            OnSectionMoveAfterDelay(2);
        }
    }

    public void TriggerNavMeshUpdate()
    {
        StartCoroutine(TriggerNavMeshUpdateAfterTime(0.3f));
    }

    private IEnumerator TriggerNavMeshUpdateAfterTime(float waitTimeSeconds)
    {
        yield return new WaitForSeconds(waitTimeSeconds);
        OnNavMeshUpdate?.Invoke();
    }

    private async void OnSectionMoveAfterDelay(float timeSeconds)
    {
        await Task.Delay(System.TimeSpan.FromSeconds(timeSeconds));
        OnSectionMove?.Invoke();
    }

    private bool ApplyCorridorEffects(CorridorSection currentSection, Door[] currentSectionDoors, LevelData_Loaded currentLoadedLevel)
    {
        bool effectUsed = false;
        //Check if this section has already been warped
        if (!currentSection.HasWarped)
        {
            bool hasWarped = false;
            //Check if we should stretch, either if its set to force or if settings for level allow it
            if (currentSection.WillStretch ||
                currentLoadedLevel.AllowRandomScaling && currentLoadedLevel.ScaleEffectCount < currentLoadedLevel.MaxScaleEffectCount && Random.value < 0.5f)
            {
                currentSection.StretchTo(currentSection.StretchAmount);
                currentLoadedLevel.ScaleEffectCount++;
                hasWarped = true;
                effectUsed = true;
            }

            //Check if we should make section wave, either if it has it set to force or if settings for level allow it
            if (currentSection.WillWave ||
                currentLoadedLevel.AllowRandomWaving && currentLoadedLevel.WaveEffectCount < currentLoadedLevel.MaxWaveEffectCount && Random.value < 0.5f)
            {
                currentSection.MakeWave();
                foreach (Door currentDoor in currentSectionDoors) currentDoor.MakeWave();
                currentLoadedLevel.WaveEffectCount++;
                hasWarped = true;
                effectUsed = true;
            }

            currentSection.HasWarped = hasWarped;
        }

        return effectUsed;
    }

    private void CleanMiceFromSection(CorridorSection section)
    {
        if (section.EntityTracker.EntitiesInArea.Any())
        {
            var miceInSection = mice.Where(x => section.EntityTracker.EntitiesInArea.Contains(x.EntityGameObject)).ToArray();
            section.EntityTracker.RemoveAllEntities();

            foreach (MouseEntity mouseEntity in miceInSection)
            {
                Destroy(mouseEntity.gameObject);
                mice.Remove(mouseEntity);
            }
        }
    }

    private void CleanUpTVManFromSection(CorridorSection section)
    {
        if (section.EntityTracker.TVManIsInArea)
        {
            if (GameManager.current.tvMan.CanEscapeRoom)
            {
                //Move to nearest section
            }
            else
            {
                GameManager.current.tvMan.RemoveFromPlay();
            }
        }
    }


    private CorridorLayoutHandler CreateNextSection(CorridorSection sectionToMove, CorridorSection middleSection, bool directionPositive, Door newSectionEndDoor)
    {
        //Handle spawning and stuff
        CleanMiceFromSection(sectionToMove);
        CleanUpTVManFromSection(sectionToMove);
        // gah

        newSectionEndDoor.fakeParent = sectionToMove.CorridorStartEnd[1];
        newSectionEndDoor.ResetDoor();

        sectionToMove.StopAllEffects();
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
        CorridorLayoutHandler newLayoutHandler = CreateCorridorPrefabForSection(sectionToMove, newSectionEndDoor, sectionToMove.CorridorNumber, directionPositive);

        return newLayoutHandler;
    }

    public void LevelChange(int newLevel)
    {
        if (newLevel != CurrentLevel) OnLevelChange?.Invoke(); //Invoke if there is anything to invoke on
        directionPositiveOnLevelStart = directionPositiveOnLastCorridorPiece;
        sectionsTraveledOnCurrentLevel = 0;
        RenumberSections();
        CurrentLevel = newLevel;
        if(!GetCurrentLevelData.EnableTVMan) GameManager.current.tvMan.RemoveFromPlay(); //Makes tvman only despawn if the next level doesn't allow him to be there
        GameManager.current.tvMan.ResetMomentoEffect();
        UpdateLevel();
        if (cachedCurrentLevelData.IsCheckpoint) 
        {
            if (!cachedCurrentLevelData.CheckPointOnDelay) SaveGame();
            else SaveAfterTimePassedDelta(2);
        }
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

[System.Serializable]
public class CorridorMatVarient
{
    public Texture albedo1;
    public Texture albedo2;
}