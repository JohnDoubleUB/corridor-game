using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    [ReadOnlyField]
    private float interestTimer;

    [SerializeField]
    [ReadOnlyField]
    private Vector3 lastPercievedLocation;

    [SerializeField]
    [ReadOnlyField]
    private Transform[] validPatrolPoints;

    public TVManBehaviour CurrentBehaviour
    {
        get { return currentBehaviour; }

        private set
        {
            if (currentBehaviour != value)
            {
                currentBehaviour = value;
                interestTimer = 0;
                OnBehaviourChange();
            }
        }
    }

    private bool CanHearNoise {
        get 
        {
            switch (CurrentBehaviour) 
            {
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

    [SerializeField]
    [ReadOnlyField]
    private PercivedEntity currentTargetType;
    public PercivedEntity CurrentTargetType { get { return currentTargetType; } }

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

    [SerializeField]
    [ReadOnlyField]
    private bool canEscapeRoom;
    public bool CanEscapeRoom { get { return canEscapeRoom; } }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        initialTransform = transform.ToTransformElements();
        agent.speed = movementSpeed;
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
        //if (movementSpeed != agent.speed) agent.speed = movementSpeed;

        transform.GenerateNoiseAlertAtTransform(IsHunting ? 4f : 1f, NoiseOrigin.TVMan);
        PlayHuntingNoise();
        BehaviourUpdate();
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

    private void BehaviourUpdate()
    {
        switch (CurrentBehaviour)
        {
            case TVManBehaviour.Patrolling:
                //Patrol behaviour
                if (validPatrolPoints != null && MoveTowardPosition(currentTarget.TargetPosition, false)) currentTarget = GetNextPatrolPoint(validPatrolPoints, currentTarget.TargetTransform);
                break;

            case TVManBehaviour.Alerted:
                if (interestTimer < alertTimeWithoutPerception)
                {
                    interestTimer += Time.deltaTime;
                }
                else
                {
                    CurrentBehaviour = TVManBehaviour.Patrolling;
                }
                break;
            
            case TVManBehaviour.Investigating:
                if (MoveTowardPosition(lastPercievedLocation, false)) 
                {
                    CurrentBehaviour = TVManBehaviour.Alerted;
                }
                break;
        }

    }

    private bool UpdateNavAgent()
    {
        UseNavMesh = IsCurrentlyOnNavMesh;
        return UseNavMesh;
    }

    /*Behaviour scripts Update functions START*/




    /*Behaviour scripts Update functions END*/
    private void OnBehaviourChange()
    {
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
                //case TVManBehaviour.PursuingTarget:
                //case TVManBehaviour.KillingTarget:
                //    if (IsCurrentlyOnNavMesh) UseNavMesh = true;
                //    break;
        }
    }

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

    //public void UpdatePatrolAndAgent(bool enableNavAgent = true) 
    //{
    //    UseNavMesh = enableNavAgent ? IsCurrentlyOnNavMesh : false;
    //    UpdateNavAgent();
    //    UpdatePatrol();
    //}

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

    public void RemoveFromPlay()
    {
        CurrentBehaviour = TVManBehaviour.None;
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

public enum PercivedEntity
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

    public Vector3 TargetPosition { get { return isTransform ? targetTransform.position : targetPosition; } }
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