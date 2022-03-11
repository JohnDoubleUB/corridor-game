using UnityEngine;

public interface IHuntableEntity
{
    GameObject EntityGameObject { get; }
    Transform EntityTransform { get; }
    bool IsIlluminated { get; }
    EntityType EntityType { get; }
    void OnBeingHunted(bool beingHunted);
    void OnEntityKilled();
}
