using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


                AttatchItemToPlinth(currentPItem, plinthItem);

                //plinthItem.transform.SetParent(currentPItem.PlynthNotifier.AssociatedTransform);
                //plinthItem.transform.localPosition = Vector3.zero;
                //currentPItem.HasCorrectItem = true;
                //currentPItem.PlynthNotifier.IsInteractable = false;
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
        }

        itemForPlinth.transform.SetParent(currentPlinth.PlynthNotifier.AssociatedTransform);
        itemForPlinth.transform.localPosition = Vector3.zero;
        itemForPlinth.transform.localRotation = Quaternion.identity;
        currentPlinth.HasCorrectItem = true;
        currentPlinth.PlynthNotifier.IsInteractable = false;
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
