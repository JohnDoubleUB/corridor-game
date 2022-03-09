using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager current;


    public Transform ScreenCenter;
    public Transform[] inventorySlotTransforms;
    public Transform[] momentoSlotTransforms;

    public InventorySlot[] inventorySlots;
    public InventorySlot[] momentoSlots;

    public bool AllowReplace;
    public bool AnyFreeInventorySlots { get { return inventorySlots.Any(x => !x.SlotOccupied) || AllowReplace; } }
    public bool AnyFreeMomentoSlots { get { return momentoSlots.Any(x => !x.SlotOccupied) || AllowReplace; } }

    public bool HasMomento;

    private void Awake()
    {
        //Important instance things
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        //Establish the new slots
        if (inventorySlotTransforms.Any()) inventorySlots = inventorySlotTransforms.Select(x => new InventorySlot(x)).ToArray();
        if (momentoSlotTransforms.Any()) momentoSlots = momentoSlotTransforms.Select(x => new InventorySlot(x)).ToArray();
    }

    public InventorySlot MoveInteractableToInventory(PickupableInteractable interactable, out InteractableObject replacedObject)
    {
        //Depending on the type of item put in that inventory
        InventorySlot[] inventoryToHandle = interactable.pickupType == PickupType.Standard ? inventorySlots : momentoSlots;
        replacedObject = null;
        InventorySlot freeSlot = null;
        
        //Find a free inventory slot
        if (inventoryToHandle != null && inventoryToHandle.Any())
        {
            //Check for free slot
            freeSlot = inventoryToHandle.FirstOrDefault(x => !x.SlotOccupied);

            if (freeSlot != null)
            {
                freeSlot.ReplaceItemToContent(interactable, out replacedObject);
            }
            else if (AllowReplace)
            {
                freeSlot = inventoryToHandle[0].ReplaceItemToContent(interactable, out replacedObject);
            }
        }


        HasMomento = !AnyFreeMomentoSlots;

        return freeSlot;
    }

    public InventorySlot MoveInteractableToInventory(PickupableInteractable interactable)
    {
        return MoveInteractableToInventory(interactable, out InteractableObject _);
    }
}

[System.Serializable]
public class InventorySlot
{
    private Transform slotParentTransform;
    private Transform slotTransform;
    private Vector3 slotParentInitialPosition;
    public Transform SlotTransform { get { return slotTransform; } }
    public Transform SlotParentTransform { get { return slotParentTransform; } }
    public Vector3 SlotParentInitialPosition { get { return slotParentInitialPosition; } }
    public bool SlotOccupied { get { return slotContent != null; } }

    public PickupableInteractable slotContent;

    public InventorySlot(Transform slotTransform)
    {
        this.slotTransform = slotTransform;
        
        slotParentTransform = slotTransform.parent;
        slotParentInitialPosition = slotParentTransform.localPosition;
    }

    public InventorySlot AddItemToContent(PickupableInteractable interactable)
    {
        slotContent = interactable;
        return this;
    }

    public void ParentContentsToItemSlot()
    {
        if (slotContent != null)
        {
            slotContent.transform.SetParent(slotTransform);
            slotContent.transform.localPosition = Vector3.zero;
            slotContent.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }


    public InventorySlot ReplaceItemToContent(PickupableInteractable interactable, out InteractableObject replacedObject)
    {
        replacedObject = slotContent != null ? RemoveItemFromContents() : null;
        AddItemToContent(interactable);
        return this;
    }

    public PickupableInteractable RemoveItemFromContents(bool makeInteractable = true)
    {
        PickupableInteractable objectToRemove = slotContent;
        slotContent = null;
        objectToRemove.transform.SetParent(null);
        objectToRemove.IsInteractable = makeInteractable;
        objectToRemove.currentSlot = null;
        return objectToRemove;
    }
}