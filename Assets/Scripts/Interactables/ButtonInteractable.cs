﻿using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractable : InteractableObject
{
    public Transform ButtonMeshTransform;
    public float buttonInAmount = 0.009f;
    public float timeInMultiplier = 4;
    public bool makeButtonInert = false;

    public Text buttonText;

    public ButtonRecievingPuzzleController buttonRecievingPuzzleController;

    public MeshRenderer buttonMesh;

    private bool buttonPushed;
    private float timer = 0;

    private Vector3 buttonOutPosition;
    private Vector3 buttonInPosition;

    private void Awake()
    {
        if (ButtonMeshTransform != null) 
        {
            buttonOutPosition = ButtonMeshTransform.localPosition;
            buttonInPosition = new Vector3(buttonOutPosition.x + buttonInAmount, buttonOutPosition.y, buttonOutPosition.z);
        }

        if(buttonText != null) 
        {
            buttonText.text = ObjectName;
        }
    }

    protected override void OnInteract() 
    {
        if (!buttonPushed) 
        {
            buttonPushed = true;
            if (buttonRecievingPuzzleController != null) buttonRecievingPuzzleController.InputCharacter(ObjectName.ToCharArray()[0], transform);
            //if (buttonReciever != null) buttonReciever.InputCharacter(ObjectName.ToCharArray()[0], transform);
            //timer = 0;
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
