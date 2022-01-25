using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableCandle : InteractableObject
{
    public ParticleSystem CandleParticle;
    public Light CandleLight;
    public MeshRenderer CandleMesh;
    public bool IsIlluminatingPlayer
    {
        get { return lineOfSightToPlayer; }
    }

    private Material meshMat;
    private bool toggleLight = true;
    private bool candleInitial = true;
    private int lineOfSightMask;
    
    private bool inRangeOfPlayer;
    public bool lineOfSightToPlayer;


    private void Awake()
    {
        lineOfSightMask = LayerMask.NameToLayer("RenderTexture");
    }

    private void Start()
    {
        meshMat = CandleMesh.materials[0];
        MaterialManager.current.TrackMaterials(meshMat);
    }

    protected override void OnInteract()
    {
        toggleLight = !toggleLight;
        CandleLight.enabled = toggleLight;
        
        if (toggleLight)
        {
            CandleParticle.Play(true);
            meshMat.SetFloat("BaseMap_EmissionAmount", 1);
        }
        else 
        {
            CandleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            meshMat.SetFloat("BaseMap_EmissionAmount", 0);
        }
    }

    private void OnDestroy()
    {
        MaterialManager.current.UntrackMaterials(meshMat);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            print("player is here");
            inRangeOfPlayer = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (candleInitial && other.gameObject.tag == "Player")
        {
            print("player is here");
            
            inRangeOfPlayer = true;
            candleInitial = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            print("player is no longer here");

            inRangeOfPlayer = false;
        }
    }

    private void Update()
    {
        //lineOfSightToPlayer = inRangeOfPlayer &&
        //    Physics.Linecast(CandleParticle.transform.position, GameManager.current.playerController.transform.position, out RaycastHit hitResult, lineOfSightMask) &&
        //    hitResult.collider.gameObject.tag == "Player";

        if (toggleLight && 
            inRangeOfPlayer &&
            Physics.Linecast(CandleParticle.transform.position, GameManager.current.playerController.transform.position, out RaycastHit hitResult, lineOfSightMask) &&
            hitResult.collider.gameObject.tag == "Player")
        {
            lineOfSightToPlayer = true;
            Debug.DrawLine(CandleParticle.transform.position, hitResult.point, Color.green);
        }
        else 
        {
            lineOfSightToPlayer = false;
        }
    }

}
