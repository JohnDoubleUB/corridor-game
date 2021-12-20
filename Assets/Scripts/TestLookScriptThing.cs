using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLookScriptThing : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(GameManager.current.trueCamera.transform.position);
    }
}
