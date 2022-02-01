﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVManController : MonoBehaviour
{
    public bool BehaviourEnabled = true;
    public bool IsHunting = true;
    public float movementSpeed = 1f;
    public float minimumDistance = 1f;

    public float sightRange = 15f;
    public float sightConeAngle = 30f;

    public float alertTimeWithoutPerception = 5f;



    [SerializeField]
    [ReadOnlyField]
    private float lastPerceivedTimer;
    [SerializeField]
    [ReadOnlyField]
    private float interestTimer;
    [SerializeField]
    [ReadOnlyField]
    private Vector3 lastPercievedLocation;

    public TVManBehaviour currentBehaviour;
    public Transform TvManEyeLevel;


    public bool enableLegacyBehaviour;

    public float xTeleportDistanceFromPlayer = 30;
    public bool teleportAwayWhenAtMinimumDistance;

    private bool playerFacingPositive;

    private AudioSource audioSource;
    private int lineOfSightMask;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lineOfSightMask = LayerMask.NameToLayer("RenderTexture");
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }


    private void Start()
    {
        if(teleportAwayWhenAtMinimumDistance && enableLegacyBehaviour) TeleportXAwayFromPosition(xTeleportDistanceFromPlayer);

        //Start listening
        AudioManager.OnEntityNoiseAlert += OnNoiseMade;
    }

    private void Update()
    {
        if (audioSource.isPlaying != IsHunting) 
        {
            if (IsHunting)
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
                if (IsHunting) IsHunting = false;
                Behaviour_Perceive();
                break;

            case TVManBehaviour.PursuingPlayer:
                if (!IsHunting) IsHunting = true;
                Behaviour_PursuePlayer();
                break;

            case TVManBehaviour.Returning:
                if (IsHunting) IsHunting = false;
                Behaviour_Returning();
                break;

            case TVManBehaviour.Waiting:
                if (IsHunting) IsHunting = false;
                Behaviour_Waiting();
                break;

            case TVManBehaviour.PursuingLastPercived:
                if (IsHunting) IsHunting = false;
                Behaviour_PursueLastPercieved();
                break;
        }

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
        if (Vector2.Distance(new Vector3(transform.position.x, transform.position.z), new Vector3(noisePosition.x, noisePosition.z)) < noiseRadius) TurnToNoise(noisePosition);
    }

    private void TurnToNoise(Vector3 noisePosition) 
    {
        lastPercievedLocation = noisePosition;
        interestTimer = 0;

        switch (currentBehaviour) 
        {
            //case TVManBehaviour.PursuingPlayer:
            //case TVManBehaviour.PursuingMouse:
            //    break;

            case TVManBehaviour.None:
            case TVManBehaviour.Returning:
                transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
                lastPerceivedTimer = 0f;
                currentBehaviour = TVManBehaviour.Waiting;
                break;

            case TVManBehaviour.Waiting:
                currentBehaviour = TVManBehaviour.PursuingLastPercived;
                lastPerceivedTimer = 0f;
                break;
        }
        //if (currentBehaviour != TVManBehaviour.PursuingPlayer && currentBehaviour != TVManBehaviour.PursuingMouse)
        //{
        //    transform.LookAt(new Vector3(noisePosition.x, transform.position.y, noisePosition.z));
        //    lastPercivedTarget = 0f;
        //    currentBehaviour = TVManBehaviour.Waiting;
        //}


    }

    private void Behaviour_PursueLastPercieved()
    {
        if (!Behaviour_Perceive() && MoveTowardPosition(lastPercievedLocation, false))
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                currentBehaviour = TVManBehaviour.Returning;
            }
        }
        else 
        {
            lastPerceivedTimer = 0f;
            if (interestTimer < alertTimeWithoutPerception * 1.5)
            {
                interestTimer += Time.deltaTime;
            }
            else 
            {
                currentBehaviour = TVManBehaviour.Waiting;
            }

        }
    }

    private void Behaviour_Waiting() 
    {
        if (!Behaviour_Perceive())
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                currentBehaviour = TVManBehaviour.Returning;
                lastPerceivedTimer = 0f;
            }
        }
    }

    private bool Behaviour_Perceive()
    {
        //Check for player
        Vector3 playerPosition = GameManager.current.player.transform.position;
        Vector3 newLookLocation = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

        if (Vector3.Distance(newLookLocation, transform.position) < sightRange)
        {
            if (LineOfSightCheck(playerPosition) == PercivedEntity.Player || Vector3.Distance(newLookLocation, transform.position) <= minimumDistance) 
            {
                currentBehaviour = TVManBehaviour.PursuingPlayer;
                lastPerceivedTimer = 0f;
                return true;
            }
        }
        else if (false) //Check for other thing (Mouse when implemented)
        {
            
        }

        return false;
    }

    private void Behaviour_Returning() 
    {
        if (!Behaviour_Perceive() && MoveTowardPosition(initialPosition, false)) 
        {
            transform.rotation = initialRotation;
            currentBehaviour = TVManBehaviour.None;
        }
    }

    private PercivedEntity LineOfSightCheck(Vector3 target) 
    {
        return LineOfSightCheck(target, out RaycastHit? hit);
    }

    private PercivedEntity LineOfSightCheck(Vector3 target, out RaycastHit? hit) 
    {
        float angleToTarget = Vector3.Angle(transform.forward, new Vector3(target.x, transform.position.y, target.z) - transform.position);
        if (Mathf.Abs(angleToTarget) < sightConeAngle && Physics.Linecast(TvManEyeLevel.position, target, out RaycastHit hitResult, lineOfSightMask))
        {
            hit = hitResult;
            switch (hitResult.collider.gameObject.tag) 
            {
                case "Player":
                    return GameManager.current.playerController.IsIlluminated ? PercivedEntity.Player : PercivedEntity.None;
                case "Entity":
                    return PercivedEntity.Entity;
            }
        }


        hit = null;
        return PercivedEntity.None;
    }

    private void Behaviour_PursuePlayer() 
    {
        MoveTowardPosition(GameManager.current.player.transform.position);
        
        if (LineOfSightCheck(GameManager.current.player.transform.position) != PercivedEntity.Player)
        {
            if (lastPerceivedTimer < alertTimeWithoutPerception)
            {
                lastPerceivedTimer += Time.deltaTime;
            }
            else
            {
                currentBehaviour = TVManBehaviour.Waiting;
                lastPerceivedTimer = 0f;
            }
        }
        else 
        {
            lastPerceivedTimer = 0f;
        }

    }

    private bool MoveTowardPosition(Vector3 target, bool stopAtMinimumDistance = true) //Returns true once target is reached
    {
        target = new Vector3(target.x, transform.position.y, target.z); //Make sure target is on the same plane 
        float minimumDistance = stopAtMinimumDistance ? this.minimumDistance : 0.01f;

        transform.LookAt(target);

        if (Vector3.Distance(target, transform.position) > minimumDistance)
        {
            transform.position += transform.forward * Time.deltaTime * movementSpeed;
        }
        else 
        {
            if (!stopAtMinimumDistance) transform.position = target;
            return true;
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

            if (IsHunting && Vector3.Distance(newLookLocation, transform.position) > minimumDistance)
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
    PursuingMouse,
    PursuingLastPercived,
    Returning,
    Patrolling,
    AbsorbingPlayer,
    Waiting
}

public enum PercivedEntity 
{
    None,
    Player,
    Entity
}