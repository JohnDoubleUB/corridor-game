using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestApplicationStuff : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        print("Target Framerate: " + Application.targetFrameRate);
        print("Vsync Count: " + QualitySettings.vSyncCount);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;


        print("Target Framerate: " + Application.targetFrameRate);
        print("Vsync Count: " + QualitySettings.vSyncCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
