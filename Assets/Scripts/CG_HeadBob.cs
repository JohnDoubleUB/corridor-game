using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_HeadBob : MonoBehaviour
{
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;
    public CG_CharacterController controller;
    public Transform additonalBobber;

    float defaultPosY = 0;
    float timer = 0;

    float additionalBobDefaultPosY = 0;

    // Start is called before the first frame update
    void Start()
    {
        defaultPosY = transform.localPosition.y;
        if (additonalBobber != null) additionalBobDefaultPosY = additonalBobber.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(controller.moveDirection.x) > 0.1f || Mathf.Abs(controller.moveDirection.z) > 0.1f)
        {
            //Player is moving
            timer += Time.deltaTime * walkingBobbingSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
            if(additonalBobber != null) additonalBobber.localPosition = new Vector3(additonalBobber.localPosition.x, additionalBobDefaultPosY + Mathf.Sin(timer) * bobbingAmount, additonalBobber.localPosition.z);
        }
        else
        {
            //Idle
            timer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);
            if (additonalBobber != null) additonalBobber.localPosition = new Vector3(additonalBobber.localPosition.x, Mathf.Lerp(additonalBobber.localPosition.y, additionalBobDefaultPosY, Time.deltaTime * walkingBobbingSpeed), additonalBobber.localPosition.z);
        }
    }
}
