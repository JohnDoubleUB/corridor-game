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


    private Vector3 entityPosition;
    private int counter;

    private void Start()
    {
        CorridorChangeManager.OnSectionMove += UpdateDestination;
        entityPosition = entityTarget.position;

        //UpdateDestination();

        //if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit hit,)) 
        //{

        //}
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
        NavMeshHit hit;
        if (agent.remainingDistance == 0 && NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            DrawCircle(transform.position, hit.distance, Color.red);
            Debug.DrawRay(hit.position, Vector3.up, Color.red);
            
            Vector3 hitPos = hit.position;
            hitPos.y = transform.position.y;
            Vector3 opposite = (transform.position - hitPos).normalized;

            agent.SetDestination(GetNewRandomDestination(transform.position, opposite));
        }
    }


    private Vector3 GetNewRandomDestination(Vector3 originalPosition, Vector3 oppositeNormalizedVector, float randomFactor = 1f) 
    {
        Vector3 randomDirection = randomFactor <= 0 ? Vector3.zero : (Random.insideUnitCircle.normalized).ToXZ() * randomFactor;

        return Vector3.Lerp(originalPosition,
                    NavMesh.SamplePosition(originalPosition + ((oppositeNormalizedVector + randomDirection).normalized * 6), out NavMeshHit hit, 200f, NavMesh.AllAreas) ? hit.position : originalPosition,
                    Random.Range(0.2f, 0.8f));
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

    private void OnDestroy()
    {
        CorridorChangeManager.OnLevelChange -= UpdateDestination;
    }
}


public enum MouseBehaviour
{
    Idle_Wander,
    Idle_Look
}