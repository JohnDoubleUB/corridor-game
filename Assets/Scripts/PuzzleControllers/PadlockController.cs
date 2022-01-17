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

    public AudioClip LockKeySlideSound;
    public AudioClip LockUnlockSound;
    public AudioClip LockDropSound;
    public AudioClip GenericLockSound;
    public float soundVolume = 1f;


    private Vector3 keyParentDefaultPos;
    private Quaternion keyParentDefaultRot;

    private Vector3 lockFocusDefaultPos;
    private Quaternion lockFocusDefaultRot;

    private Vector3 lockHoleDefaultPos;
    private Quaternion lockHoleDefaultRot;

    float pickupSpeedMultiplier = 1.5f;

    private bool beingSolved;

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
        lockFocusDefaultRot = lockFocus.rotation;
        InventorySlot inventoryItem = InventoryManager.current.inventorySlots.Where(x => x.SlotOccupied && x.slotContent.ObjectName == RequiredObject.ObjectName).FirstOrDefault();
        PadlockNotifier.IsInteractable = false;


        if (inventoryItem != null)
        {
            //Checking lock and putting in key and twisting
            PickupableInteractable item = inventoryItem.RemoveItemFromContents(false);
            item.transform.position = Vector3.zero;
            InsertKey(item);
        }
        else 
        {
            //Animation for checking lock only
            RotateThenBack();
        }
    }


    private async Task RotateLock(bool toFacePlayer = true) 
    {

        print("rotating");
        float positionValue = 0;
        float smoothedPositionValue;
        Quaternion initialRotation = lockFocus.rotation;
        Transform cameraTarget = GameManager.current.playerController.playerCamera.transform;
        Quaternion targetRotation = toFacePlayer ? Quaternion.LookRotation(cameraTarget.position - lockFocus.position) : lockFocusDefaultRot;

        if (GenericLockSound != null) AudioManager.current.PlayClipAt(GenericLockSound, transform.position, soundVolume/2).transform.SetParent(transform);

        while (positionValue < 1)
        {
            await Task.Yield();
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lockFocus.rotation = Quaternion.RotateTowards(initialRotation, targetRotation, smoothedPositionValue * 360f);
            
        }
        
        lockFocus.rotation = targetRotation;
    }

    private void RotateLockBack() 
    {
        Task tc = RotateLock(false);
    }

    private async void RotateThenBack() 
    {
        await RotateLock(true);
        await RotateLock(false);
        PadlockNotifier.IsInteractable = true;
    }

    private async void InsertKey(PickupableInteractable keyItem) 
    {
        beingSolved = true;
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

        //if (LockKeySlideSound != null) 
        //{ 
        //    AudioManager.current.PlayClipAt(LockKeySlideSound, transform.position, soundVolume, true, 0.2f).transform.SetParent(transform);
        //    AudioManager.current.PlayClipAt(LockKeySlideSound, transform.position, soundVolume, true, 0.6f).transform.SetParent(transform);
        //}

        while (positionValue < 1)
        {
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            keyItem.transform.localRotation = Quaternion.Euler(Vector3.Lerp(initialRotation, targetVector, smoothedPositionValue));
            keyItem.transform.localPosition = Vector3.Lerp(initialPosition, targetVector, smoothedPositionValue);
            await Task.Yield();
        }


        if (lockAnimator != null) lockAnimator.Play("Unlock");
        //await Task.Delay(1000);
        //if (LockUnlockSound != null) AudioManager.current.PlayClipAt(LockUnlockSound, transform.position, soundVolume).transform.SetParent(transform);
        //await Task.Delay(500);

        //Task temp = RotateLock(false);

        //positionValue = 0;

        //Vector3 lockFocusPos = lockFocus.localPosition;
        //Vector3 lockFocusPosTarget = lockFocus.localPosition - new Vector3(0, 5, 0);

        //PuzzleSolved = true;

        //if (LockDropSound != null) AudioManager.current.PlayClipAt(LockDropSound, transform.position, soundVolume).transform.SetParent(transform);

        //while (positionValue < 1)
        //{
        //    positionValue += Time.deltaTime * 0.5f;
        //    smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
        //    lockFocus.localPosition = Vector3.Lerp(lockFocusPos, lockFocusPosTarget, smoothedPositionValue);
        //    await Task.Yield();
        //}


        
    }

    public async void DropLock() 
    {

        //Task temp = RotateLock(false);

        float positionValue = 0;
        float smoothedPositionValue;

        Vector3 lockFocusPos = lockFocus.localPosition;
        Vector3 lockFocusPosTarget = lockFocus.localPosition - new Vector3(0, 5, 0);

        PuzzleSolved = true;

        if (LockDropSound != null) AudioManager.current.PlayClipAt(LockDropSound, transform.position, soundVolume).transform.SetParent(transform);

        while (positionValue < 1)
        {
            positionValue += Time.deltaTime * 0.5f;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lockFocus.localPosition = Vector3.Lerp(lockFocusPos, lockFocusPosTarget, smoothedPositionValue);
            await Task.Yield();
        }
    }

    public override void LoadPuzzleData(PuzzleElementControllerData puzzleData)
    {
        base.LoadPuzzleData(puzzleData);

        if (PuzzleSolved) 
        {
            gameObject.SetActive(false);
        }
    }
    private void PlayLockKeySlideSound()
    {
        if (LockKeySlideSound != null) AudioManager.current.PlayClipAt(LockKeySlideSound, transform.position, soundVolume).transform.SetParent(transform);
    }

    private void PlayLockUnlockSound()
    {
        if (LockUnlockSound != null) AudioManager.current.PlayClipAt(LockUnlockSound, transform.position, soundVolume).transform.SetParent(transform);
    }

    private void PlayGenericLockSound()
    {
        if (GenericLockSound != null) AudioManager.current.PlayClipAt(GenericLockSound, transform.position, soundVolume).transform.SetParent(transform);
    }

    private void PlaySoundTest(AudioClip test) { }

    private void OnDestroy()
    {
        if (beingSolved && beingSolved != PuzzleSolved) PuzzleSolved = true;
    }
}
