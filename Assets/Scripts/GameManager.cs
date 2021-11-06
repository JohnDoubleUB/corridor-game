using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager current;
    public GameObject player;
    public CG_CharacterController playerController;
    public Camera trueCamera;
    public TVManController tvMan;

    public float maximumTVManEffectDistance = 10f;
    public bool tvManEffectEnabled = true;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    private void Update()
    {
        TVManEffectUpdate();
    }

    private void TVManEffectUpdate() 
    {
        if (tvMan != null && player != null && MaterialManager.current != null) 
        {
            float distanceFromPlayer = Vector3.Distance(tvMan.transform.position, player.transform.position);

            if (distanceFromPlayer <= maximumTVManEffectDistance && tvManEffectEnabled)
            {
                float remappedValue = distanceFromPlayer.Remap(maximumTVManEffectDistance, tvMan.minumumDistance + 0.5f, 0f, 1f);
                MaterialManager.current.alternateBlend = remappedValue;
            }
            else if (MaterialManager.current.alternateBlend != 0) 
            {
                MaterialManager.current.alternateBlend = 0;
            }
            //MaterialManager.current.alternateBlend = Mathf.Lerp(0f, 1f, )


        }
    }
}
