using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHuntableEntities
{
    Transform EntityTransform { get; }
    bool IsIlluminated { get; }
    EntityType EntityType { get; }
    void OnBeingHunted();
    void OnEntityKilled();
}
