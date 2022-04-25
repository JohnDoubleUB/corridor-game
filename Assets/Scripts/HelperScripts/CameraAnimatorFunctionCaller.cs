using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimatorFunctionCaller : MonoBehaviour
{
    public SpriteRenderer playerSprite;

    private void Start()
    {
        if(playerSprite != null && !playerSprite.enabled) playerSprite.enabled = true;
    }
    public void TogglePlayerSprite() 
    {
        if (playerSprite != null) playerSprite.enabled = !playerSprite.enabled;
    }

    public void CallRevertToSave() 
    {
        if(playerSprite != null) playerSprite.enabled = true;
        if (GameManager.current != null) GameManager.current.RestartCurrentScene();
    }
}
