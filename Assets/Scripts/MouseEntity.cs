using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class MouseEntity : InteractableObject
{
    public NavMeshAgent agent;
    public Animator mouseAnimator;
    public MouseBob mouseBobber;
    public Rigidbody mouseRB;

    public AudioClip[] MouseThrowNoises;
    public AudioClip[] MouseLandNoises;
    public AudioClip[] MouseSqueaks;

    public float squeakDelay = 6f;
    public float squeakDelayVariation = 3f;


    public float idleDelay = 6f;
    public float idleDelayVariation = 3f;
    
    public float fleeSpeedMultiplier = 5f;
    public bool IsBeingChased;

    public float landNoiseRadius = 6f;
    public float noiseVolume = 1f;

    private float pickupSpeedMultiplier = 5f;

    public bool IsHeld { get { return isHeld; } }

    //TODO: Make there a time limit for mouse to reach explore destination so it doesn't get stuck
    [Range(0.0f, 1.0f)]
    public float chanceToStand = 0.2f;

    public float safeDistanceFromSound = 4f;

    public float maxWanderTime = 10f; //This is to precent the mouse from getting stuck trying to wander to a point 

    public MouseBehaviour currentBehaviour;

    public MouseBehaviour CurrentBehaviour
    {
        get { return currentBehaviour; }
        private set
        {
            bool isNewBehaviour = currentBehaviour != value;
            bool isFleeingFromTvMan = value == MouseBehaviour.ChasedByTVMan;
            bool isFleeing = value == MouseBehaviour.FleeingFromNoise || isFleeingFromTvMan;
            bool needsToMove = isFleeing || value == MouseBehaviour.Wander;
            bool isBeingHeld = value == MouseBehaviour.Held;

            if (agent.isActiveAndEnabled) agent.isStopped = !needsToMove;

            if (isBeingHeld || needsToMove) mouseAnimator.Play(isFleeing && isNewBehaviour ? "Startled" : "Idle", 0);

            if (isNewBehaviour || isFleeing)
            {
                initialBehaviourUpdate = isNewBehaviour;
                behaviourJustChanged = isNewBehaviour;
                currentBehaviour = value;
                behaviourTimer = 0f;
                fleeTimer = 0f;
                SetMouseOverallSpeedMultiplier(isFleeing ? fleeSpeedMultiplier : 1);
            }
        }
    }

    private List<AudioSource> throwNoises = new List<AudioSource>();

    private Vector3 entityPosition;

    [HideInInspector]
    public float MovementAmount;

    [SerializeField]
    [ReadOnlyField]
    private float currentIdleDelay;


    [SerializeField]
    [ReadOnlyField]
    private int destinationsSinceLastIdleStand;


    [SerializeField]
    [ReadOnlyField]
    private float fleeTimer;

    [SerializeField]
    [ReadOnlyField]
    private float behaviourTimer;

    [SerializeField]
    [ReadOnlyField]
    private float currentSqueakDelay;

    [SerializeField]
    [ReadOnlyField]
    private float squeakTimer;

    private Vector3 lastNoiseHeard;

    private int minimumDestinationsSinceLastStand = 3;

    private bool initialBehaviourUpdate = true;
    private bool behaviourJustChanged = false;

    private bool isHeld;

    private float defaultSpeed;
    private float defaultAngularSpeed;
    private float defaultAcceleration;

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
        GenerateRandomSqueakDelay();
    }

    protected override void OnInteract()
    {
        if (GameManager.current.playerController.heldMouse == null && CurrentBehaviour != MouseBehaviour.Thrown)
        {
            SetPickedUp(true);
            PlayerPickup();
        }
    }

    private void SetPickedUp(bool pickedUp)
    {
        CurrentBehaviour = pickedUp ? MouseBehaviour.Held : MouseBehaviour.Idle;
        agent.enabled = !pickedUp;
        IsInteractable = !pickedUp;
        mouseBobber.AllowMouseBob = !pickedUp;
        mouseAnimator.Play("Startled", 0);
        if (!pickedUp && MouseLandNoises != null && MouseLandNoises.Any()) 
        { 
            transform.PlayClipAtTransform(MouseLandNoises[Random.Range(0, MouseLandNoises.Length)], true, noiseVolume, true, 0, true, landNoiseRadius);
            
            foreach (AudioSource throwNoise in throwNoises) 
            {
                if (throwNoise != null) 
                {
                    throwNoise.Stop();
                    Destroy(throwNoise.gameObject);
                }
            }

        }
    }


    private async void PlayerPickup()
    {
        isHeld = false;
        CurrentBehaviour = MouseBehaviour.Held;

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
        behaviourJustChanged = false;


        CheckIfAndTransitionToChased();
        BehaviourUpdate();

        switch (CurrentBehaviour) 
        {
            case MouseBehaviour.Idle:
            case MouseBehaviour.Held:
            case MouseBehaviour.Look:
            case MouseBehaviour.Wander:
                if (squeakTimer > currentSqueakDelay)
                {
                    if (MouseSqueaks != null && MouseSqueaks.Any()) transform.PlayClipAtTransform(MouseSqueaks[Random.Range(0, MouseSqueaks.Length)], true, noiseVolume, true, 0, false);
                    GenerateRandomSqueakDelay();
                    squeakTimer = 0f;
                }
                else 
                {
                    squeakTimer += Time.deltaTime;
                }
                break;
        }


        entityPosition = transform.position;
        if (!behaviourJustChanged) initialBehaviourUpdate = false;
    }


    private void BehaviourUpdate() 
    {
        switch (CurrentBehaviour)
        {
            case MouseBehaviour.Idle:
                Behaviour_Idle();
                break;

            case MouseBehaviour.Look:
                Behaviour_IdleStand();
                break;

            case MouseBehaviour.Wander:
                Behaviour_Wander();
                break;

            case MouseBehaviour.FleeingFromNoise:
                Behaviour_FleeFromNoise();
                break;

            case MouseBehaviour.Freeze:
                Behaviour_Freeze();
                break;

            case MouseBehaviour.Held:
                break;

            case MouseBehaviour.ChasedByTVMan:
                break;

            case MouseBehaviour.Thrown:
                Behaviour_Thrown();
                break;
        }
    }

    private void CheckIfAndTransitionToChased() 
    {
        switch (CurrentBehaviour) 
        {
            case MouseBehaviour.Idle:
            case MouseBehaviour.Freeze:
            case MouseBehaviour.Look:
            case MouseBehaviour.Wander:
            case MouseBehaviour.FleeingFromNoise:
                if (IsBeingChased) CurrentBehaviour = MouseBehaviour.ChasedByTVMan;
                break;
        }
    }

    private void Behaviour_Freeze()
    {
        if (!mouseAnimator.GetCurrentAnimatorStateInfo(0).IsName("Scared")) mouseAnimator.Play("Scared", 0);
        if (behaviourTimer < currentIdleDelay / 2)
        {
            behaviourTimer += Time.deltaTime;
        }
        else
        {
            CurrentBehaviour = MouseBehaviour.Idle;
        }
    }

    private Vector3 GetNewRandomDestination(Vector3 originalPosition, Vector3 oppositeNormalizedVector, float randomFactor = 1f, float distanceModifier = 6f)
    {
        Vector3 randomDirection = randomFactor <= 0 ? Vector3.zero : (Random.insideUnitCircle.normalized).ToXZ() * randomFactor;

        return Vector3.Lerp(originalPosition,
                    NavMesh.SamplePosition(originalPosition + ((oppositeNormalizedVector + randomDirection).normalized * distanceModifier), out NavMeshHit hit, 200f, NavMesh.AllAreas) ? hit.position : originalPosition,
                    Random.Range(0.2f, 0.8f));
    }


    private void Behaviour_Idle()
    {
        if (behaviourTimer < currentIdleDelay)
        {
            behaviourTimer += Time.deltaTime;
        }
        else
        {
            if (destinationsSinceLastIdleStand >= minimumDestinationsSinceLastStand && Random.value < chanceToStand)
            {
                CurrentBehaviour = MouseBehaviour.Look;
                destinationsSinceLastIdleStand = 0;
            }
            else
            {
                CurrentBehaviour = MouseBehaviour.Wander;
                destinationsSinceLastIdleStand++;
            }

            GenerateRandomIdleDelay();
        }
    }

    private void Behaviour_Wander()
    {
        if (initialBehaviourUpdate)
        {
            Explore();
        }
        else if (agent.remainingDistance == 0 || behaviourTimer > maxWanderTime)
        {
            CurrentBehaviour = MouseBehaviour.Idle;
        }
        else 
        {
            behaviourTimer += Time.deltaTime;
        }
    }

    private void Behaviour_FleeFromNoise()
    {
        if (Vector3.Distance(transform.position, lastNoiseHeard) > safeDistanceFromSound)
        {
            CurrentBehaviour = MouseBehaviour.Idle;
        }



        if (fleeTimer > 2f)
        {
            CurrentBehaviour = MouseBehaviour.Freeze;
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
        }
        else if (mouseAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
        {
            CurrentBehaviour = MouseBehaviour.Idle;
        }
    }

    private void Behaviour_Thrown()
    {
        Vector3 positionButGround = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
        if (Vector3.Distance(transform.position, entityPosition) < 0.05f || Vector3.Distance(transform.position, positionButGround) < 0.2f)
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

    private void GenerateRandomSqueakDelay() 
    {
        currentSqueakDelay = Random.Range(Mathf.Max(squeakDelay - squeakDelayVariation, 1f), squeakDelayVariation + squeakDelay);
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
        if (noiseOrigin != NoiseOrigin.Mouse && Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) ReactToNoise(noisePosition);
    }

    private void ReactToNoise(Vector3 noisePosition)
    {
        if (CurrentBehaviour != MouseBehaviour.Freeze && CurrentBehaviour != MouseBehaviour.Held && CurrentBehaviour != MouseBehaviour.Thrown)
        {
            lastNoiseHeard = noisePosition;
            

            Vector3 hitPos = noisePosition;
            hitPos.y = transform.position.y;
            Vector3 opposite = (transform.position - hitPos).normalized;

            agent.SetDestination(GetNewRandomDestination(transform.position, opposite, 1f, 20f));

            CurrentBehaviour = MouseBehaviour.FleeingFromNoise;
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
        if (CurrentBehaviour != MouseBehaviour.Held && CurrentBehaviour != MouseBehaviour.Thrown && other.tag == "Player")
        {
            CurrentBehaviour = MouseBehaviour.Freeze;
        }
    }

    public void ThrowAtTarget(Vector3 target, float magnitude, Vector3 forwardVector)
    {
        mouseRB.isKinematic = false;
        mouseRB.transform.forward = forwardVector;
        mouseRB.LaunchAtTarget(target, Vector3.one * 600, magnitude);
        CurrentBehaviour = MouseBehaviour.Thrown;
        
        if (MouseThrowNoises != null && MouseThrowNoises.Any()) 
        {
            throwNoises.Add(transform.PlayClipAtTransform(MouseThrowNoises[Random.Range(0, MouseThrowNoises.Length)], true, noiseVolume, true, 0, false));
        }
    }
}


public enum MouseBehaviour
{
    Idle,
    Look,
    Wander,
    FleeingFromNoise,
    Freeze,
    Held,
    Thrown,
    ChasedByTVMan
}