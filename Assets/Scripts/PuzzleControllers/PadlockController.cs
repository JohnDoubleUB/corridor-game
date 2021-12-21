using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PadlockController : PuzzleElementController
{
    public PuzzleElementNotifier PadlockNotifier;
    public InteractableObject RequiredObject;
    
    public Transform keyParent;
    public Transform lockFocus;
    public Transform lockHole;

    public float lockRotationUnlockOffset = 30f;

    private Vector3 keyParentDefaultPos;
    private Quaternion keyParentDefaultRot;

    private Vector3 lockFocusDefaultPos;
    private Quaternion lockFocusDefaultRot;

    private Vector3 lockHoleDefaultPos;
    private Quaternion lockHoleDefaultRot;

    float pickupSpeedMultiplier = 1.5f;

    private void Awake()
    {
        if (PadlockNotifier != null) PadlockNotifier.puzzleToNotify = this;

        if (keyParent != null) 
        {
            keyParentDefaultPos = keyParent.localPosition;
            keyParentDefaultRot = keyParent.localRotation;
        }

        if (lockFocus != null) 
        {
            lockFocusDefaultPos = lockFocus.localPosition;
            lockFocusDefaultRot = lockFocus.rotation;
        }

        if (lockHole != null)
        {
            lockHoleDefaultPos = lockHole.localPosition;
            lockHoleDefaultRot = lockHole.localRotation;
        }

    }


    public override void Notify(PuzzleElementNotifier notifier = null)
    {
        InventorySlot inventoryItem = InventoryManager.current.inventorySlots.Where(x => x.SlotOccupied && x.slotContent.ObjectName == RequiredObject.ObjectName).FirstOrDefault();
        PadlockNotifier.IsInteractable = false;


        if (inventoryItem != null)
        {
            //Checking lock and putting in key and twisting
        }
        else 
        {
            //Animation for checking lock only
            RotateLockToFacePlayer();
        }
    }


    public async void RotateLockToFacePlayer() 
    {
        float positionValue = 0;
        float smoothedPositionValue;
        Quaternion initialRotation = lockFocus.rotation;

        Transform cameraTarget = GameManager.current.playerController.playerCamera.transform;

        while (positionValue < 1f) 
        {
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lockFocus.rotation = Quaternion.RotateTowards(initialRotation, Quaternion.LookRotation(cameraTarget.position - lockFocus.position), smoothedPositionValue * 200f);
            await Task.Yield();
        }
        lockFocus.LookAt(cameraTarget);
    }
}
