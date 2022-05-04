using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RadioInteractable : InteractableObject
{
    public Animator radioAnimatorFront;
    public Animator radioAnimatorBack;
    
    public AudioClip RadioOnOffSound;
    public AudioClip RadioDroneSound;
    
    public AudioSource RadioSpeaker;
    public float RadioSpeakerDefaultVolume = 0.5f;

    public GameObject DialogueAudioSourceObject;

    public bool radioOn;

    public DialogueWithSubtitles DialogueToPlay;
    public AudioClip AudioToPlay;
    public int ConversationNumber = 0;
    public int DialoguePartNo = 0;

    private AudioSource[] DialogueAudioSources;
    private Conversation conversationToPlay;
    private bool reachedEndOfDialogue;

    public delegate void EndOfAudioAction();
    public event EndOfAudioAction OnEndOfDialogue;

    public delegate void NextDialoguePartAction(int partNo);
    public event NextDialoguePartAction OnNextDialoguePart;

    public bool playRadioDroneSound = true;
    public bool allowDialogueSkipWithInput;

    private void Awake()
    {
        RadioSpeaker.clip = RadioDroneSound;
        RadioSpeaker.volume = RadioSpeakerDefaultVolume;

        if (DialogueAudioSourceObject != null) DialogueAudioSources = DialogueAudioSourceObject.GetComponents<AudioSource>();


        DialoguePartNo = 0;

        if(AudioToPlay != null) DialogueAudioSources[0].clip = AudioToPlay;

        if (DialogueToPlay) 
        {
            conversationToPlay = DialogueToPlay.Conversations.FirstOrDefault(x => x.ConversationNo == ConversationNumber);
        } 

    }
    protected override void OnInteract()
    {
        radioOn = !radioOn;

        if (radioAnimatorFront != null) 
        {
            radioAnimatorFront.Play(radioOn ? "On" : "Off");
        }

        if (radioAnimatorBack != null) 
        {
            radioAnimatorBack.Play(radioOn ? "On" : "Off");
        }

        if(RadioOnOffSound != null) transform.PlayClipAtTransform(RadioOnOffSound);
        //AudioManager.current.PlayClipAt(RadioOnOffSound, transform.position, 1f, true);

        if (radioOn)
        {
            if (AudioToPlay != null)
            {
                DialogueAudioSources[0].Play();
            }
            else
            {
                if (!PlayDialogue() && playRadioDroneSound) RadioSpeaker.Play();
            }
        }
        else 
        {
            if (AudioToPlay != null)
            {
                DialogueAudioSources[0].Pause();
            }
            else
            {
                if (playRadioDroneSound) RadioSpeaker.Stop();
                PauseDialogue();
            }
        }
    }

    private void Update()
    {
        if (radioOn) 
        {
            
            if ((allowDialogueSkipWithInput && GameManager.current.playerController.cutsceneMode && Input.anyKeyDown) || !reachedEndOfDialogue && conversationToPlay != null && !DialogueAudioSources[0].isPlaying) 
            {
                PlayNextDialoguePart();
            }
        }
    }

    private bool PlayDialogue() 
    {
        if (!reachedEndOfDialogue && conversationToPlay != null)
        {
            DialoguePart currentPart = conversationToPlay.DialogueParts[DialoguePartNo];
            ClearPlayerSubtitles();

            if (DialogueAudioSources[0].clip == null)
            {
                //Play clip from position
                //DialoguePart currentPart = conversationToPlay.DialogueParts[DialoguePartNo];

                for (int i = 0; i < DialogueAudioSources.Length && i < currentPart.Dialogues.Count; i++) 
                {
                    DialogueAudioSources[i].clip = currentPart.Dialogues[i].DialogueAudio;
                    DialogueAudioSources[i].Play();
                    GameManager.current.playerController.UIHandler.DialogueBoxes[i].text = currentPart.Dialogues[i].Subtitles;
                    GameManager.current.playerController.UIHandler.DialogueBoxes[i].color = DialogueToPlay.SubtitleColours[currentPart.Dialogues[i].SubtitleColour];
                }
            }
            else
            {
                foreach (AudioSource aS in DialogueAudioSources) aS.Play();

                for (int i = 0; i < DialogueAudioSources.Length && i < currentPart.Dialogues.Count; i++)
                {
                    GameManager.current.playerController.UIHandler.DialogueBoxes[i].text = currentPart.Dialogues[i].Subtitles;
                    GameManager.current.playerController.UIHandler.DialogueBoxes[i].color = DialogueToPlay.SubtitleColours[currentPart.Dialogues[i].SubtitleColour];
                }

            }

            return true;
        }

        return false;
    }

    private void PauseDialogue() 
    {
        foreach (AudioSource aS in DialogueAudioSources) aS.Pause();
        ClearPlayerSubtitles();
    }

    private void PlayNextDialoguePart() 
    {
        if (DialoguePartNo < conversationToPlay.DialogueParts.Count - 1)
        {
            DialoguePartNo++;

            DialoguePart currentPart = conversationToPlay.DialogueParts[DialoguePartNo];

            ClearPlayerSubtitles();

            //Trigger change dialogue event


            OnNextDialoguePart?.Invoke(DialoguePartNo);

            for (int i = 0; i < DialogueAudioSources.Length && i < currentPart.Dialogues.Count; i++)
            {
                DialogueAudioSources[i].clip = currentPart.Dialogues[i].DialogueAudio;
                DialogueAudioSources[i].Play();

                GameManager.current.playerController.UIHandler.DialogueBoxes[i].text = currentPart.Dialogues[i].Subtitles;
                GameManager.current.playerController.UIHandler.DialogueBoxes[i].color = DialogueToPlay.SubtitleColours[currentPart.Dialogues[i].SubtitleColour];
            }
        }
        else 
        {
            //We reached the end of dialogue
            reachedEndOfDialogue = true;

            ClearPlayerSubtitles();

            foreach (AudioSource aS in DialogueAudioSources)
            { 
                aS.Stop();
                aS.clip = null;
            }

            OnEndOfDialogue?.Invoke();

            if(playRadioDroneSound) RadioSpeaker.Play();
        }
    }

    private void ClearPlayerSubtitles() 
    {
        foreach (Text tb in GameManager.current.playerController.UIHandler.DialogueBoxes) tb.text = "";
    }

    private void OnDestroy()
    {
        ClearPlayerSubtitles();
    }
}
