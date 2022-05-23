using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioDialogueLoader : MonoBehaviour
{
    public AudioClip[] JohnCasterClips;
    public AudioClip[] TheEntityClips;

    public TextAsset CasterSubtitlesText;
    public TextAsset TheEntitySubtitlesText;

    public Dictionary<string, string> JohnCasterSubtitlesConverted;

    public DialogueWithSubtitles CasterSubtitles;
    public DialogueWithSubtitles TheEntitySubtitles;

    // Start is called before the first frame update
    void Start()
    {
        print("Loading in dialogue!");
        TheEntitySubtitles.Conversations = GetConversations(GetDialogues(
            JsonConvert.DeserializeObject<Dictionary<string, string>>(TheEntitySubtitlesText.text),
            TheEntityClips).ToList());
        
        CasterSubtitles.Conversations = GetConversations(GetDialogues(
            JsonConvert.DeserializeObject<Dictionary<string, string>>(CasterSubtitlesText.text),
            JohnCasterClips)
            .ToList());
    }


    private Dialogue[] GetDialogues(Dictionary<string, string> audioAndSubtitles, AudioClip[] Audios) 
    {
        return Audios.Select(x => new Dialogue(audioAndSubtitles[x.name + ".wav"], x)).ToArray();
    }

    private List<Conversation> GetConversations(List<Dialogue> dialogues) 
    {
        List<Conversation> conversations = new List<Conversation>();
        List<Dialogue> conversationDialogue = new List<Dialogue>();
        Dialogue[] orderedDialogues = dialogues.OrderBy(x => x.ConversationNo).ToArray();

        for (int i = 0; i < orderedDialogues.Length; i++) 
        {
            Dialogue d = orderedDialogues[i];

            if (!conversationDialogue.Any())
            {
                conversationDialogue.Add(d);
                if (i == orderedDialogues.Length - 1) conversations.Add(new Conversation(conversationDialogue.ToArray()));
            }
            else if (conversationDialogue[0].ConversationNo != d.ConversationNo)
            {
                conversations.Add(new Conversation(conversationDialogue.ToArray()));
                conversationDialogue.Clear();
                conversationDialogue.Add(d);
            }
            else 
            {
                conversationDialogue.Add(d);
                if (i == orderedDialogues.Length - 1) conversations.Add(new Conversation(conversationDialogue.ToArray()));
            }

        }

        return conversations;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
