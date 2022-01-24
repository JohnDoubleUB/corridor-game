using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableCandle : InteractableObject
{
    public ParticleSystem CandleParticle;
    public Light CandleLight;
    public MeshRenderer CandleMesh;

    private Material meshMat;
    private bool toggleLight = true;

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
}
