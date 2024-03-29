﻿using System.Collections.Generic;
using UnityEngine;

public class EntityTracker : MonoBehaviour
{
    public CorridorSection section;

    [SerializeField]
    [ReadOnlyField]
    private List<GameObject> entitiesInArea = new List<GameObject>();

    [SerializeField]
    [ReadOnlyField]
    private GameObject tvManInArea;

    private bool initialCollisions = true;

    public bool TVManIsInArea { get { return tvManInArea != null; } }
    public List<GameObject> EntitiesInArea { get { return entitiesInArea; } }
    public GameObject TVManInArea 
    { 
        set 
        {
            tvManInArea = value;
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Entity")
        {
            AddDistinctEntities(other.gameObject);
        }
        if (other.tag == "TVMan")
        {
            tvManInArea = other.gameObject;
            NotifyTVMan();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Entity")
        {
            RemoveEntities(other.gameObject);
        }
        else if (other.gameObject.tag == "TVMan")
        {
            tvManInArea = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (initialCollisions && other.gameObject.tag == "TVMan")
        {
            NotifyTVMan();
            initialCollisions = false;
        }
    }

    public void NotifyTVMan() 
    {
        //print("update tvman!");
        //TVManController tempController = GameManager.current.tvMan;
        //bool tvNavMeshShouldBeDisabled = section.sectionType != SectionType.Middle;
        //tempController.UpdatePatrolAndAgent();
        //tempController.UseNavMesh = !tvNavMeshShouldBeDisabled;


    }

    public void AddDistinctEntities(params GameObject[] entities)
    {
        foreach (GameObject entity in entities)
        {
            if (!entitiesInArea.Contains(entity)) entitiesInArea.Add(entity);
        }
    }

    public void RemoveEntities(params GameObject[] entities)
    {
        foreach (GameObject entity in entities)
        {
            if (entitiesInArea.Contains(entity)) entitiesInArea.Remove(entity);
        }
    }

    public void RemoveAllEntities()
    {
        entitiesInArea = new List<GameObject>();
    }
}
