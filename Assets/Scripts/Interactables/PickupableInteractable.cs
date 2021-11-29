using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableInteractable : InteractableObject
{
    public PickupType pickupType;
    public InventorySlot currentSlot;
    public float pickupSpeedMultiplier = 4f;

    private Vector3 PositionAtTimeOfPickup;
    private float positionValue = 0;
    private bool beingPickedUp;

    private Collider pickupCollider;


    protected override void OnInteract()
    {
        bool isStandardMomento = pickupType == PickupType.Standard;

        if (isStandardMomento ? InventoryManager.current.AnyFreeInventorySlots : InventoryManager.current.AnyFreeMomentoSlots)
        {
            print("yes");
            transform.parent = null;
            currentSlot = InventoryManager.current.MoveInteractableToInventory(this);
            IsInteractable = false;
            PositionAtTimeOfPickup = transform.position;
            beingPickedUp = true;
            positionValue = 0;
        }

        if (pickupCollider.enabled != !beingPickedUp) pickupCollider.enabled = !beingPickedUp;
    }

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        //Vector3 cameraPosition = GameManager.current.playerController.playerCamera.transform.position;
        //transform.LookAt(cameraPosition);
        if (beingPickedUp) 
        {
            if (positionValue < 1f)
            {
                Vector3 cameraPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3 (0, 0.6f, 0);
                positionValue += Time.deltaTime * pickupSpeedMultiplier;
                float smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                transform.position = Vector3.Lerp(PositionAtTimeOfPickup, cameraPosition, smoothedPositionValue);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(cameraPosition - transform.position), smoothedPositionValue * 50f);
            }
            else 
            {
                print("hi");
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