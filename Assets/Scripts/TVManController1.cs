﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class TVManControllerOLD : MonoBehaviour
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
    private float lastPerceivedTimer;
    [SerializeField]
    [ReadOnlyField]
    private float interestTimer;
    [SerializeField]
    [ReadOnlyField]
    private Vector3 lastPercievedLocation;

    [SerializeField]
    [ReadOnlyField]
    private Transform[] validPatrolPoints;

    [SerializeField]
    [ReadOnlyField]
    private MouseEntity targetMouse;

    public TVManBehaviourOLD CurrentBehaviour
    {
        get { return currentBehaviour; }
        set
        {
            if (currentBehaviour != value)
            {
                if (currentBehaviour == TVManBehaviourOLD.PursuingMouse) targetMouse = null;
                currentBehaviour = value;
                OnBehaviourChange();
            }
        }
    }

    [SerializeField]
    [ReadOnlyField]
    private TVManBehaviourOLD currentBehaviour;

    public Transform TvManEyeLevel;
    private AudioSource audioSource;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;

    private bool updateNavDestination = true;

    private IEnumerator toPutInPlayOnSectionMove = null;

    private bool IsCurrentlyOnNavMesh
    {
        get
        {
            return NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas);
        }
    }

    [SerializeField]
    [ReadOnlyField]
    private Transform patrolTarget;

    [SerializeField]
    [ReadOnlyField]
    private bool canEscapeRoom;
    public bool CanEscapeRoom { get { return canEscapeRoom; } }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }


    private void Start()
    {
        //Start listening
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
        CorridorChangeManager.OnSectionMove += OnSectionMove;
        CorridorChangeManager.OnNavMeshUpdate += OnNavMeshUpdate;
        OnBehaviourChange();
    }

    private void Update()
    {
        if (movementSpeed != agent.speed) agent.speed = movementSpeed;

        transform.GenerateNoiseAlertAtTransform(IsHunting ? 4f : 1f, NoiseOrigin.TVMan);

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

        BehaviourUpdate();
    }

    private void GetUpdatedValidPatrolPoints()
    {
        //if (!UseNavMesh) UseNavMesh = true;

        Transform[][] tempPatrolPoints = CorridorChangeManager.current.TVManPatrolPoints;

        if (tempPatrolPoints.Any())
        {
            IEnumerable<Transform[]> temp = !agent.enabled ? tempPatrolPoints : tempPatrolPoints.Where(x =>
            {
                NavMeshPath newNavPath = new NavMeshPath();
                return agent.CalculatePath(x[0].position, newNavPath) && newNavPath.status == NavMeshPathStatus.PathComplete;
            });

            canEscapeRoom = temp.Any() && temp.Count() > 1;

            if (temp != null && temp.Any())
            {
                validPatrolPoints = temp.SelectMany(x => x).ToArray();
            }
            else
            {
                validPatrolPoints = null;
            }
        }
    }

    private void BehaviourUpdate()
    {
        switch (CurrentBehaviour)
        {
            case TVManBehaviourOLD.None:
                CurrentBehaviour = TVManBehaviourOLD.Patrolling;
                break;

            case TVManBehaviourOLD.PursuingPlayer:
                Behaviour_PursuePlayer();
                break;

            case TVManBehaviourOLD.PursuingMouse:
                Behaviour_PursueMouse();
                break;

            case TVManBehaviourOLD.Waiting:
                Behaviour_Waiting();
                break;

            case TVManBehaviourOLD.PursuingLastPercived:
                Behaviour_PursueLastPercieved();
                break;

            case TVManBehaviourOLD.Patrolling:
                Behaviour_Patrolling();
                break;
        }
    }

    private void OnSectionMove()
    {
        if (toPutInPlayOnSectionMove != null)
        {
            StartCoroutine(toPutInPlayOnSectionMove);
            toPutInPlayOnSectionMove = null;
        }

        transform.SetParent(null);
        updateNavDestination = true;
    }

    private void OnNavMeshUpdate()
    {
        agent.enabled = IsCurrentlyOnNavMesh;
        GetUpdatedValidPatrolPoints();
    }

    private void OnBehaviourChange()
    {
        interestTimer = 0f;
        lastPerceivedTimer = 0f;
        patrolTarget = null;

        switch (currentBehaviour)
        {
            case TVManBehaviourOLD.Patrolling:
                if (IsCurrentlyOnNavMesh)
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                }
                else
                {
                    agent.enabled = false;
                }
                GetUpdatedValidPatrolPoints();
                FindNextPatrolPoint();
                break;

            case TVManBehaviourOLD.Waiting:
                if (agent.enabled) agent.isStopped = true;
                goto case TVManBehaviourOLD.None;

            case TVManBehaviourOLD.NotInPlay:
                agent.enabled = false;
                canEscapeRoom = false;
                transform.SetParent(null);
                transform.SetPositionAndRotation(initialPosition, initialRotation);
                goto case TVManBehaviourOLD.None;

            case TVManBehaviourOLD.PursuingLastPercived:
            case TVManBehaviourOLD.None:
                if (IsHunting) IsHunting = false;
                break;

            case TVManBehaviourOLD.PursuingMouse:
            case TVManBehaviourOLD.PursuingPlayer:
                if (IsCurrentlyOnNavMesh && !agent.enabled) agent.enabled = true;
                if (!IsHunting) IsHunting = true;
                break;
        }
    }

    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin)
    {
        if (CurrentBehaviour != TVManBehaviourOLD.NotInPlay && noiseOrigin != NoiseOrigin.TVMan && Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) TurnToNoise(noisePosition);
    }

    public void RemoveFromPlay()
    {
        CurrentBehaviour = TVManBehaviourOLD.NotInPlay;
    }

    public void PutInPlayOnSectionMove(Transform spawnTransform)
    {
        toPutInPlayOnSectionMove = PutInPlay(spawnTransform);
    }

    private IEnumerator PutInPlay(Transform spawnTransform)
    {

        PutInPlayNow(spawnTransform);
        //if (CurrentBehaviour != TVManBehaviour.NotInPlay) 
        //{ 
        //    GetUpdatedValidPatrolPoints(); 
        //}

        yield return null;
    }

    public void PutInPlayNow(Transform spawnTransform)
    {
        print("he put in play");
        initialSpawnPosition = spawnTransform.position;
        initialSpawnRotation = transform.rotation;
        //CurrentBehaviour = TVManBehaviour.None;
        //transform.SetPositionAndRotation(initialSpawnPosition, initialSpawnRotation);
        //toPutInPlayOnSectionMove = null;
    }

    private void TurnToNoise(Vector3 noisePosition)
    {
        //lastPercievedLocation = noisePosition;
        //interestTimer = 0;


        //switch (CurrentBehaviour)
        //{
        //    case TVManBehaviour.None:
        //    case TVManBehaviour.Returning:
        //    case TVManBehaviour.Patrolling:
        //        transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
        //        lastPerceivedTimer = 0f;
        //        CurrentBehaviour = TVManBehaviour.Waiting;
        //        break;

        //    case TVManBehaviour.Waiting:
        //        NavMeshPath newPath = new NavMeshPath();
        //        bool pathCalculated = agent.CalculatePath(lastPercievedLocation, newPath);
        //        if (pathCalculated && newPath.status == NavMeshPathStatus.PathComplete)
        //        {
        //            CurrentBehaviour = TVManBehaviour.PursuingLastPercived;
        //        }
        //        lastPerceivedTimer = 0f;
        //        break;
        //}
    }

    private void Behaviour_PursueLastPercieved()
    {
        if (!Behaviour_Perceive() && MoveTowardPosition(lastPercievedLocation, false))
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                CurrentBehaviour = TVManBehaviourOLD.Patrolling;
            }
        }
        else
        {
            lastPerceivedTimer = 0f;
            if (interestTimer < alertTimeWithoutPerception * 1.5)
            {
                interestTimer += Time.deltaTime;
            }
            else
            {
                CurrentBehaviour = TVManBehaviourOLD.Waiting;
            }

        }
    }

    private void Behaviour_Waiting()
    {
        if (!Behaviour_Perceive())
        {
            //agent.isStopped = true;
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                CurrentBehaviour = TVManBehaviourOLD.Patrolling;
                lastPerceivedTimer = 0f;
            }
        }
    }
    //TODO: something isn't right here, I think tv man behaviour needs a large overhaul
    private bool Behaviour_Perceive()
    {
        //Check for player
        Vector3 playerPosition = GameManager.current.player.transform.position;
        Vector3 playerLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

        if (Vector3.Distance(playerLookLocation, transform.position) < sightRange &&
            (LineOfSightCheck(playerPosition) == PercivedEntity.Player || Vector3.Distance(playerLookLocation, transform.position) <= minimumDistance))
        {
            CurrentBehaviour = TVManBehaviourOLD.PursuingPlayer;
            lastPerceivedTimer = 0f;
            updateNavDestination = true;
            return true;

        }
        else
        {
            //Get all mice that are visible and are in line of sight
            IEnumerable<MouseEntity> miceVisibleToTVMan = CorridorChangeManager.current.Mice.Where(x => x.DetectableByTVMan && LineOfSightCheck(x.transform.position) == PercivedEntity.Mouse);
            if (miceVisibleToTVMan.Any())
            {
                //Get all of those that are in range
                IEnumerable<Tuple<float, MouseEntity>> miceWithinRange = miceVisibleToTVMan
                    .Select(x => new Tuple<float, MouseEntity>(Vector3.Distance(transform.position, new Vector3(x.transform.position.x, transform.position.y, x.transform.position.z)), x))
                    .Where(x => x.Item1 < sightRange);

                if (miceWithinRange != null && miceWithinRange.Any())
                {
                    //Get mouse that is the closest to the player
                    MouseEntity newTarget = miceWithinRange.OrderBy(x => x.Item1).Select(x => x.Item2).First();
                    
                    //If this isn't the current target then change it!
                    if (targetMouse != newTarget)
                    {

                        //Tell mouse it's being hunted;

                        targetMouse = newTarget;
                        CurrentBehaviour = TVManBehaviourOLD.PursuingMouse;
                        updateNavDestination = true;
                        return true;
                    }
                }
            }

        }

        return false;
    }

    private void Behaviour_Patrolling()
    {
        if (validPatrolPoints != null)
        {
            if (!validPatrolPoints.Contains(patrolTarget) || !Behaviour_Perceive() && MoveTowardPosition(patrolTarget.position, false))
            {
                FindNextPatrolPoint();
            }
        }
    }

    private bool FindNextPatrolPoint()
    {
        bool newTargetSelected = false;

        if (validPatrolPoints != null)
        {
            IEnumerable<Transform> nonCurrentPatrolTargets = validPatrolPoints.Where(x => x != patrolTarget);
            IEnumerable<Transform> forwardPatrolTargets = nonCurrentPatrolTargets.Where(x => Vector3.Dot(x.position - transform.position, transform.forward) >= 0);

            Transform newTarget = forwardPatrolTargets.Any() ? forwardPatrolTargets.OrderBy(x => Vector3.Distance(transform.position, x.position)).First()
                : nonCurrentPatrolTargets.Any() ? nonCurrentPatrolTargets.OrderBy(x => Vector3.Distance(x.position, transform.position)).First()
                : patrolTarget;

            newTargetSelected = patrolTarget != newTarget;
            patrolTarget = newTarget;
        }

        return newTargetSelected;
    }



    private PercivedEntity LineOfSightCheck(Vector3 target)
    {
        return LineOfSightCheck(target, out RaycastHit? hit);
    }

    private PercivedEntity LineOfSightCheck(Vector3 target, out RaycastHit? hit)
    {
        if (transform.GetAngleToTarget(target) < sightConeAngle && Physics.Linecast(TvManEyeLevel.position, target, out RaycastHit hitResult, lineOfSightMask))
        {
            hit = hitResult;
            switch (hitResult.collider.gameObject.tag)
            {
                case "Player":
                    return GameManager.current.playerController.IsIlluminated ? PercivedEntity.Player : PercivedEntity.None;
                case "Entity":
                    return PercivedEntity.Mouse;
            }
        }
        hit = null;
        return PercivedEntity.None;
    }

    private void Behaviour_PursuePlayer()
    {
        MoveTowardPosition(GameManager.current.player.transform.position);

        if (LineOfSightCheck(GameManager.current.player.transform.position) != PercivedEntity.Player)
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                CurrentBehaviour = TVManBehaviourOLD.Waiting;
                lastPerceivedTimer = 0f;
            }
        }
        else
        {
            lastPerceivedTimer = 0f;
        }

    }

    private void Behaviour_PursueMouse()
    {
        MoveTowardPosition(targetMouse.transform.position);

        if (Behaviour_Perceive())
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                CurrentBehaviour = TVManBehaviourOLD.Waiting;
                lastPerceivedTimer = 0f;
            }
        }
        else
        {
            lastPerceivedTimer = 0f;
        }
    }

    private bool MoveTowardPosition(Vector3 target, bool stopAtMinimumDistance = true) //Returns true once target is reached
    {
        target = new Vector3(target.x, transform.position.y, target.z); //Make sure target is on the same plane 
        float minimumDistance = stopAtMinimumDistance ? this.minimumDistance : 0.01f;
        transform.LookAt(target);

        if (!agent.enabled)
        {
            if (Vector3.Distance(target, transform.position) > minimumDistance)
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

//public enum TVManBehaviour
//{
//    NotInPlay,
//    None,
//    PursuingPlayer,
//    PursuingMouse,
//    PursuingLastPercived,
//    Returning,
//    Patrolling,
//    AbsorbingPlayer,
//    Waiting
//}

//public enum PercivedEntity
//{
//    None,
//    Player,
//    Entity
//}

public enum TVManBehaviourOLD
{
    NotInPlay,
    None,
    PursuingPlayer,
    PursuingMouse,
    PursuingLastPercived,
    Returning,
    Patrolling,
    AbsorbingPlayer,
    Waiting
}
