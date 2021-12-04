using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableInteractable : InteractableObject
{
    public PickupType pickupType;
    public InventorySlot currentSlot;
    public float pickupSpeedMultiplier = 4f;
    public float pickupScaleMultiplier = 1f;

    private Vector3 PositionAtTimeOfPickup;
    private float positionValue = 0;
    private bool beingPickedUp;
    private InventorySlot lastFrameInventorySlot;

    private Collider pickupCollider;

    private Vector3 defaultScale;

    protected override void OnInteract()
    {
        bool isStandardMomento = pickupType == PickupType.Standard;

        if (isStandardMomento ? InventoryManager.current.AnyFreeInventorySlots : InventoryManager.current.AnyFreeMomentoSlots)
        {
            transform.parent = null;
            currentSlot = InventoryManager.current.MoveInteractableToInventory(this);
            IsInteractable = false;
            PositionAtTimeOfPickup = transform.position;
            beingPickedUp = true;
            positionValue = 0;
        }

        if (pickupCollider.enabled != !beingPickedUp) pickupCollider.enabled = !beingPickedUp;
    }

    protected void Awake()
    {
        defaultScale = transform.localScale;
        pickupCollider = GetComponent<Collider>();
    }

    protected void Update()
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
                transform.localScale = transform.localScale * pickupScaleMultiplier;
                if (currentSlot != null) currentSlot.ParentContentsToItemSlot();
                beingPickedUp = false;
            }
        }

        if (currentSlot == null && transform.localScale != defaultScale) 
        {
            transform.localScale = defaultScale;
        }
    }

}

public enum PickupType 
{
    Standard,
    Momento
}