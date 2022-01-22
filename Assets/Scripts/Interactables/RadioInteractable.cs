using System.Collections;
using System.Collections.Generic;
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
    public int ConversationNumber = 0;
    public int DialoguePartNo = 0;

    private AudioSource[] DialogueAudioSources;
    private Conversation conversationToPlay;
    private bool reachedEndOfDialogue;

    private void Awake()
    {
        RadioSpeaker.clip = RadioDroneSound;
        RadioSpeaker.volume = RadioSpeakerDefaultVolume;

        if (DialogueAudioSourceObject != null) DialogueAudioSources = DialogueAudioSourceObject.GetComponents<AudioSource>();


        DialoguePartNo = 0;
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

        AudioManager.current.PlayClipAt(RadioOnOffSound, transform.position, 1f, true);

        if (radioOn)
        {
            RadioSpeaker.Play();
            PlayDialogue();
        }
        else 
        {
            RadioSpeaker.Stop();
            PauseDialogue();
        }
    }

    private void Update()
    {
        if (radioOn) 
        {
            
            if (!reachedEndOfDialogue && conversationToPlay != null && !DialogueAudioSources[0].isPlaying) 
            {
                //DialoguePartNo++;
                PlayNextDialoguePart();
            }
        }
    }

    private void PlayDialogue() 
    {
        if (!reachedEndOfDialogue && conversationToPlay != null)
        {
            if (DialogueAudioSources[0].clip == null)
            {
                //Play clip from position
                DialoguePart currentPart = conversationToPlay.DialogueParts[DialoguePartNo];

                ClearPlayerSubtitles();

                for (int i = 0; i < DialogueAudioSources.Length && i < currentPart.Dialogues.Count; i++) 
                {
                    DialogueAudioSources[i].clip = currentPart.Dialogues[i].DialogueAudio;
                    DialogueAudioSources[i].Play();
                    GameManager.current.playerController.DialogueBoxes[i].text = currentPart.Dialogues[i].Subtitles;
                }
            }
            else
            {
                foreach (AudioSource aS in DialogueAudioSources)
                {
                    aS.Play();
                }
            }
        }
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

            for (int i = 0; i < DialogueAudioSources.Length && i < currentPart.Dialogues.Count; i++)
            {
                DialogueAudioSources[i].clip = currentPart.Dialogues[i].DialogueAudio;
                DialogueAudioSources[i].Play();

                GameManager.current.playerController.DialogueBoxes[i].text = currentPart.Dialogues[i].Subtitles;
            }
        }
        else 
        {
            reachedEndOfDialogue = true;

            ClearPlayerSubtitles();

            foreach (AudioSource aS in DialogueAudioSources)
            { 
                aS.Stop();
                aS.clip = null;
            }
        }
    }

    private void ClearPlayerSubtitles() 
    {
        foreach (Text tb in GameManager.current.playerController.DialogueBoxes) tb.text = "";
    }

    private void OnDestroy()
    {
        ClearPlayerSubtitles();
    }
}
