using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVManController : MonoBehaviour
{
    public bool BehaviourEnabled = true;
    public bool huntingPlayer = true;
    public float movementSpeed = 1f;
    public float minumumDistance = 1f;

    public TVManBehaviour currentBehaviour;
    public Transform TvManEyeLevel;


    public bool enableLegacyBehaviour;

    public float xTeleportDistanceFromPlayer = 30;
    public bool teleportAwayWhenAtMinimumDistance;

    private bool playerFacingPositive;

    private AudioSource audioSource;
    private int lineOfSightMask;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lineOfSightMask = LayerMask.NameToLayer("RenderTexture");
    }


    private void Start()
    {
        if(teleportAwayWhenAtMinimumDistance && enableLegacyBehaviour) TeleportXAwayFromPosition(xTeleportDistanceFromPlayer);

        //Start listening
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
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

        //if()



        playerFacingPositive = GameManager.current.player.transform.forward.x > 0;

        switch (currentBehaviour) 
        {
            case TVManBehaviour.None:
                if(Behaviour_PerceivePlayer()) currentBehaviour = TVManBehaviour.PursuingPlayer;
                if (huntingPlayer) huntingPlayer = false;
                break;
            case TVManBehaviour.PursuingPlayer:
                if (!huntingPlayer) huntingPlayer = true;

                Vector3 playerPosition = GameManager.current.player.transform.position;
                Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
                transform.LookAt(newLookLocation);
                if (huntingPlayer && Vector3.Distance(newLookLocation, transform.position) > minumumDistance)
                {
                    transform.position += transform.forward * Time.deltaTime * movementSpeed;
                }


                break;
        }
        

        //transform.LookAt(newLookLocation);



        if (enableLegacyBehaviour)
        {
            OldUpdateBehaviour();
        }
        else 
        {

        }
    }

    private void OnNoiseMade(Vector3 noisePosition, float noiseRadius) 
    {
        if (Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) OnNoiseHeard(noisePosition);
    }

    private void OnNoiseHeard(Vector3 noisePosition) 
    {
        if (currentBehaviour == TVManBehaviour.None)
        {
            transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
        }
    }

    private bool Behaviour_PerceivePlayer() //Returns true if the player has been percieved
    {

        Vector3 playerPosition = GameManager.current.player.transform.position;
        Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

        

        if (Vector3.Distance(newLookLocation, transform.position) < 15f)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, newLookLocation - transform.position);
            if (Mathf.Abs(angleToPlayer) < 30)
            {
                //Do a line trace to the player
                return Physics.Linecast(TvManEyeLevel.position, playerPosition, out RaycastHit hitResult, lineOfSightMask) && hitResult.collider.gameObject.tag == "Player";
            }
        }

        return false;
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
