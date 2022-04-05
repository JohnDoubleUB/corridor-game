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


        UpdateMomentoStatus();

        return freeSlot;
    }

    public InventorySlot MoveInteractableToInventory(PickupableInteractable interactable)
    {
        return MoveInteractableToInventory(interactable, out InteractableObject _);
    }

    public void UpdateMomentoStatus() 
    {
        HasMomento = !AnyFreeMomentoSlots;
    }

    public void LoadSavedInventoryData(IEnumerable<InventoryItemData> inventoryItemData, IEnumerable<InventoryItemData> momentoItemData) 
    {
        CreateInventoryItemsFromData(inventoryItemData);
        CreateInventoryItemsFromData(momentoItemData, PickupType.Momento);
    }

    public void CreateInventoryItemsFromData(IEnumerable<InventoryItemData> itemData, PickupType pickupType = PickupType.Standard)
    {
        if (itemData != null && itemData.Any())
        {
            bool itemsAreMomentos = pickupType == PickupType.Momento;

            foreach (InventoryItemData inventoryItem in itemData.Where(x => x.PickupableData != null))
            {
                PickupableInteractable itemToSpawn = GameManager.current.PickupablesIndex.Pickupables.FirstOrDefault(x => x.PickupID == inventoryItem.PickupableData.PickupID);
                if (itemToSpawn != null)
                {
                    PickupableInteractable spawnedItem = Instantiate(itemToSpawn);
                    spawnedItem.LoadItemData(inventoryItem.PickupableData);
                    spawnedItem.IsInteractable = false;

                    //Get the appropriate inventory slot
                    spawnedItem.currentSlot = (itemsAreMomentos ? momentoSlots : inventorySlots).ElementAt(inventoryItem.SlotIndex).AddItemToContent(spawnedItem); //Add this item to it

                    //Then finish the process of adding the item on the item itself
                    spawnedItem.SetInInventory();

                    if (itemsAreMomentos) 
                    {
                        UpdateMomentoStatus(); 
                    }//This should fix the bug with tvman killing you even if you have a momento (Hopefully)
                }
            }
        }
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

[System.Serializable]
public class InventoryData
{
    public InventoryItemData[] InventoryItems;
    public InventoryItemData[] MomentoItems;

    public InventoryData(InventoryManager inventory)
    {
        InventoryItems = inventory.inventorySlots.Select((x, index) => new InventoryItemData(x, index)).ToArray();
        MomentoItems = inventory.momentoSlots.Select((x, index) => new InventoryItemData(x, index)).ToArray();
    }
}

[System.Serializable]
public class InventoryItemData
{
    public int SlotIndex;
    public PickupableData PickupableData;

    public InventoryItemData(InventorySlot InventorySlot, int SlotIndex = 0)
    {
        this.SlotIndex = SlotIndex;
        PickupableData = InventorySlot.slotContent != null ? new PickupableData(InventorySlot.slotContent) : null;
    }
}