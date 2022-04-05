using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimatorFunctionCaller : MonoBehaviour
{
    public void CallRevertToSave() 
    {
        if (GameManager.current != null) GameManager.current.RestartCurrentScene();
    }
}
