using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseEntity : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform entityTarget;

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
        agent.SetDestination(NavMesh.SamplePosition(entityPosition, out NavMeshHit hit, 400f, NavMesh.AllAreas) ? hit.position : entityPosition);
        print("destination: " + agent.destination);
    }

    private void OnDestroy()
    {
        CorridorChangeManager.OnLevelChange -= UpdateDestination;
    }
}
