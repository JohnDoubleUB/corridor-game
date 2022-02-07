using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TVManController : MonoBehaviour
{
    public LayerMask lineOfSightMask;
    public bool BehaviourEnabled = true;
    public bool IsHunting = true;
    public float movementSpeed = 1f;
    public float minimumDistance = 1f;

    public float sightRange = 15f;
    public float sightConeAngle = 30f;

    public float alertTimeWithoutPerception = 5f;

    public bool useNavAgent = false;
    public NavMeshAgent agent;


    [SerializeField]
    [ReadOnlyField]
    private float lastPerceivedTimer;
    [SerializeField]
    [ReadOnlyField]
    private float interestTimer;
    [SerializeField]
    [ReadOnlyField]
    private Vector3 lastPercievedLocation;

    public TVManBehaviour currentBehaviour;
    public Transform TvManEyeLevel;

    private AudioSource audioSource;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

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
    }

    private void Update()
    {
        if (useNavAgent != agent.enabled) agent.enabled = useNavAgent;
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

        switch (currentBehaviour)
        {
            case TVManBehaviour.None:
                if (IsHunting) IsHunting = false;
                Behaviour_Perceive();
                break;

            case TVManBehaviour.PursuingPlayer:
                if (!IsHunting) IsHunting = true;
                Behaviour_PursuePlayer();
                break;

            case TVManBehaviour.Returning:
                if (IsHunting) IsHunting = false;
                Behaviour_Returning();
                break;

            case TVManBehaviour.Waiting:
                if (IsHunting) IsHunting = false;
                Behaviour_Waiting();
                break;

            case TVManBehaviour.PursuingLastPercived:
                if (IsHunting) IsHunting = false;
                Behaviour_PursueLastPercieved();
                break;
        }
    }

    private void UpdateNavDestination()
    {
        updateNavDestination = true;
    }

    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin)
    {
        if (noiseOrigin != NoiseOrigin.TVMan && Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) TurnToNoise(noisePosition);
    }

    private void TurnToNoise(Vector3 noisePosition)
    {
        lastPercievedLocation = noisePosition;
        interestTimer = 0;

        switch (currentBehaviour)
        {
            //case TVManBehaviour.PursuingPlayer:
            //case TVManBehaviour.PursuingMouse:
            //    break;

            case TVManBehaviour.None:
            case TVManBehaviour.Returning:
                transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
                lastPerceivedTimer = 0f;
                currentBehaviour = TVManBehaviour.Waiting;
                break;

            case TVManBehaviour.Waiting:
                currentBehaviour = TVManBehaviour.PursuingLastPercived;
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
                currentBehaviour = TVManBehaviour.Returning;
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
                currentBehaviour = TVManBehaviour.Waiting;
            }

        }
    }

    private void Behaviour_Waiting()
    {
        if (!Behaviour_Perceive())
        {
            agent.isStopped = true;
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                currentBehaviour = TVManBehaviour.Returning;
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
            currentBehaviour = TVManBehaviour.PursuingPlayer;
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
        if (!Behaviour_Perceive() && MoveTowardPosition(initialPosition, false))
        {
            transform.rotation = initialRotation;
            currentBehaviour = TVManBehaviour.None;
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
                currentBehaviour = TVManBehaviour.Waiting;
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

        if (!useNavAgent)
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
            if (agent.isStopped) agent.isStopped = false;
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