using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 12f;
    public Camera playerCamera;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (x != 0 || z != 0) controller.Move(move * speed * Time.deltaTime);
        else controller.SimpleMove(Vector3.zero);

        if (Input.GetButtonDown("Cancel")) 
        {
            print("End game!");
            Application.Quit();
        }

        if (Input.GetButtonDown("Interact"))
        {
            print("interact yo");
        }

        if (playerCamera != null) 
        {
            RaycastHit lookedAtObject;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out lookedAtObject, 5f, LayerMask.GetMask("Interactables")))
            {
                Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * lookedAtObject.distance, Color.yellow);
                Debug.Log("Did Hit: " + lookedAtObject.collider.gameObject.name);
            }
            else
            {
                Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * 5, Color.white);
                Debug.Log("Did not Hit");
            }


        }


    }




}
