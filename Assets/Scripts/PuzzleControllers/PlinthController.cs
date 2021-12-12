using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlinthController : PuzzleElementController
{
    public PlinthNotifierAndItem[] Plinths;

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
            }

        }

        base.LoadPuzzleData(puzzleData);
    }

    private void AttatchItemToPlinth(PlinthNotifierAndItem currentPlinth, InteractableObject itemForPlinth, bool playAnimation = false) 
    {
        if (itemForPlinth.IsInteractable) itemForPlinth.IsInteractable = false;



        if (playAnimation)
        {
            print("not implemented but animation would play for the thing");
            AnimateItemToPoint(currentPlinth.PlynthNotifier.AssociatedTransform, new Vector3(0, 0.2f, 0));
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

    public async void AnimateItemToPoint(Transform transformToMove, Vector3 offset)
    {
        Vector3 finalTargetPosition = transformToMove.position;
        Vector3 targetPosition = finalTargetPosition + offset;
        
        //Rotation things
        Vector3 targetRotation = transformToMove.rotation.eulerAngles;
        Vector3 initialRotation = transformToMove.rotation.eulerAngles + new Vector3(0, Random.Range(0f, 1f) > 0.5f ? 180 : -180, 0);


        float pickupSpeedMultiplier = 1.5f;
        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 initalPosition;

        while (positionValue < 1f)
        {
            initalPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3(0, 0.6f, 0);
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);


            transformToMove.SetPositionAndRotation(
                Vector3.Lerp(initalPosition, targetPosition, smoothedPositionValue),
                Quaternion.Euler(Vector3.Lerp(initialRotation, targetRotation, smoothedPositionValue))
                );

            await Task.Yield();

        }

        if (offset != Vector3.zero)
        {
            positionValue = 0;
            initalPosition = transformToMove.position;

            while (positionValue < 1f)
            {
                positionValue += Time.deltaTime * pickupSpeedMultiplier;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);

                transformToMove.position = Vector3.Lerp(initalPosition, finalTargetPosition, smoothedPositionValue);

                await Task.Yield();
            }
        }

        transformToMove.SetPositionAndRotation(finalTargetPosition, Quaternion.Euler(targetRotation));
        
        if (PuzzleSolved) 
        {
            print("plinths move down!");
        }

    }
}

[System.Serializable]
public class PlinthNotifierAndItem 
{
    public PuzzleElementNotifier PlynthNotifier;
    public InteractableObject RequiredObject;
    public bool HasCorrectItem;
}

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
