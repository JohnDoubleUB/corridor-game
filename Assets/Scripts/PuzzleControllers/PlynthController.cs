using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlynthController : PuzzleElementController
{
    public PlythNotifierAndItem[] Plynths;

    private void Awake()
    {
        for (int i = 0; i < Plynths.Length; i++) 
        {
            PuzzleElementNotifier notifierElement = Plynths[i].PlynthNotifier;
            notifierElement.NotifierId = i;
            notifierElement.puzzleToNotify = this;
        }
    }

    public override void Notify(PuzzleElementNotifier notifier = null)
    {
        print("hiii");
        if (notifier != null) 
        {
            PlythNotifierAndItem currentPItem = Plynths[notifier.NotifierId];
            InventorySlot plynthItemSlot = InventoryManager.current.inventorySlots.Where(x => x.SlotOccupied && x.slotContent.ObjectName == currentPItem.RequiredObject.ObjectName).FirstOrDefault();

            if (plynthItemSlot != null) 
            {
                PickupableInteractable plynthItem = plynthItemSlot.RemoveItemFromContents(false);
                plynthItem.transform.SetParent(currentPItem.PlynthNotifier.AssociatedTransform);
                plynthItem.transform.localPosition = Vector3.zero;
                currentPItem.HasCorrectItem = true;
                currentPItem.PlynthNotifier.IsInteractable = false;
            }
        }

    }

    public override void LoadPuzzleData(PuzzleElementControllerData puzzleData)
    {
        //PlynthController plynthController = puzzleData as PlynthController;
        //if (plynthController != null)
        //{
        //    disabledButtons = numberpadData.DisabledButtons.ToList();
        //}


        base.LoadPuzzleData(puzzleData);
    }



}

[System.Serializable]
public class PlythNotifierAndItem 
{
    public PuzzleElementNotifier PlynthNotifier;
    public InteractableObject RequiredObject;
    public bool HasCorrectItem;
}
