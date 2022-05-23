using UnityEngine;

[CreateAssetMenu(fileName = "PickupablesIndex", menuName = "ScriptableObjects/PickupablesData", order = 1)]
public class PickupablesIndex : ScriptableObject
{
    public PickupableInteractable[] Pickupables;
}
