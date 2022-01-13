using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParentTestScript : MonoBehaviour
{
    public static GameParentTestScript current;
    
    public bool dospin;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }
    void LateUpdate()
    {
        if(dospin) transform.Rotate(50 * Time.deltaTime, 0, 0); //rotates 50 degrees per second around z axis
    }


}
