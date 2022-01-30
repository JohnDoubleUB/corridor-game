using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseEntity : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform entityTarget;
    public float offsetFromPosition;

    private Vector3 entityPosition;


    private void Start()
    {
        CorridorChangeManager.OnSectionMove += UpdateDestination;
        entityPosition = entityTarget.position;
        UpdateDestination();
    }

    // Update is called once per frame
    void Update()
    {
        if (entityTarget.position != entityPosition) 
        {
            entityPosition = entityTarget.position;
            UpdateDestination();
        }
    }

    private void UpdateDestination() 
    {
        Vector3 calculatedDestination = NavMesh.SamplePosition(entityPosition, out NavMeshHit hit, 400f, NavMesh.AllAreas) ? hit.position : entityPosition;

        if (offsetFromPosition > 0) 
        {
            Vector3 dir = (transform.position - calculatedDestination).normalized;
            calculatedDestination += dir * offsetFromPosition;
        }

        agent.SetDestination(calculatedDestination);

        print("destination: " + agent.destination);
    }

    private void OnDestroy()
    {
        CorridorChangeManager.OnLevelChange -= UpdateDestination;
    }
}
