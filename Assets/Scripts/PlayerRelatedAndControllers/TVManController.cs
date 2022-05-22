using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class TVManController : MonoBehaviour
{
    public LayerMask lineOfSightMask;
    public bool BehaviourEnabled = true;
    public bool IsHunting = false;
    public float movementSpeed = 1f;
    public float minimumDistance = 1f;

    public float sightRange = 15f;
    public float sightConeAngle = 30f;

    public float alertTimeWithoutPerception = 5f;
    public float timeToKill = 2f;
    public float delayAfterMomento = 30f;
    public float MaxDistanceFromTarget = 40f;
    //public float EscapeDangerZone = 6f;

    public bool UseNavMesh
    {
        get
        {
            return agent.enabled;
        }
        set
        {
            agent.enabled = value;
        }
    }

    public bool ReadyToSpawn;

    [SerializeField]
    [ReadOnlyField]
    private bool momentoDelayActive;

    public bool MomentoEffectActive { get { return momentoDelayActive; } }


    [SerializeField]
    [ReadOnlyField]
    private float momentoDelayTimer;

    public float CurrentMomentoDelayTimer { get { return momentoDelayTimer; } }

    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    [ReadOnlyField]
    private float interestTimer;


    [SerializeField]
    [ReadOnlyField]
    private float killTimer;

    [SerializeField]
    [ReadOnlyField]
    private Vector3 lastPercievedLocation;

    [SerializeField]
    [ReadOnlyField]
    private Transform[] validPatrolPoints;

    public bool CanSeeTarget { get { return canSeeTarget; } }

    private bool canSeeTarget;

    private float DistanceFromPlayer
    {
        get
        {
            //Get player
            Vector3 currentPlayerLocation = GameManager.current.player.transform.position;
            currentPlayerLocation.y = transform.position.y;

            //Determine distance of tvman from player
            return Vector3.Distance(transform.position, currentPlayerLocation);
        }
    }

    public TVManBehaviour CurrentBehaviour
    {
        get { return currentBehaviour; }

        private set
        {
            if (currentBehaviour != value)
            {
                currentBehaviour = value;
                interestTimer = 0;
                killTimer = 0;
                OnBehaviourChange();
            }
        }
    }

    private bool CanHearNoise
    {
        get
        {
            switch (CurrentBehaviour)
            {
                case TVManBehaviour.PursuingTarget:
                    return CurrentTargetType != EntityType.Player;

                case TVManBehaviour.None:
                case TVManBehaviour.KillingTarget:
                    return false;

                default:
                    return true;
            }
        }
    }

    private bool CanReachTarget(Transform target)
    {
        return CanReachTarget(target.position);
    }

    private bool CanReachTarget(Vector3 target)
    {
        NavMeshPath newNavPath = new NavMeshPath();
        return agent.CalculatePath(target, newNavPath) && newNavPath.status == NavMeshPathStatus.PathComplete;
    }

    [SerializeField]
    [ReadOnlyField]
    private TVManBehaviour currentBehaviour;

    public EntityType CurrentTargetType
    {
        get
        {
            return HuntedTarget != null ? HuntedTarget.EntityType : EntityType.None;
        }
    }

    public Transform TvManEyeLevel;
    private AudioSource audioSource;

    private TransformElements initialTransform;

    private bool updateNavDestination = true;

    private IEnumerator ExecuteOnSectionMove = null;

    private bool IsCurrentlyOnNavMesh
    {
        get
        {
            return NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas);
        }
    }

    [SerializeField]
    [ReadOnlyField]
    private MovementTarget currentTarget;

    private IHuntableEntity HuntedTarget
    {
        get
        {
            return huntedTarget;
        }
        set
        {
            if (value != huntedTarget)
            {
                if (huntedTarget != null)
                {
                    //if (huntedTarget.IsBeingKilled) Destroy(huntedTarget.EntityGameObject);
                    //else 
                    huntedTarget.OnBeingHunted(false);
                }
                huntedTarget = value;
                if (huntedTarget != null)
                {
                    huntedTarget.OnBeingHunted(true);
                }
            }

            IsHunting = value != null;
        }
    }

    private IHuntableEntity huntedTarget;

    [SerializeField]
    [ReadOnlyField]
    private bool canEscapeRoom;
    public bool CanEscapeRoom { get { return canEscapeRoom; } }

    private float defaultVolume;
    private float volumeMultiplier;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        initialTransform = transform.ToTransformElements();
        agent.speed = movementSpeed;
        defaultVolume = audioSource.volume;
    }


    private void Start()
    {
        //Start listening
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
        CorridorChangeManager.OnSectionMove += OnSectionMove;
        CorridorChangeManager.OnNavMeshUpdate += OnNavMeshUpdate;
    }

    private void Update()
    {
        //Allow for changing volume
        if (audioSource.isPlaying && volumeMultiplier != AudioManager.current.SoundVolumeMultiplier) 
        {
            volumeMultiplier = AudioManager.current.SoundVolumeMultiplier;
            audioSource.volume = defaultVolume * volumeMultiplier;
        }

        if (momentoDelayActive)
        {
            if (momentoDelayTimer < delayAfterMomento)
            {
                momentoDelayTimer += Time.deltaTime;
            }
            else
            {
                momentoDelayActive = false;
            }
        }

        //if (movementSpeed != agent.speed) agent.speed = movementSpeed;

        transform.GenerateNoiseAlertAtTransform(IsHunting ? 4f : 1f, NoiseOrigin.TVMan);
        PlayHuntingNoise();
        BehaviourUpdate();
    }

    private void OnDestroy()
    {
        AudioManager.OnEntityNoiseAlert -= OnNoiseMade;
        CorridorChangeManager.OnSectionMove -= OnSectionMove;
        CorridorChangeManager.OnNavMeshUpdate -= OnNavMeshUpdate;
    }

    private void PlayHuntingNoise()
    {
        if (audioSource.isPlaying != IsHunting)
        {
            if (IsHunting)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    private IEnumerable<Transform> GetValidPatrolPoints()
    {
        Transform[][] tempPatrolPoints = CorridorChangeManager.current.TVManPatrolPoints;

        if (tempPatrolPoints.Any())
        {
            IEnumerable<Transform[]> temp = !agent.enabled ? tempPatrolPoints : tempPatrolPoints.Where(x => CanReachTarget(x[0]));

            canEscapeRoom = temp.Any() && temp.Count() > 1;

            if (temp != null && temp.Any())
            {
                return temp.SelectMany(x => x).ToArray();
            }
        }

        return null;
    }

    private MovementTarget GetNextPatrolPoint(IEnumerable<Transform> patrolPoints, Transform pointToIgnore = null)
    {
        if (patrolPoints != null)
        {
            IEnumerable<Transform> nonCurrentPatrolTargets = patrolPoints.Where(x => pointToIgnore == null || x != pointToIgnore);
            IEnumerable<Transform> forwardPatrolTargets = nonCurrentPatrolTargets.Where(x => Vector3.Dot(x.position - transform.position, transform.forward) >= 0);

            Transform newTarget = forwardPatrolTargets.Any() ? forwardPatrolTargets.OrderBy(x => Vector3.Distance(transform.position, x.position)).First()
                : nonCurrentPatrolTargets.Any() ? nonCurrentPatrolTargets.OrderBy(x => Vector3.Distance(x.position, transform.position)).First()
                : null;

            return newTarget.ToMovementTarget();
        }

        return null;
    }

    private void UpdatePatrol()
    {
        if (CurrentBehaviour == TVManBehaviour.Patrolling)
        {
            validPatrolPoints = GetValidPatrolPoints().ToArray();
            currentTarget = GetNextPatrolPoint(validPatrolPoints);
        }
    }

    public void UpdatePatrolAndNavMesh() 
    {
        if (CurrentBehaviour == TVManBehaviour.Patrolling && !agent.enabled)
        {
            UpdateNavAgent();
            UpdatePatrol();
        }
    }

    private void BehaviourUpdate()
    {
        switch (CurrentBehaviour)
        {
            case TVManBehaviour.Patrolling:
                //Patrol behaviour
                if (!PercieveNewTargets() && validPatrolPoints != null && MoveTowardPosition(currentTarget.TargetPosition, false)) 
                {
                    currentTarget = GetNextPatrolPoint(validPatrolPoints, currentTarget.TargetTransform);
                }
                    
                break;

            case TVManBehaviour.Alerted:
                if (!PercieveNewTargets())
                {
                    if (interestTimer < alertTimeWithoutPerception)
                    {
                        interestTimer += Time.deltaTime;
                    }
                    else
                    {
                        CurrentBehaviour = TVManBehaviour.Patrolling;
                    }
                }
                break;

            case TVManBehaviour.Investigating:
                if (!PercieveNewTargets())
                {
                    if (MoveTowardPosition(lastPercievedLocation, false))
                    {
                        CurrentBehaviour = TVManBehaviour.Alerted;
                    }
                }
                break;

            case TVManBehaviour.PursuingTarget:
                //If mouse is target, constantly look for new target in case player or closer mouse is seen
                bool targetIsMouse = CurrentTargetType == EntityType.Mouse;
                if (targetIsMouse) PercieveNewTargets();

                //Move towared target, if target is reached we want to do something
                if (currentTarget.IsTargetNull)
                {
                    CurrentBehaviour = TVManBehaviour.Alerted;
                    canSeeTarget = false;
                }
                else if (MoveTowardPosition(currentTarget.TargetPosition))
                {
                    canSeeTarget = true;
                    if (targetIsMouse && killTimer < timeToKill)
                    {
                        killTimer += Time.deltaTime;
                    }
                    else
                    {
                        CurrentBehaviour = TVManBehaviour.KillingTarget;
                        canSeeTarget = false;
                    }
                }
                else if (CurrentTargetType == EntityType.Player) //If target is player
                {
                    if (PercievePlayer()) //Check if we can still see the player
                    {
                        canSeeTarget = true;
                        interestTimer = Mathf.Max(0f, interestTimer - (Time.deltaTime * 3)); //If we can then make sure the interest timer is decreased
                    }
                    else if (interestTimer < alertTimeWithoutPerception) //Otherwise increase the interest timer
                    {
                        canSeeTarget = false;
                        interestTimer += Time.deltaTime;
                        killTimer = Mathf.Max(0f, killTimer - Time.deltaTime);
                    }
                    else //If we have reached the interest timer limit then tvman returns to an alerted state
                    {
                        CurrentBehaviour = TVManBehaviour.Alerted;
                        canSeeTarget = false;
                    }

                    //if (CurrentBehaviour != TVManBehaviour.Alerted)
                    //{
                    //    float currentDistanceFromPlayer = DistanceFromPlayer;
                    //    if (DistanceFromPlayer < EscapeDangerZone)
                    //    {
                    //        float remappedValue = currentDistanceFromPlayer.Remap(0, 6, 0.1f, 1);
                    //        GameManager.current.HuntingWalkSpeedModifier = remappedValue;
                    //    }
                    //    else 
                    //    {
                    //        GameManager.current.HuntingWalkSpeedModifier = 1f;
                    //    }
                    //}
                    //else 
                    //{
                    //    GameManager.current.HuntingWalkSpeedModifier = 1f;
                    //}
                }

                break;


        }

    }

    private void OnBehaviourChange()
    {
        if (CurrentBehaviour != TVManBehaviour.PursuingTarget && CurrentBehaviour != TVManBehaviour.KillingTarget)
        {
            HuntedTarget = null;
        }

        switch (CurrentBehaviour)
        {
            case TVManBehaviour.None:
                transform.position = initialTransform.Position;
                transform.rotation = initialTransform.Rotation;
                UseNavMesh = false;
                break;

            case TVManBehaviour.Patrolling:
                UpdateNavAgent();
                UpdatePatrol();
                break;

            case TVManBehaviour.Idle:
            case TVManBehaviour.Alerted:
                if (UseNavMesh) agent.ResetPath();
                break;

            case TVManBehaviour.PursuingTarget:
                UpdateNavAgent();
                if (UseNavMesh) agent.ResetPath();
                break;

            case TVManBehaviour.KillingTarget:
                if (HuntedTarget != null && HuntedTarget.EntityType != EntityType.None)
                {
                    UseNavMesh = false;
                    KillHuntableEntity(HuntedTarget);
                }
                else
                {
                    UseNavMesh = IsCurrentlyOnNavMesh;
                    CurrentBehaviour = TVManBehaviour.Alerted;
                }
                break;
        }
    }

    public void ForceKillTarget() 
    {
        CurrentBehaviour = TVManBehaviour.KillingTarget;
    }

    private bool UpdateNavAgent()
    {
        UseNavMesh = IsCurrentlyOnNavMesh;
        return UseNavMesh;
    }

    /*Behaviour scripts Update functions START*/

    private bool PercieveNewTargets()
    {
        IHuntableEntity newTarget = FindHighestPriorityVisibleTarget();
        if (newTarget != null)
        {
            HuntedTarget = newTarget;
            currentTarget = newTarget.EntityTransform.ToMovementTarget();
            CurrentBehaviour = TVManBehaviour.PursuingTarget;
            return true;
        }

        //else
        //{
        //    HuntedTarget = null;
        //}


        return false;
    }

    private void KillHuntableEntity(IHuntableEntity entity)
    {
        if (entity.EntityType == EntityType.None) return;

        if (entity.EntityType == EntityType.Mouse)
        {
            //Kill mouse
            entity.OnEntityKilled();
            KillMouse(entity);

        }
        else
        {

            entity.OnEntityKilled();
            //Kill player
        }
    }

    private async void KillMouse(IHuntableEntity entity)
    {
        Transform entityTransform = entity.EntityTransform;
        Vector3 positionAtTimeOfPickup = entityTransform.position;
        Vector3 target = TvManEyeLevel.position - (Vector3.up * 0.2f);

        float positionValue = 0;
        float smoothedPositionValue;
        float pickupSpeedMultiplier = 0.2f;

        while (positionValue < 1f && entity != null && entity.EntityTransform != null)
        {
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            entityTransform.position = Vector3.Slerp(positionAtTimeOfPickup, target, smoothedPositionValue);
            await Task.Yield();
        }

        CurrentBehaviour = TVManBehaviour.Patrolling;
    }

    /*Behaviour scripts Update functions END*/

    private void OnSectionMove()
    {
        if (ExecuteOnSectionMove != null)
        {
            StartCoroutine(ExecuteOnSectionMove);
            ExecuteOnSectionMove = null;
        }
        UpdateNavAgent();
        UpdatePatrol();
    }


    private IHuntableEntity FindHighestPriorityVisibleTarget()
    {
        //Criteria; is in sight angle (cone), Is in sight range (Radius of tvman), is illuminated (visible ), is in sight line

        if (PercievePlayer()) return GameManager.current.playerController;

        //Get all huntable entities
        List<IHuntableEntity> huntableEntities = CorridorChangeManager.current.Mice;

        //Find anything within sight range of tvman
        //Then check for things
        if (huntableEntities != null && huntableEntities.Any())
        {
            //Find entities within sight and range
            IEnumerable<Tuple<IHuntableEntity, Vector3, float>> entitiesInSightAngleRangeVisibility = huntableEntities
                .Select(x =>
                {
                    Vector3 lookPosition = new Vector3(x.EntityTransform.position.x, transform.position.y, x.EntityTransform.position.z);
                    return new Tuple<IHuntableEntity, Vector3, float>(x, lookPosition, Vector3.Distance(transform.position, lookPosition));
                })
                .Where(x => x.Item1.IsIlluminated && IsWithinSightAngle(x.Item2) && x.Item3 < sightRange);

            return entitiesInSightAngleRangeVisibility != null && entitiesInSightAngleRangeVisibility.Any() ?
                entitiesInSightAngleRangeVisibility.OrderBy(x => x.Item3).Select(x => x.Item1).FirstOrDefault(x => LineOfSightCheck(x))
                : null;
        }

        return null;
    }

    private bool PercievePlayer()
    {
        IHuntableEntity playerEntity = GameManager.current.playerController;
        Vector3 lookPosition = new Vector3(playerEntity.EntityTransform.position.x, transform.position.y, playerEntity.EntityTransform.position.z);
        float distanceFromPlayer = Vector3.Distance(transform.position, lookPosition);
        //bool playerCanBePercievedAtShortDistance = (CurrentBehaviour == TVManBehaviour.Alerted || CurrentBehaviour == TVManBehaviour.PursuingTarget && CurrentTargetType == EntityType.Player) && distanceFromPlayer <= 1f; //This is the old line
        bool playerCanBePercievedAtShortDistance = (CurrentBehaviour == TVManBehaviour.Alerted || CurrentBehaviour == TVManBehaviour.Investigating || CurrentBehaviour == TVManBehaviour.PursuingTarget && CurrentTargetType == EntityType.Player) && distanceFromPlayer <= 1.1f; //Slight change
        return playerCanBePercievedAtShortDistance || playerEntity.IsIlluminated && IsWithinSightAngle(lookPosition) && distanceFromPlayer < sightRange && LineOfSightCheck(playerEntity);
    }

    private bool IsWithinSightAngle(Vector3 target)
    {
        return transform.GetAngleToTarget(target) < sightConeAngle;
    }

    //returns true if it spots the object
    private bool LineOfSightCheck(IHuntableEntity entity, out RaycastHit hitResult)
    {
        return Physics.Linecast(TvManEyeLevel.position, entity.EntityColliderPosition, out hitResult, lineOfSightMask)
            && hitResult.collider.gameObject.CompareTag(entity.EntityGameObject.tag)
            && hitResult.collider.gameObject == entity.EntityGameObject;
    }


    private bool LineOfSightCheck(IHuntableEntity entity)
    {
        return LineOfSightCheck(entity, out RaycastHit _);
    }

    private void OnNavMeshUpdate()
    {
        UpdatePatrol();
    }


    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin)
    {
        if (CanHearNoise && noiseOrigin != NoiseOrigin.TVMan && Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius)
        {
            TurnToNoise(noisePosition);
        }
    }

    public void RemoveFromPlay(bool momentoUsed = false, float delayOnTimer = 0f)
    {
        CurrentBehaviour = TVManBehaviour.None;

        if (momentoUsed)
        {
            momentoDelayTimer = 0f;
            momentoDelayActive = true;
        }
    }

    public void ResetMomentoEffect()
    {
        momentoDelayTimer = 0f;
        momentoDelayActive = false;
    }

    public void PutInPlayOnSectionMove(Transform spawnTransform)
    {
        ExecuteOnSectionMove = PutInPlay(spawnTransform);
    }

    private IEnumerator PutInPlay(Transform spawnTransform)
    {

        PutInPlayNow(spawnTransform);
        yield return null;
    }

    public void PutInPlayNow(Transform spawnTransform)
    {
        //print("he put in play");
        transform.position = spawnTransform.position;
        CurrentBehaviour = TVManBehaviour.Patrolling;
    }

    private void TurnToNoise(Vector3 noisePosition)
    {
        lastPercievedLocation = noisePosition;
        transform.LookAt(new Vector3(lastPercievedLocation.x, transform.position.y, lastPercievedLocation.z));
        interestTimer = 0;
        CurrentBehaviour = CurrentBehaviour == TVManBehaviour.Investigating || (CurrentBehaviour == TVManBehaviour.Alerted && CanReachTarget(noisePosition)) ? TVManBehaviour.Investigating : TVManBehaviour.Alerted;
    }


    private bool MoveTowardPosition(Vector3 target, bool stopAtMinimumDistance = true) //Returns true once target is reached
    {
        target = new Vector3(target.x, transform.position.y, target.z); //Make sure target is on the same plane 
        float minimumDistance = stopAtMinimumDistance ? this.minimumDistance : 0.01f;
        transform.LookAt(target);

        if (!agent.enabled)
        {
            if (Vector3.Distance(target, transform.position) > minimumDistance * 3)
            {
                transform.position += transform.forward * Time.deltaTime * movementSpeed;
            }
            else
            {
                if (!stopAtMinimumDistance) transform.position = target;
                return true;
            }
        }
        else
        {
            agent.stoppingDistance = minimumDistance;

            if (agent.destination != target || updateNavDestination)
            {
                agent.SetDestination(target);
                updateNavDestination = false;
            }
            else if (agent.remainingDistance <= minimumDistance)
            {
                return true;
            }
        }
        return false;
    }

    public void LoadTVManData(TVManData tvManData)
    {
        if (tvManData.MomentoDelayActive) RemoveFromPlay(true, tvManData.CurrentMomentoDelayTimer);
    }

}

public enum TVManBehaviour
{
    None,
    Idle,
    Alerted,
    Patrolling,
    Investigating,
    PursuingTarget,
    KillingTarget
}

public enum EntityType
{
    None,
    Player,
    Mouse
}

public class MovementTarget
{
    private Transform targetTransform;
    private Vector3 targetPosition;
    private bool isTransform;

    private Vector3 lastPosition;
    public bool IsTargetNull { get { return isTransform && targetTransform == null; } }
    public Vector3 TargetPosition 
    { 
        get 
        { 
            return isTransform && targetTransform.position != null ? targetTransform.position : targetPosition; 
        } 
    }
    public Transform TargetTransform { get { return targetTransform; } }
    public bool IsTransform { get { return isTransform; } }
    public MovementTarget(Transform TargetTrasform)
    {
        targetTransform = TargetTrasform;
        isTransform = true;
    }

    public MovementTarget(Vector3 TargetPosition)
    {
        targetPosition = TargetPosition;
    }
}