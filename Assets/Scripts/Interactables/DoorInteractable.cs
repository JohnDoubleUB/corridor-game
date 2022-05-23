public class DoorInteractable : InteractableObject
{
    public Door door;
    protected override void OnInteract()
    {
        if (door != null) door.InteractOpenClose();
    }
}
