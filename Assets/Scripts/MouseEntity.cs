using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class MouseEntity : InteractableObject
{
    public NavMeshAgent agent;
    public Animator mouseAnimator;
    public MouseBob mouseBobber;
    public Rigidbody mouseRB;

    public float offsetFromPosition;
    public float idleDelay = 6f;
    public float idleDelayVariation = 3f;
    private float pickupSpeedMultiplier = 5f;

    public bool IsHeld { get { return isHeld; } }

    //TODO: Make there a time limit for mouse to reach explore destination so it doesn't get stuck
    //TODO: Fix crouching letting you fall through the floor sometimes
    [Range(0.0f, 1.0f)]
    public float chanceToStand = 0.2f;

    public float safeDistanceFromSound = 4f;


    public MouseBehaviour currentBehaviour;

    private Vector3 entityPosition;

    [HideInInspector]
    public float MovementAmount;

    [SerializeField]
    [ReadOnlyField]
    private float currentIdleDelay;


    [SerializeField]
    [ReadOnlyField]
    private int destinationsSinceLastIdleStand;

    private Vector3 lastNoiseHeard;


    private int minimumDestinationsSinceLastStand = 3;
    private bool initialBehaviourUpdate = true;
    private bool isHeld;

    [SerializeField]
    [ReadOnlyField]
    private float behaviourTimer;

    private float defaultSpeed;
    private float defaultAngularSpeed;
    private float defaultAcceleration;



    [SerializeField]
    [ReadOnlyField]
    private float fleeTimer;

    private Vector3 initialPosition;

    private void Awake()
    {
        defaultSpeed = agent.speed;
        defaultAngularSpeed = agent.angularSpeed;
        defaultAcceleration = agent.acceleration;
        initialPosition = transform.position;
    }


    private void Start()
    {
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
        entityPosition = transform.position;
        GenerateRandomIdleDelay();
    }

    protected override void OnInteract()
    {
        print("pickup!");
        if (GameManager.current.playerController.heldMouse == null && currentBehaviour != MouseBehaviour.Thrown)
        {
            SetPickedUp(true);
            PlayerPickup();
        }
    }

    private void SetPickedUp(bool pickedUp)
    {
        currentBehaviour = pickedUp ? MouseBehaviour.Held : MouseBehaviour.Idle_Wander;
        agent.enabled = !pickedUp;
        IsInteractable = !pickedUp;
        mouseAnimator.Play("Idle");
        mouseBobber.AllowMouseBob = !pickedUp;
    }


    private async void PlayerPickup()
    {
        isHeld = false;
        currentBehaviour = MouseBehaviour.Held;
        if (!mouseAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) mouseAnimator.Play("Idle");

        Vector3 positionAtTimeOfPickup = transform.position;
        Quaternion rotationAtTimeOfPickup = transform.rotation;

        float positionValue = 0;
        float smoothedPositionValue;

        Transform handTransform = GameManager.current.playerController.mouseHand;


        Vector3 handPosition;
        Quaternion handRotation;

        GameManager.current.playerController.heldMouse = this;

        while (positionValue < 1f)
        {
            handPosition = handTransform.position;
            handRotation = handTransform.rotation;
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            transform.SetPositionAndRotation(
                Vector3.Lerp(positionAtTimeOfPickup, handPosition, smoothedPositionValue),
                Quaternion.Lerp(rotationAtTimeOfPickup, handRotation, smoothedPositionValue)
                );
            await Task.Yield();
        }

        transform.SetParent(handTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        isHeld = true;
    }

    // Update is called once per frame
    void Update()
    {
        MovementAmount = Vector3.Distance(transform.position, entityPosition);

        switch (currentBehaviour)
        {
            case MouseBehaviour.Idle_Wander:
                Behaviour_IdleExplore();
                break;

            case MouseBehaviour.Idle_Look:
                Behaviour_IdleStand();
                break;

            case MouseBehaviour.FleeingFromNoise:
                Behaviour_FleeFromNoise();
                break;

            case MouseBehaviour.Freeze:
                Behaviour_Freeze();
                break;

            case MouseBehaviour.Held:
                break;

            case MouseBehaviour.Thrown:
                Behaviour_Thrown();
                break;
        }

        entityPosition = transform.position;
    }

    private void Behaviour_Freeze()
    {
        if (!mouseAnimator.GetCurrentAnimatorStateInfo(0).IsName("Scared")) mouseAnimator.Play("Scared", 0);
        if (!agent.isStopped) agent.isStopped = true;
        if (behaviourTimer < currentIdleDelay / 2)
        {
            behaviourTimer += Time.deltaTime;
        }
        else
        {
            behaviourTimer = 0;
            currentBehaviour = MouseBehaviour.Idle_Wander;
            mouseAnimator.Play("Idle");
            agent.isStopped = false;
        }
    }

    private Vector3 GetNewRandomDestination(Vector3 originalPosition, Vector3 oppositeNormalizedVector, float randomFactor = 1f, float distanceModifier = 6f)
    {
        Vector3 randomDirection = randomFactor <= 0 ? Vector3.zero : (Random.insideUnitCircle.normalized).ToXZ() * randomFactor;

        return Vector3.Lerp(originalPosition,
                    NavMesh.SamplePosition(originalPosition + ((oppositeNormalizedVector + randomDirection).normalized * distanceModifier), out NavMeshHit hit, 200f, NavMesh.AllAreas) ? hit.position : originalPosition,
                    Random.Range(0.2f, 0.8f));
    }


    private void Behaviour_IdleExplore()
    {
        if (fleeTimer != 0f) fleeTimer = 0f;
        if (agent.remainingDistance == 0)
        {
            if (behaviourTimer < currentIdleDelay)
            {
                behaviourTimer += Time.deltaTime;
            }
            else
            {
                if (destinationsSinceLastIdleStand >= minimumDestinationsSinceLastStand && Random.value < chanceToStand)
                {
                    currentBehaviour = MouseBehaviour.Idle_Look;
                    initialBehaviourUpdate = true;
                    destinationsSinceLastIdleStand = 0;
                }
                else
                {
                    Explore();
                    destinationsSinceLastIdleStand++;
                }
                behaviourTimer = 0;
                GenerateRandomIdleDelay();
            }
        }
    }

    private void Behaviour_FleeFromNoise()
    {
        if (Vector3.Distance(transform.position, lastNoiseHeard) > safeDistanceFromSound)
        {
            behaviourTimer = 0;
            //GenerateRandomIdleDelay();
            currentBehaviour = MouseBehaviour.Idle_Wander;
        }

        if (fleeTimer > 1f)
        {
            fleeTimer = 0f;
            currentBehaviour = MouseBehaviour.Freeze;
        }
        else if (MovementAmount < agent.speed)
        {
            fleeTimer += Time.deltaTime;
        }
    }

    private void Behaviour_IdleStand()
    {
        if (initialBehaviourUpdate)
        {
            mouseAnimator.Play("Idle_Standup", 0);
            initialBehaviourUpdate = false;
        }
        else if (mouseAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
        {
            currentBehaviour = MouseBehaviour.Idle_Wander;
            SetMouseOverallSpeedMultiplier(1);
        }
    }

    private void Behaviour_Thrown() 
    {
        if(Vector3.Distance(transform.position, entityPosition) < 0.1f) 
        {
            Vector3 dir = (transform.position - entityPosition).normalized;

            mouseRB.isKinematic = true;
            transform.rotation.SetLookRotation(dir);
            Vector3 newPosition = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
            transform.position = NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 100f, NavMesh.AllAreas) ? new Vector3(hit.position.x, newPosition.y, hit.position.z) : newPosition;
            SetPickedUp(false);
        }
    } 


    private void GenerateRandomIdleDelay()
    {
        currentIdleDelay = Random.Range(Mathf.Max(idleDelay - idleDelayVariation, 1f), idleDelayVariation + idleDelay);
    }

    private void Explore()
    {
        if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit hit, NavMesh.AllAreas))
        {
            Vector3 hitPos = hit.position;
            hitPos.y = transform.position.y;
            Vector3 opposite = (transform.position - hitPos).normalized;

            agent.SetDestination(GetNewRandomDestination(transform.position, opposite));
        }
    }

    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius, NoiseOrigin noiseOrigin)
    {
        if (Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) ReactToNoise(noisePosition);
    }

    private void ReactToNoise(Vector3 noisePosition)
    {
        if (currentBehaviour != MouseBehaviour.Freeze && currentBehaviour != MouseBehaviour.Held && currentBehaviour != MouseBehaviour.Thrown)
        {
            lastNoiseHeard = noisePosition;
            currentBehaviour = MouseBehaviour.FleeingFromNoise;
            behaviourTimer = 0;


            Vector3 hitPos = noisePosition;
            hitPos.y = transform.position.y;
            Vector3 opposite = (transform.position - hitPos).normalized;

            agent.SetDestination(GetNewRandomDestination(transform.position, opposite, 1f, 20f));

            SetMouseOverallSpeedMultiplier(5);
        }
    }

    private void SetMouseOverallSpeedMultiplier(float multiplier)
    {
        float speedMult = defaultSpeed * multiplier;
        float angularSpeedMult = defaultAngularSpeed * multiplier;
        float accelerationMult = defaultAcceleration * multiplier;

        if (agent.speed != speedMult) agent.speed = speedMult;
        if (agent.angularSpeed != angularSpeedMult) agent.angularSpeed = angularSpeedMult;
        if (accelerationMult != defaultAcceleration) agent.acceleration = accelerationMult;
    }

    private void OnDestroy()
    {
        AudioManager.OnEntityNoiseAlert -= OnNoiseMade;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (currentBehaviour != MouseBehaviour.Held && currentBehaviour != MouseBehaviour.Thrown && other.tag == "Player") 
        {
            fleeTimer = 0;
            currentBehaviour = MouseBehaviour.Freeze;
        }
    }

    public void ThrowAtTarget(Vector3 target, float magnitude) 
    {
        mouseRB.isKinematic = false;
        mouseRB.LaunchAtTarget(target, magnitude);
        currentBehaviour = MouseBehaviour.Thrown;
    }
}


public enum MouseBehaviour
{
    Idle_Wander,
    Idle_Look,
    FleeingFromNoise,
    Freeze,
    Held,
    Thrown
}