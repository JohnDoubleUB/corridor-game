using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractable : InteractableObject
{
    public Transform ButtonMeshTransform;
    public float buttonInAmount = 0.009f;
    public float timeInMultiplier = 4;

    public Text buttonText;

    public NumberpadController numberPadController;

    private bool buttonPushed;
    private float timer = 0;

    private Vector3 buttonOutPosition;
    private Vector3 buttonInPosition;

    private void Awake()
    {
        if (ButtonMeshTransform != null) 
        {
            buttonOutPosition = ButtonMeshTransform.position;
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
            if (numberPadController != null) numberPadController.InputCharacter(ObjectName.ToCharArray()[0]);
            //timer = 0;
        }
    }

    private void Update()
    {
        if (buttonPushed && ButtonMeshTransform.position.x != buttonInPosition.x)
        {
            ButtonMeshTransform.position = buttonInPosition;
        }
        else if (!buttonPushed && ButtonMeshTransform.position.x != buttonOutPosition.x) 
        {
            ButtonMeshTransform.position = buttonOutPosition;
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
