using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorSection : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Transform corridorTransform;
    public Light[] lights;
    public float segmentLength = 10f;
    public Transform FakeParent;
    public GameObject corridorProps;
    
    public Transform[] CorridorStartEnd;
    public CorridorMoverScript toNotifyOnPlayerEnter;
    public Door DoorPrefab;
    public bool HasWarped;

    public int CorridorNumber;

    public SectionType sectionType;

    private Material[] meshMaterials;

    public bool FlipSection;
    private bool currentSectionFlip;

    public bool FlipCorridorX;
    private bool currentFlipX;

    public bool FlipCorridorZ;
    private bool currentFlipZ;

    private float defaultCorX;
    private float defaultCorZ;

    private float defaultVariationAmplitude;

    private void Awake()
    {
        defaultCorX = corridorTransform.localScale.x;
        defaultCorZ = corridorTransform.localScale.z;
        meshMaterials = meshRenderer.materials;
        currentFlipX = FlipCorridorX;
        currentFlipZ = FlipCorridorZ;
        currentSectionFlip = FlipSection;
        defaultVariationAmplitude = meshMaterials[0].GetFloat("_VariationAmplitude");
    }

    private void Start()
    {
        if(MaterialManager.current != null) MaterialManager.current.TrackMaterials(meshMaterials);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && toNotifyOnPlayerEnter != null) 
        {
            toNotifyOnPlayerEnter.OnPlayerEnter(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            print("Player exited");
        }
    }

    private void Update()
    {
        CorridorFlipCheck();
        SectionFlipCheck();
        foreach (Material meshMat in meshMaterials) 
        { 
            meshMat.SetFloat("_UVStretch", transform.localScale.x);
            meshMat.SetFloat("_VariationAmplitude", defaultVariationAmplitude / transform.localScale.x);
        }

        if (FakeParent != null && transform.position != FakeParent.position) transform.position = FakeParent.position;
    }

    private void CorridorFlipCheck() 
    {
        if (FlipCorridorX != currentFlipX || FlipCorridorZ != currentFlipZ)
        {
            corridorTransform.localScale = new Vector3(FlipCorridorX ? -defaultCorX : defaultCorX, corridorTransform.localScale.y, FlipCorridorZ ? -defaultCorZ : defaultCorZ);
            currentFlipX = FlipCorridorX;
            currentFlipZ = FlipCorridorZ;
        }
    }

    private void SectionFlipCheck() 
    {
        if (FlipSection != currentSectionFlip) 
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            currentSectionFlip = FlipSection;
        }
    }

    public void TogglePropFlip() 
    {
        Vector3 currentScale = corridorProps.transform.localScale;
        corridorProps.transform.localScale = new Vector3(currentScale.x > 0 ? -1 : 1, currentScale.y, currentScale.z);
    }

    public void SetPropSectionFlip(bool flip)
    {
        Vector3 currentScale = corridorProps.transform.localScale;
        corridorProps.transform.localScale = new Vector3(flip ? -1 : 1, currentScale.y, currentScale.z);
    }

    public void SetCorridorStretch(float scaleX) 
    {
        transform.localScale = new Vector3(transform.localScale.x > 0 ? scaleX : -scaleX, transform.localScale.y, transform.localScale.z);
    }

    public void SetAllWavyness(float value) 
    {
        foreach (Material meshMat in meshMaterials) meshMat.SetFloat("_DriftSpeed", value);
    }

    public void SetFloorWavyness(float value) 
    {
        meshMaterials[1].SetFloat("_DriftSpeed", value);
    }

    public void SetWallWavyness(float value) 
    {
        meshMaterials[0].SetFloat("_DriftSpeed", value);
    }
}

public enum SectionType 
{
    Middle,
    Front,
    Back
}