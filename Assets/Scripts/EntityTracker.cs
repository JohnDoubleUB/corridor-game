using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityTracker : MonoBehaviour
{
    [SerializeField]
    [ReadOnlyField]
    private List<GameObject> entitiesInArea = new List<GameObject>();

    [SerializeField]
    [ReadOnlyField]
    private GameObject tvManInArea;

    public bool TVManIsInArea { get { return tvManInArea != null; } }
    public List<GameObject> EntitiesInArea { get { return entitiesInArea; } }
    public GameObject TVManInArea { set { tvManInArea = value; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Entity")
        {
            AddDistinctEntities(other.gameObject);
        }
        else if (other.gameObject.tag == "TVMan")
        {
            tvManInArea = other.gameObject;
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
