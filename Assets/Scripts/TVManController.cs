using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVManController : MonoBehaviour
{
    public bool moveTowardPlayer = true;
    public float movementSpeed = 1f;
    public float minumumDistance = 1f;
    private void Update()
    {
        if (GameManager.current != null && GameManager.current.player != null) 
        {
            Vector3 playerPosition = GameManager.current.player.transform.position;
            Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
            transform.LookAt(newLookLocation);

            if (moveTowardPlayer && Vector3.Distance(newLookLocation, transform.position) > minumumDistance) 
            {
                transform.position += transform.forward * Time.deltaTime * movementSpeed;
            }

        }
    }
}
