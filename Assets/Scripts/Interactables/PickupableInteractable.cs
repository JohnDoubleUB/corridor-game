using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PickupableInteractable : InteractableObject
{
    public PickupType pickupType;
    public InventorySlot currentSlot;
    public float pickupSpeedMultiplier = 2.5f;
    public float pickupScaleMultiplier = 1f;
    public AudioClip pickupSound;
    public bool varyPickupSoundPitch;
    public float pickupSoundVolume = 0.5f;

    private bool beingPickedUp;
    private Collider pickupCollider;
    private Vector3 defaultScale;

    protected override void OnInteract()
    {
        bool isStandardMomento = pickupType == PickupType.Standard;

        if (isStandardMomento ? InventoryManager.current.AnyFreeInventorySlots : InventoryManager.current.AnyFreeMomentoSlots) Pickup();

        if (pickupCollider.enabled != !beingPickedUp) pickupCollider.enabled = !beingPickedUp;
    }

    protected void Awake()
    {
        defaultScale = transform.localScale;
        pickupCollider = GetComponent<Collider>();
    }

    protected void Update()
    {
        if (currentSlot == null && transform.localScale != defaultScale) 
        {
            transform.localScale = defaultScale;
        }
    }

    private async void Pickup() 
    {
        OnSuccessfulInteract();
        transform.parent = null;
        currentSlot = InventoryManager.current.MoveInteractableToInventory(this);
        IsInteractable = false;
        beingPickedUp = true;

        if (pickupSound != null)
        {
            transform.PlayClipAtTransform(pickupSound, true, pickupSoundVolume, varyPickupSoundPitch);
            //AudioManager.current.PlayClipAt(pickupSound, transform.position, pickupSoundVolume, varyPickupSoundPitch).transform.SetParent(transform); 
        }

        Vector3 positionAtTimeOfPickup = transform.position;
        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 cameraPosition;
        

        while (positionValue < 1f) 
        {
            cameraPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3(0, 0.6f, 0);
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            transform.SetPositionAndRotation(
                Vector3.Lerp(positionAtTimeOfPickup, cameraPosition, smoothedPositionValue), 
                Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(cameraPosition - transform.position), smoothedPositionValue * 50f)
                );
            await Task.Yield();
        }

        transform.localScale = transform.localScale * pickupScaleMultiplier;
        if (currentSlot != null) currentSlot.ParentContentsToItemSlot();
        beingPickedUp = false;
    }

}

public enum PickupType 
{
    Standard,
    Momento
}