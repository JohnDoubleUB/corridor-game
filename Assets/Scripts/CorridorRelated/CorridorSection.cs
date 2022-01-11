using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CorridorSection : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshCollider meshCollider;

    public CorridorMeshVarient MeshCorridorVarient;

    public Transform corridorTransform;
    public Light[] lights;
    public float segmentLength = 10f;
    public Transform FakeParent;
    public GameObject corridorProps;
    
    public Transform[] CorridorStartEnd;
    public CorridorChangeManager toNotifyOnPlayerEnter;
    public CorridorLayoutHandler CurrentLayout;
    public Door DoorPrefab;
    public bool HasWarped;
    
    public bool WillStretch;
    
    public bool WillWave;

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

    private BoxCollider boxCol;

    private bool wavyInProgress;
    private bool stretchInProgress;

    private void Awake()
    {
        defaultCorX = corridorTransform.localScale.x;
        defaultCorZ = corridorTransform.localScale.z;
        meshMaterials = meshRenderer.materials;
        currentFlipX = FlipCorridorX;
        currentFlipZ = FlipCorridorZ;
        currentSectionFlip = FlipSection;
        defaultVariationAmplitude = meshMaterials[0].GetFloat("_VariationAmplitude");
        boxCol = GetComponent<BoxCollider>();
        MeshCorridorVarient = new CorridorMeshVarient(meshFilter, meshCollider);
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
            //print("Player exited");
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
        foreach (Material meshMat in meshMaterials) 
        { 
            meshMat.SetFloat("_DriftSpeed", value);
        }
    }

    public void SetFloorWavyness(float value) 
    {
        Material meshMat = meshMaterials[1];
        meshMat.SetFloat("_DriftSpeed", value);
    }

    public void SetWallWavyness(float value) 
    {
        Material meshMat = meshMaterials[0];
        meshMat.SetFloat("_DriftSpeed", value);
    }

    public void StretchTo(float stretchTarget)
    {
        TransitionToStretchTarget(stretchTarget); 
    }

    public void MakeWave(bool effectWall = true, bool effectFloor = true) 
    {
        TransitionToWavy(effectWall, effectFloor);
    }

    private async void TransitionToStretchTarget(float stretchTarget) 
    {
        float stretchTimer = 0;
        float initialStretch = Math.Abs(transform.localScale.x);
        float stretchSpeed = 0.5f;
        
        stretchInProgress = true;

        while (stretchTimer < 1f && stretchInProgress)
        {
            stretchTimer += Time.deltaTime * stretchSpeed;
            float currentStretch = Mathf.SmoothStep(initialStretch, stretchTarget, stretchTimer);
            SetCorridorStretch(currentStretch);
            await Task.Yield();
        }

        if (!wavyInProgress) 
        {
            SetCorridorStretch(initialStretch);
        }
        else 
        {
            SetCorridorStretch(stretchTarget);
            stretchInProgress = false;
        }
    }

    private async void TransitionToWavy(bool wall, bool floor) 
    {
        float wavyTimer = 0;
        float currentWavy = 0;
        float wavySpeed = 0.5f;

        wavyInProgress = true;
        
        while (currentWavy < 1f && wavyInProgress)
        {
            wavyTimer += Time.deltaTime * wavySpeed;
            currentWavy = Mathf.SmoothStep(0, 1, wavyTimer);

            SetWallAndOrFloorWavyness(wall, floor, currentWavy);

            await Task.Yield();
        }

        if (!wavyInProgress)
        {
            SetWallAndOrFloorWavyness(wall, floor, 0);
        }
        else
        {
            SetWallAndOrFloorWavyness(wall, floor, 1f);
            wavyInProgress = false;
        }

    }

    public void StopAllEffects()
    {
        wavyInProgress = false;
        stretchInProgress = false;
    }

    public void SetMaterialVarient(CorridorMatVarient materialVarient) 
    {
        if (meshMaterials[0].GetTexture("Albedo1") != materialVarient.albedo1) meshMaterials[0].SetTexture("Albedo1", materialVarient.albedo1);
        if (meshMaterials[0].GetTexture("Albedo2") != materialVarient.albedo2) meshMaterials[0].SetTexture("Albedo2", materialVarient.albedo2);
        
        if (meshMaterials[1].GetTexture("Albedo1") != materialVarient.albedo1) meshMaterials[1].SetTexture("Albedo1", materialVarient.albedo1);
        if (meshMaterials[1].GetTexture("Albedo2") != materialVarient.albedo2) meshMaterials[1].SetTexture("Albedo2", materialVarient.albedo2);
    }

    private void SetWallAndOrFloorWavyness(bool wall, bool floor, float wavyAmount) 
    {
        if (wall && floor) SetAllWavyness(wavyAmount);
        else if (wall) SetWallWavyness(wavyAmount);
        else if (floor) SetFloorWavyness(wavyAmount);
    }
}

public enum SectionType 
{
    Middle,
    Front,
    Back
}

public class CorridorMeshVarient 
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public MeshFilter MeshFilter { get { return meshFilter; } }
    public MeshCollider MeshCollider { get { return meshCollider; } }
    public Mesh CurrentMesh { get { return meshFilter.mesh; } }
    
    public CorridorMeshVarient(MeshFilter meshFilter, MeshCollider meshCollider) 
    {
        this.meshFilter = meshFilter;
        this.meshCollider = meshCollider;
    }

    public void ChangeMesh(Mesh mesh) 
    {
        if (meshFilter.mesh != mesh) meshFilter.mesh = mesh;
        if (meshCollider.sharedMesh != mesh) meshCollider.sharedMesh = mesh;
    }


}