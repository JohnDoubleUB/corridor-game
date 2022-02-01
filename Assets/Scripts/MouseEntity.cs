using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseEntity : InteractableObject
{
    public NavMeshAgent agent;
    public Transform entityTarget;
    public float offsetFromPosition;
    public Animator mouseAnimator;
    public float idleDelay = 6f;
    public float idleDelayVariation = 3f;

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

    [SerializeField]
    [ReadOnlyField]
    private float behaviourTimer;

    private float defaultSpeed;
    private float defaultAngularSpeed;
    private float defaultAcceleration;


    private void Awake()
    {
        defaultSpeed = agent.speed;
        defaultAngularSpeed = agent.angularSpeed;
        defaultAcceleration = agent.acceleration;
    }


    private void Start()
    {
        CorridorChangeManager.OnSectionMove += UpdateDestination;
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
        entityPosition = entityTarget.position;

        //UpdateDestination();

        //if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit hit,)) 
        //{

        //}
        GenerateRandomIdleDelay();
    }


    void DrawCircle(Vector3 center, float radius, Color color)
    {
        Vector3 prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, newPos, color);
            prevPos = newPos;
        }
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
        }

        entityPosition = transform.position;
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
            GenerateRandomIdleDelay();
            currentBehaviour = MouseBehaviour.Idle_Wander;
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

    private void GenerateRandomIdleDelay()
    {        
        currentIdleDelay = Random.Range(Mathf.Max(idleDelay - idleDelayVariation, 1f), idleDelayVariation + idleDelay);
    }

    private void Explore()
    {
        if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit hit, NavMesh.AllAreas))
        {
            DrawCircle(transform.position, hit.distance, Color.red);
            Debug.DrawRay(hit.position, Vector3.up, Color.red);

            Vector3 hitPos = hit.position;
            hitPos.y = transform.position.y;
            Vector3 opposite = (transform.position - hitPos).normalized;

            agent.SetDestination(GetNewRandomDestination(transform.position, opposite));
        }
    }



    private void UpdateDestination()
    {
        //Vector3 calculatedDestination = NavMesh.SamplePosition(entityPosition, out NavMeshHit hit, 400f, NavMesh.AllAreas) ? hit.position : entityPosition;

        //if (offsetFromPosition > 0) 
        //{
        //    Vector3 dir = (transform.position - calculatedDestination).normalized;
        //    calculatedDestination += dir * offsetFromPosition;
        //}

        //agent.SetDestination(calculatedDestination);

        //print("destination: " + agent.destination);
    }


    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius)
    {
        if (Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) ReactToNoise(noisePosition);
    }

    private void ReactToNoise(Vector3 noisePosition) 
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
        CorridorChangeManager.OnLevelChange -= UpdateDestination;
        AudioManager.OnEntityNoiseAlert -= OnNoiseMade;
    }
}


public enum MouseBehaviour
{
    Idle_Wander,
    Idle_Look,
    FleeingFromNoise
}