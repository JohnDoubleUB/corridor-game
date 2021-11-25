using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableInteractable : InteractableObject
{
    public PickupType pickupType;
    public InventorySlot currentSlot;

    private Vector3 PositionAtTimeOfPickup;
    private float positionValue = 0;
    private bool beingPickedUp;

    protected override void OnInteract()
    {
        bool isStandardMomento = pickupType == PickupType.Standard;

        if (isStandardMomento ? InventoryManager.current.AnyFreeInventorySlots : InventoryManager.current.AnyFreeMomentoSlots)
        {
            transform.parent = null;
            currentSlot = InventoryManager.current.MoveInteractableToInventory(this);
            IsInteractable = false;
            PositionAtTimeOfPickup = transform.position;
            currentSlot.ParentContentsToItemSlot();
            beingPickedUp = true;
            positionValue = 0;
        }
    }

    private void Update()
    {
        //Vector3 cameraPosition = GameManager.current.playerController.playerCamera.transform.position;
        //transform.LookAt(cameraPosition);
        if (beingPickedUp) 
        {
            if (positionValue < 1f)
            {
                Vector3 cameraPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3 (0, 0.2f, 0);
                positionValue += Time.deltaTime * 5f;
                transform.position = Vector3.Lerp(PositionAtTimeOfPickup, cameraPosition, positionValue);
                transform.LookAt(cameraPosition);

            }
            else 
            {
                if (currentSlot != null) currentSlot.ParentContentsToItemSlot();
                beingPickedUp = false;
            }
        }
    }

}

public enum PickupType 
{
    Standard,
    Momento
}