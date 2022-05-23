using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueWithSubs", menuName = "ScriptableObjects/DialogueWithSubs", order = 1)]
public class DialogueWithSubtitles : ScriptableObject
{
    public Color[] SubtitleColours;
    public List<Conversation> Conversations;
}

[System.Serializable]
public class Dialogue
{
    public AudioClip DialogueAudio;
    public string Subtitles;
    public int ConversationNo;
    public int PartNo;
    public int SubtitleColour = 0;


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

        PartNo = int.TryParse(dialoguePartString, out int dPartResult) ? dPartResult : -1;
        ConversationNo = int.TryParse(splitText[0], out int conNoResult) ? conNoResult : -1;
    }
}

[System.Serializable]
public class DialoguePart
{
    public int PartNo;
    public List<Dialogue> Dialogues;

    public DialoguePart(params Dialogue[] associatedDialogues)
    {
        if (associatedDialogues.Any())
        {
            Dialogues = associatedDialogues.ToList();
            PartNo = Dialogues[0].PartNo;
        }
    }
}

[System.Serializable]
public class Conversation
{
    public string ConversationName;
    public int ConversationNo;
    public List<DialoguePart> DialogueParts = new List<DialoguePart>();

    public Conversation(params Dialogue[] associatedDialogues) 
    {
        DialoguePart dialoguePart = null;
        Dialogue[] orderdDialogues = associatedDialogues.OrderBy(x => x.PartNo).ToArray();
        ConversationNo = associatedDialogues[0].ConversationNo;
        ConversationName = ConversationNo + "_" + associatedDialogues[0].Subtitles;


        for (int i = 0; i < orderdDialogues.Length; i++) 
        {
            Dialogue d = orderdDialogues[i];

            if (dialoguePart == null)
            {
                dialoguePart = new DialoguePart(d);
                if (i == orderdDialogues.Length - 1) DialogueParts.Add(dialoguePart);
            }
            else if (dialoguePart.PartNo != d.PartNo)
            {
                DialogueParts.Add(dialoguePart);
                dialoguePart = new DialoguePart(d);
                if (i == orderdDialogues.Length - 1) DialogueParts.Add(dialoguePart);
            }
            else
            {
                dialoguePart.Dialogues.Add(d);
                if (i == orderdDialogues.Length - 1) DialogueParts.Add(dialoguePart);
            }
        }

    }
}


