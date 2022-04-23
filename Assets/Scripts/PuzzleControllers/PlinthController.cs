using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlinthController : PuzzleElementController
{
    public PlinthNotifierAndItem[] Plinths;
    public AudioClip[] PlinthItemPlaceSounds;
    public AudioClip[] PlinthSlideDownSounds;
    public AudioClip PuzzleCompletedSound;

    public float plinthLowerAmount = 1f;
    float pickupSpeedMultiplier = 1.5f;

    private bool plinthsAreDown;

    private int plinthCompleteCount;

    private void Awake()
    {
        for (int i = 0; i < Plinths.Length; i++)
        {
            PuzzleElementNotifier notifierElement = Plinths[i].PlynthNotifier;
            notifierElement.NotifierId = i;
            notifierElement.puzzleToNotify = this;
        }
    }

    public override void Notify(PuzzleElementNotifier notifier = null)
    {
        if (notifier != null)
        {
            PlinthNotifierAndItem currentPItem = Plinths[notifier.NotifierId];
            InventorySlot plinthItemSlot = InventoryManager.current.inventorySlots.Where(x => x.SlotOccupied && x.slotContent.ObjectName == currentPItem.RequiredObject.ObjectName).FirstOrDefault();

            if (plinthItemSlot != null)
            {
                PickupableInteractable plinthItem = plinthItemSlot.RemoveItemFromContents(false);
                AttatchItemToPlinth(currentPItem, plinthItem, true);

                //Check if all plinths are now correct, if so then set Puzzle as solved
                if (Plinths.All(x => x.HasCorrectItem))
                {
                    PuzzleSolved = true;
                }
                else
                {
                    OnPuzzleUpdated();
                }

            }
        }

    }

    public override void LoadPuzzleData(PuzzleElementControllerData puzzleData)
    {
        PlinthControllerData plinthControllerData = puzzleData as PlinthControllerData;

        if (plinthControllerData != null)
        {
            for (int i = 0; i < Plinths.Length; i++)
            {
                if (!plinthControllerData.PlinthItemsPlaced[i]) continue;
                PlinthNotifierAndItem currentPlinth = Plinths[i];
                AttatchItemToPlinth(currentPlinth, Instantiate(currentPlinth.RequiredObject));
                plinthCompleteCount++;
            }

        }

        if (puzzleData.PuzzleSolved) LowerPlinths();

        base.LoadPuzzleData(puzzleData);
    }

    private void AttatchItemToPlinth(PlinthNotifierAndItem currentPlinth, InteractableObject itemForPlinth, bool playAnimation = false)
    {
        if (itemForPlinth.IsInteractable) itemForPlinth.IsInteractable = false;



        if (playAnimation)
        {
            AnimateItemToPoint(currentPlinth, new Vector3(0, 0.2f, 0));
        }

        itemForPlinth.transform.SetParent(currentPlinth.PlynthNotifier.AssociatedTransform);
        itemForPlinth.transform.localPosition = Vector3.zero;
        itemForPlinth.transform.localRotation = Quaternion.identity;
        currentPlinth.HasCorrectItem = true;
        currentPlinth.PlynthNotifier.IsInteractable = false;
    }

    public override void OnPuzzleUpdated()
    {
        LayoutHandler.UpdatePuzzleData(this, (PlinthControllerData)this);
    }


    public async void AnimatePlinthAndItem(Transform transformToMove)
    {
        await Task.Yield();

        //await AnimateItemToPoint(transformToMove)
    }
    
    public async void AnimateItemToPoint(PlinthNotifierAndItem plinthAndItem, Vector3 offset)
    {
        Transform transformToMove = plinthAndItem.PlynthNotifier.AssociatedTransform;

        Vector3 finalTargetPosition = transformToMove.localPosition;
        Vector3 targetPosition = finalTargetPosition + offset;

        bool leftOrRight = Random.Range(0f, 1f) > 0.5f;

        //Rotation things
        Vector3 targetRotation = transformToMove.localRotation.eulerAngles;
        Vector3 rotationOvershoot = transformToMove.localRotation.eulerAngles + new Vector3(0, leftOrRight ? -20 : 20, 0);
        Vector3 initialRotation = transformToMove.localRotation.eulerAngles + new Vector3(0, leftOrRight ? 180 : -180, 0);

        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 initalPosition;

        while (positionValue < 1f)
        {
            initalPosition = (GameManager.current.playerController.playerCamera.transform.position - plinthAndItem.PlynthNotifier.transform.position) - new Vector3(0, 0.6f, 0);

            positionValue += Time.deltaTime * pickupSpeedMultiplier;

            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);

            transformToMove.localPosition = Vector3.Lerp(initalPosition, targetPosition, smoothedPositionValue);
            transformToMove.localRotation = Quaternion.Euler(Vector3.Lerp(initialRotation, rotationOvershoot, smoothedPositionValue));

            await Task.Yield();

        }

        if (offset != Vector3.zero)
        {
            positionValue = 0;

            while (positionValue < 1f)
            {
                positionValue += Time.deltaTime * pickupSpeedMultiplier;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);

                transformToMove.localPosition = Vector3.Lerp(targetPosition, finalTargetPosition, smoothedPositionValue);
                transformToMove.localRotation = Quaternion.Euler(Vector3.Lerp(rotationOvershoot, targetRotation, smoothedPositionValue));

                await Task.Yield();
            }
        }

        transformToMove.localPosition = finalTargetPosition;
        transformToMove.localRotation = Quaternion.Euler(targetRotation);

        plinthCompleteCount++;

        //Sound effect!
        int tempNotifId = plinthAndItem.PlynthNotifier.NotifierId;
        if (PlinthItemPlaceSounds != null && PlinthItemPlaceSounds.Any() && tempNotifId < PlinthItemPlaceSounds.Length)
            AudioManager.current.PlayClipAt(PlinthItemPlaceSounds[tempNotifId], transformToMove.position, 1f, false).transform.SetParent(transform);
        

        if (PuzzleSolved && !plinthsAreDown && plinthCompleteCount > 2)
        {
            plinthsAreDown = true;
            LowerPlinths(true);
        }

    }

    private async void LowerPlinths(bool playAnimation = false) 
    {
        //Wait for a few seconds

        if (!playAnimation)
        {
            await Task.Yield();
            foreach (PlinthNotifierAndItem p in Plinths)
            {
                p.PlynthNotifier.transform.position += new Vector3(0, -plinthLowerAmount, 0);
            }
        }
        else 
        {
            if(PuzzleCompletedSound != null) transform.PlayClipAtTransform(PuzzleCompletedSound, true, 1f, false, 0.3f, false);

            float pickupSpeedMax = pickupSpeedMultiplier / 2;
            float pickupSpeedMin = pickupSpeedMax - (pickupSpeedMax * 0.2f);
            
            //Play plinth down animation!
            foreach (PlinthNotifierAndItem p in Plinths)
            {
                MoveObject(p.PlynthNotifier.transform, 
                    new Vector3(0, -plinthLowerAmount, 0), 
                    Random.Range(0f, 0.5f), 
                    Random.Range(pickupSpeedMin, pickupSpeedMax), 
                    PlinthSlideDownSounds != null && PlinthSlideDownSounds.Any() && p.PlynthNotifier.NotifierId < PlinthSlideDownSounds.Length ? 
                    PlinthSlideDownSounds[p.PlynthNotifier.NotifierId] : 
                    null);
            }
        }

    }


    private async void MoveObject(Transform ObjectToMove, Vector3 offset, float initialDelay = 1, float speedMultiplier = 1f, AudioClip movementSound = null)
    {
        float timer = 0;
        while (timer < initialDelay) 
        { 
            timer += Time.deltaTime;
            await Task.Yield();
        }

        AudioSource tempAudioSource = movementSound != null ? AudioManager.current.PlayClipAt(movementSound, ObjectToMove.position, 1f, false) : null;

        if (tempAudioSource != null) tempAudioSource.transform.SetParent(ObjectToMove);

        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 initalPosition = ObjectToMove.transform.localPosition;
        Vector3 targetPosition = ObjectToMove.transform.localPosition + offset;

        while (positionValue < 1f)
        {
            positionValue += Time.deltaTime * speedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            ObjectToMove.localPosition = Vector3.Lerp(initalPosition, targetPosition, smoothedPositionValue);
            await Task.Yield();
        }

        if (tempAudioSource != null) tempAudioSource.Stop();

    }
}

[System.Serializable]
public class PlinthNotifierAndItem
{
    public PuzzleElementNotifier PlynthNotifier;
    public InteractableObject RequiredObject;
    public bool HasCorrectItem;
}

[System.Serializable]
public class PlinthControllerData : PuzzleElementControllerData
{
    public bool[] PlinthItemsPlaced;
    public PlinthControllerData(PlinthController plinthController) : base(plinthController)
    {
        PlinthItemsPlaced = plinthController.Plinths.Select(x => x.HasCorrectItem).ToArray();
    }

    public static implicit operator PlinthControllerData(PlinthController plinthController)
    {
        return new PlinthControllerData(plinthController);
    }
}
