using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDialogueLoader : MonoBehaviour
{
    public AudioClip[] JohnCasterClips;
    public AudioClip[] TheEntityClips;

    public TextAsset CasterSubtitles;
    public TextAsset TheEntitySubtitles;

    public string[][] JohnCasterSubtitlesConverted;

    // Start is called before the first frame update
    void Start()
    {
        print(CasterSubtitles.text);
        JohnCasterSubtitlesConverted = JsonUtility.FromJson<string[][]>(TheEntitySubtitles.text);
        //CasterSubtitles.

        print(JohnCasterSubtitlesConverted.Length);

        foreach (string[] s in JohnCasterSubtitlesConverted) 
        {
            print(s[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
