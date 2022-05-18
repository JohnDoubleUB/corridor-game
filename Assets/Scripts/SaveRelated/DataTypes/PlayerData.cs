[System.Serializable]
public class PlayerData
{
    public bool NotepadPickedUp; //Stores whether the player has picked up the notepad

    public PlayerData(CG_CharacterController characterController)
    {
        NotepadPickedUp = characterController.NotepadPickedUp;
    }
}
