using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        CasterSubtitles.Dialogue = GetDialogues(
            JsonConvert.DeserializeObject<Dictionary<string, string>>(CasterSubtitlesText.text),
            JohnCasterClips)
            .ToList();

        TheEntitySubtitles.Dialogue = GetDialogues(
            JsonConvert.DeserializeObject<Dictionary<string, string>>(TheEntitySubtitlesText.text),
            TheEntityClips)
            .ToList();
    }


    private Dialogue[] GetDialogues(Dictionary<string, string> audioAndSubtitles, AudioClip[] Audios) 
    {
        return Audios.Select(x => new Dialogue(audioAndSubtitles[x.name + ".wav"], x)).ToArray();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
