using UnityEngine;

public class TriggerPlayerPrompt : MonoBehaviour
{
    public bool promptShouldLinger;
    public PromptType promptType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            switch (promptType) 
            {
                case PromptType.CrouchPrompt:
                    GameManager.current.playerController.DisplayCrouchPrompt(promptShouldLinger);
                    break;

                default:
                case PromptType.NotepadPrompt:
                    GameManager.current.playerController.DisplayNotepadPrompt(promptShouldLinger);
                    break;
            }
        }
    }


    public enum PromptType 
    {
        NotepadPrompt,
        CrouchPrompt
    }

}