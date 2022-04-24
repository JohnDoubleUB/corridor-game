using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomCorridorEventScript : MonoBehaviour
{
    public string EventTag;
    public abstract void TriggerCustomEvent();

    protected bool TriggerEvent() 
    {
        return CorridorChangeManager.current.AddEventTag(EventTag);
    }
}
