using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueWithSubs", menuName = "ScriptableObjects/DialogueWithSubs", order = 1)]
public class DialogueWithSubtitles : ScriptableObject
{
    public List<Dialogue> Dialogue;
    public Conversation[] Conversations;
}

[System.Serializable]
public class Dialogue 
{
    public AudioClip DialogueAudio;
    public string Subtitles;
    public int ConversationNo;
    public int DialoguePart;


    public Dialogue(string Subtitles, AudioClip DialogueAudio) 
    {
        this.DialogueAudio = DialogueAudio;
        this.Subtitles = Subtitles;

        string[] splitText = DialogueAudio.name.Split('.');
        string dialoguePartString = "";

        foreach (char c in splitText[1]) 
        {
            if (!char.IsDigit(c)) break;
            dialoguePartString += c;
        }

        DialoguePart = int.TryParse(dialoguePartString, out int dPartResult) ? dPartResult : -1;
        ConversationNo = int.TryParse(splitText[0], out int conNoResult) ? conNoResult : -1;
    }
}

[System.Serializable]
public class DialogueSection 
{
    public Dialogue[] Dialogues;

    public DialogueSection(params Dialogue[] associatedDialogues) 
    {
        if (associatedDialogues.Any()) 
        {
            Dialogues = associatedDialogues;



        }
    }
}

[System.Serializable]
public class Conversation 
{
    DialogueSection[] DialogueSections;
}


