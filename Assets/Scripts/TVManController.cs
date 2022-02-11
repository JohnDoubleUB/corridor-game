using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TVManController : MonoBehaviour
{
    //public bool IsInPlay = false;
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


    public TVManBehaviour CurrentBehaviour
    {
        get { return currentBehaviour; }
        set
        {
            if (currentBehaviour != value)
            {
                currentBehaviour = value;

                OnBehaviourChange();
            }
        }
    }

    [SerializeField]
    [ReadOnlyField]
    private TVManBehaviour currentBehaviour;

    public Transform TvManEyeLevel;
    private AudioSource audioSource;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;

    private bool updateNavDestination = true;

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
        CorridorChangeManager.OnSectionMove += UpdateNavDestination;
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

    private void BehaviourUpdate()
    {
        switch (CurrentBehaviour)
        {
            case TVManBehaviour.None:
                Behaviour_Perceive();
                break;

            case TVManBehaviour.PursuingPlayer:
                Behaviour_PursuePlayer();
                break;

            case TVManBehaviour.Returning:
                Behaviour_Returning();
                break;

            case TVManBehaviour.Waiting:
                Behaviour_Waiting();
                break;

            case TVManBehaviour.PursuingLastPercived:
                Behaviour_PursueLastPercieved();
                break;
        }
    }

    private void UpdateNavDestination()
    {
        updateNavDestination = true;
    }

    private void OnBehaviourChange()
    {
        interestTimer = 0f;
        lastPerceivedTimer = 0f;


        switch (currentBehaviour)
        {
            case TVManBehaviour.PursuingLastPercived:
                UseNavMesh = true;
                goto case TVManBehaviour.None;

            case TVManBehaviour.NotInPlay:
                UseNavMesh = false;
                transform.SetPositionAndRotation(initialPosition, initialRotation);
                goto case TVManBehaviour.None;

            case TVManBehaviour.Waiting:
            case TVManBehaviour.Returning:
            case TVManBehaviour.None:
                if (IsHunting) IsHunting = false;
                break;

            case TVManBehaviour.PursuingPlayer:
                UseNavMesh = true;
                if (!IsHunting) IsHunting = true;
                break;
        }

        bool TVManNeedsToMove = currentBehaviour != TVManBehaviour.None && currentBehaviour != TVManBehaviour.Waiting;
        if (UseNavMesh) agent.isStopped = !TVManNeedsToMove;
    }

    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin)
    {
        if (CurrentBehaviour != TVManBehaviour.NotInPlay && noiseOrigin != NoiseOrigin.TVMan && Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) TurnToNoise(noisePosition);
    }

    public void RemoveFromPlay()
    {
        CurrentBehaviour = TVManBehaviour.NotInPlay;
    }

    public void PutInPlay(Vector3 position)
    {
        initialSpawnPosition = position;
        initialSpawnRotation = transform.rotation;
        CurrentBehaviour = TVManBehaviour.None;
        transform.SetPositionAndRotation(initialSpawnPosition, initialSpawnRotation);
    }

    private void TurnToNoise(Vector3 noisePosition)
    {
        lastPercievedLocation = noisePosition;
        interestTimer = 0;
        Vector3 directionOfNoise = (noisePosition - transform.position).normalized;


        switch (CurrentBehaviour)
        {
            case TVManBehaviour.None:
            case TVManBehaviour.Returning:
                transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
                lastPerceivedTimer = 0f;
                CurrentBehaviour = TVManBehaviour.Waiting;
                break;

            case TVManBehaviour.Waiting:
                UseNavMesh = true;
                NavMeshPath newPath = new NavMeshPath();
                bool pathCalculated = agent.CalculatePath(lastPercievedLocation, newPath);
                UseNavMesh = false;
                if (pathCalculated && newPath.status == NavMeshPathStatus.PathComplete) 
                {
                    CurrentBehaviour = TVManBehaviour.PursuingLastPercived;
                }
                //CurrentBehaviour = TVManBehaviour.PursuingLastPercived;
                lastPerceivedTimer = 0f;
                break;
        }
        //if (currentBehaviour != TVManBehaviour.PursuingPlayer && currentBehaviour != TVManBehaviour.PursuingMouse)
        //{
        //    transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
        //    lastPercivedTarget = 0f;
        //    currentBehaviour = TVManBehaviour.Waiting;
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
                CurrentBehaviour = TVManBehaviour.Returning;
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
                CurrentBehaviour = TVManBehaviour.Waiting;
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
                CurrentBehaviour = TVManBehaviour.Returning;
                lastPerceivedTimer = 0f;
            }
        }
    }

    private bool Behaviour_Perceive()
    {
        //Check for player
        Vector3 playerPosition = GameManager.current.player.transform.position;
        Vector3 playerLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

        if (Vector3.Distance(playerLookLocation, transform.position) < sightRange &&
            (LineOfSightCheck(playerPosition) == PercivedEntity.Player || Vector3.Distance(playerLookLocation, transform.position) <= minimumDistance))
        {
            CurrentBehaviour = TVManBehaviour.PursuingPlayer;
            lastPerceivedTimer = 0f;
            return true;

        }
        else if (false) //Check for other thing (Mouse when implemented)
        {

        }

        return false;
    }

    private void Behaviour_Returning()
    {
        if (!Behaviour_Perceive() && MoveTowardPosition(initialSpawnPosition, false))
        {
            transform.rotation = initialSpawnRotation;
            CurrentBehaviour = TVManBehaviour.None;
        }
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
                    return PercivedEntity.Entity;
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
                CurrentBehaviour = TVManBehaviour.Waiting;
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

public enum TVManBehaviour
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

public enum PercivedEntity
{
    None,
    Player,
    Entity
}