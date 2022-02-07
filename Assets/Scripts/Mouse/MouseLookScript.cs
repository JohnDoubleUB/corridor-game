using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookScript : MonoBehaviour
{
    public MouseEntity mouseEntity;

    public Vector3 restingSightPosition;

    public float visionConeAngle = 30f;

    public Transform sightTransform;

    private bool IsMouseAbleToLook
    {
        get
        {
            switch (mouseEntity.CurrentBehaviour)
            {
                case MouseBehaviour.Idle:
                case MouseBehaviour.Look:
                case MouseBehaviour.Wander:
                    return true;
            }

            return false;
        }
    }

    private bool IsPlayerInFrontOfMouse { get { return mouseEntity.transform.GetAngleToTarget(GameManager.current.trueCamera.transform.position) < visionConeAngle; } }

    private bool IsPlayerInRangeOfMouse { 
        get 
        {
            Vector3 playerPositionOnPlane = new Vector3(GameManager.current.player.transform.transform.position.x, transform.position.y, GameManager.current.player.transform.transform.position.z);

            return Vector3.Distance(transform.position, playerPositionOnPlane) < 10f;
        } 
    }

    private void Awake()
    {
        restingSightPosition = sightTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offsetPosition = new Vector3(sightTransform.position.x, sightTransform.position.y, -sightTransform.position.z);

        transform.LookAt(offsetPosition);
        if (IsMouseAbleToLook && IsPlayerInRangeOfMouse && IsPlayerInFrontOfMouse)
        {
            Vector3 playerEyes = GameManager.current.trueCamera.transform.position;



        }
        else
        {

        }
    }


    //private bool isMouseAbleToLook()
    //{

    //}
}
