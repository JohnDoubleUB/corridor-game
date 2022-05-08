using UnityEngine;

public interface IHuntableEntity
{
    GameObject EntityGameObject { get; }
    Transform EntityTransform { get; }
    Vector3 EntityColliderPosition { get; }
    bool IsIlluminated { get; }
    EntityType EntityType { get; }
    void OnBeingHunted(bool beingHunted);
    void OnEntityKilled();
}
