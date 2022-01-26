using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVManController : MonoBehaviour
{
    public bool huntingPlayer = true;
    public float movementSpeed = 1f;
    public float minumumDistance = 1f;

    public bool enableLegacyBehaviour;

    public float xTeleportDistanceFromPlayer = 30;
    public bool teleportAwayWhenAtMinimumDistance;

    private bool playerFacingPositive;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void Start()
    {
        if(teleportAwayWhenAtMinimumDistance && enableLegacyBehaviour) TeleportXAwayFromPosition(xTeleportDistanceFromPlayer);
    }

    private void Update()
    {
        if (audioSource.isPlaying != huntingPlayer) 
        {
            if (huntingPlayer)
            {
                audioSource.Play();
            }
            else 
            {
                audioSource.Stop();
            }
        }






        playerFacingPositive = GameManager.current.player.transform.forward.x > 0;





        Vector3 playerPosition = GameManager.current.player.transform.position;
        Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
        transform.LookAt(newLookLocation);









        if (enableLegacyBehaviour) OldUpdateBehaviour();
    }


    private void OldUpdateBehaviour() 
    {
        if (GameManager.current != null && GameManager.current.player != null)
        {
            //print("player forward vector: " + GameManager.current.player.transform.forward);
            bool playerFacingPositive = GameManager.current.player.transform.forward.x > 0;

            if (playerFacingPositive != this.playerFacingPositive && teleportAwayWhenAtMinimumDistance)
            {
                this.playerFacingPositive = playerFacingPositive;
                TeleportXAwayFromPosition(Vector3.Distance(GameManager.current.player.transform.position, transform.position));
            }



            Vector3 playerPosition = GameManager.current.player.transform.position;
            Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
            transform.LookAt(newLookLocation);

            if (huntingPlayer && Vector3.Distance(newLookLocation, transform.position) > minumumDistance)
            {
                transform.position += transform.forward * Time.deltaTime * movementSpeed;
            }
            else if (teleportAwayWhenAtMinimumDistance)
            {
                TeleportXAwayFromPosition(xTeleportDistanceFromPlayer);
            }

        }
    }

    private void TeleportXAwayFromPosition(float teleportDistance) 
    {
        transform.position = new Vector3(GameManager.current.player.transform.position.x + (playerFacingPositive ? teleportDistance : -teleportDistance), transform.position.y, GameManager.current.player.transform.position.z);
    }
}

public enum TVManBehaviour 
{
    None,
    PursuingPlayer,
    PursuingLastPercived,
    Returning,
    Patrolling,
    AbsorbingPlayer
} 
