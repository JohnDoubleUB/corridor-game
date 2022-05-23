[System.Serializable]
public class PlayerData
{
    public bool NotepadPickedUp; //Stores whether the player has picked up the notepad
    public bool VisibilityPromptEnabled;

    public PlayerData() { }

    public PlayerData(CG_CharacterController characterController)
    {
        NotepadPickedUp = characterController.NotepadPickedUp;
        VisibilityPromptEnabled = characterController.VisibilityPromptEnabled;
    }
}
