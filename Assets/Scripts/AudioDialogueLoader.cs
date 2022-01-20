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

    public TextAsset CasterSubtitles;
    public TextAsset TheEntitySubtitles;

    public Dictionary<string, string> JohnCasterSubtitlesConverted;

    public Text testText;

    public DialogueWithSubtitles dialogueScriptableObject;

    // Start is called before the first frame update
    void Start()
    {
        print(CasterSubtitles.text);
        //JohnCasterSubtitlesConverted = JsonUtility.FromJson<string[][]>(TheEntitySubtitles.text);
        //CasterSubtitles.
        JohnCasterSubtitlesConverted = JsonConvert.DeserializeObject<Dictionary<string, string>>(CasterSubtitles.text);
        
        print(JohnCasterSubtitlesConverted.Count);

        foreach (KeyValuePair<string, string> s in JohnCasterSubtitlesConverted)
        {
            print(Path.GetFileNameWithoutExtension(s.Key));
        }

        if (testText != null) testText.text = JohnCasterSubtitlesConverted[JohnCasterClips[0].name + ".wav"];

        //dialogueScriptableObject.Dialogue.Add(new Dialogue(JohnCasterClips[0], "this isn't actually the subtitle because I'm testing and I'm lazy!"));

        dialogueScriptableObject.Dialogue = GetDialogues(JohnCasterSubtitlesConverted, JohnCasterClips).ToList();
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
