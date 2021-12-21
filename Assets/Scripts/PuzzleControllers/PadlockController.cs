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

    public Animator lockAnimator;

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
            PickupableInteractable item = inventoryItem.RemoveItemFromContents(false);
            item.transform.position = Vector3.zero;
            InsertKey(item);
            PuzzleSolved = true;

        }
        else 
        {
            //Animation for checking lock only
            RotateThenBack();
        }
    }


    private async Task RotateLock(bool toFacePlayer = true) 
    {
        float positionValue = 0;
        float smoothedPositionValue;
        Quaternion initialRotation = lockFocus.rotation;
        Transform cameraTarget = GameManager.current.playerController.playerCamera.transform;
        Quaternion targetRotation = toFacePlayer ? Quaternion.LookRotation(cameraTarget.position - lockFocus.position) : lockFocusDefaultRot;

        while (positionValue < 1)
        {
            await Task.Yield();
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lockFocus.rotation = Quaternion.RotateTowards(initialRotation, targetRotation, smoothedPositionValue * 360f);
            
        }
        
        lockFocus.rotation = targetRotation;
    }


    private async void RotateThenBack() 
    {
        await RotateLock(true);
        await RotateLock(false);
        PadlockNotifier.IsInteractable = true;
    }

    private async void InsertKey(PickupableInteractable keyItem) 
    {
        await RotateLock(true);

        Transform tempCamRef = GameManager.current.trueCamera.transform;

        keyItem.transform.position = tempCamRef.position - new Vector3(0, 0.2f, 0);
        keyItem.transform.rotation = tempCamRef.rotation;
        keyItem.transform.SetParent(keyParent);

        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 initialRotation = new Vector3(0, 30, 0);
        Vector3 initialPosition = keyItem.transform.localPosition;
        Vector3 targetVector = Vector3.zero;

        while (positionValue < 1)
        {
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            keyItem.transform.localRotation = Quaternion.Euler(Vector3.Lerp(initialRotation, targetVector, smoothedPositionValue));
            keyItem.transform.localPosition = Vector3.Lerp(initialPosition, targetVector, smoothedPositionValue);
            await Task.Yield();
        }


        if (lockAnimator != null) lockAnimator.Play("Unlock");
        await Task.Delay(1000);

        RotateLock(false);

        positionValue = 0;

        Vector3 lockFocusPos = lockFocus.localPosition;
        Vector3 lockFocusPosTarget = lockFocus.localPosition - new Vector3(0, 5, 0);


        while (positionValue < 1)
        {
            positionValue += Time.deltaTime * 0.5f;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lockFocus.localPosition = Vector3.Lerp(lockFocusPos, lockFocusPosTarget, smoothedPositionValue);
            await Task.Yield();
        }

    }
}
