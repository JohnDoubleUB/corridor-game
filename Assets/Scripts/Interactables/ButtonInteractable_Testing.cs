using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractable_Testing : InteractableObject
{
    public Transform ButtonMeshTransform;
    public float buttonInAmount = 0.009f;
    public float timeInMultiplier = 4;
    public bool makeButtonInert = false;

    public Text buttonText;

    public MeshRenderer buttonMesh;

    private bool buttonPushed;
    private float timer = 0;

    private Vector3 buttonOutPosition;
    private Vector3 buttonInPosition;

    public TestButtonEffectType testEffectType;

    private void Awake()
    {
        if (ButtonMeshTransform != null) 
        {
            buttonOutPosition = ButtonMeshTransform.localPosition;
            buttonInPosition = new Vector3(buttonOutPosition.x + buttonInAmount, buttonOutPosition.y, buttonOutPosition.z);
        }
    }

    protected override void OnInteract() 
    {
        if (!buttonPushed) 
        {
            buttonPushed = true;

            switch (testEffectType) 
            {
                case TestButtonEffectType.Spin:
                    GameParentTestScript.current.dospin = !GameParentTestScript.current.dospin;
                    break;

                case TestButtonEffectType.TVMan:
                    GameManager.current.tvMan.moveTowardPlayer = !GameManager.current.tvMan.moveTowardPlayer;
                    GameManager.current.tvMan.teleportAwayWhenAtMinimumDistance = !GameManager.current.tvMan.teleportAwayWhenAtMinimumDistance;
                    break;
            }
        }
    }

    private void Update()
    {
        if (!makeButtonInert)
        {
            if (buttonPushed && ButtonMeshTransform.position.x != buttonInPosition.x)
            {
                ButtonMeshTransform.localPosition = buttonInPosition;
            }
            else if (!buttonPushed && ButtonMeshTransform.position.x != buttonOutPosition.x)
            {
                ButtonMeshTransform.localPosition = buttonOutPosition;
            }

            if (buttonPushed)
            {
                if (timer < 1)
                {
                    timer += Time.deltaTime * timeInMultiplier;
                }
                else
                {
                    buttonPushed = false;
                    timer = 0;
                }
            }
        }


    }
}

public enum TestButtonEffectType 
{
    Spin,
    TVMan
}