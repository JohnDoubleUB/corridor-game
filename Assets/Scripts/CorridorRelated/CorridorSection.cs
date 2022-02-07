using System;
using System.Threading.Tasks;
using UnityEngine;

public class CorridorSection : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshCollider meshCollider;

    public Transform corridorTransform;
    public Light[] lights;
    public float segmentLength = 10f;
    public Transform FakeParent;
    public GameObject corridorProps;

    public Transform[] CorridorStartEnd;
    public CorridorChangeManager toNotifyOnPlayerEnter;

    public AudioClip corridorStretchSound;
    public float corridorStretchVolume = 1f;

    public Transform[] MouseSpawnLocations;
    public Transform[] TVManPatrolLocations;
    public CorridorLayoutHandler CurrentLayout
    {
        get
        {
            return currentLayout;
        }
        set
        {
            currentLayout = value;
        }

    }

    [SerializeField]
    private CorridorLayoutHandler currentLayout;



    public Door DoorPrefab;
    public bool HasWarped
    {
        get
        {
            return currentLayout != null ? currentLayout.LayoutData.HasWarped : false;
        }
        set
        {
            if (value && currentLayout != null && currentLayout.LayoutData != null) currentLayout.LayoutData.HasWarped = value;
            hasWarped = value;
        }
    }

    //This is kind of just so we can see stuff in the inspector, it doesn't do anything and should probably be removed.
    [SerializeField]
    private bool hasWarped;

    public bool WillStretch
    {
        get
        {
            return currentLayout != null ? currentLayout.ForceStretch : false;
        }
    }

    public bool WillWave
    {
        get
        {
            return currentLayout != null ? currentLayout.ForceWave : false;
        }
    }

    public float StretchAmount
    {
        get
        {
            return currentLayout != null ? currentLayout.StretchAmount : 2f;
        }
    }

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

    public float defaultVariationAmplitude;
    private float currentVariationAmplitude;

    public NavMeshSourceTag[] navMeshes;

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
        currentVariationAmplitude = 0f;
        boxCol = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        if (MaterialManager.current != null) MaterialManager.current.TrackMaterials(meshMaterials);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && toNotifyOnPlayerEnter != null)
        {
            toNotifyOnPlayerEnter.OnPlayerEnter(this);
        }
    }

    public void OnSectionEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && toNotifyOnPlayerEnter != null)
        {
            toNotifyOnPlayerEnter.OnPlayerEnter(this);
        }
    }

    private void Update()
    {
        CorridorFlipCheck();
        SectionFlipCheck();
        foreach (Material meshMat in meshMaterials)
        {
            meshMat.SetFloat("_UVStretch", transform.localScale.x);
            meshMat.SetFloat("_VariationAmplitude", currentVariationAmplitude / transform.localScale.x);
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
            currentVariationAmplitude = value != 0f ? defaultVariationAmplitude : 0f;
        }
    }

    public void SetFloorWavyness(float value)
    {
        Material meshMat = meshMaterials[1];
        meshMat.SetFloat("_DriftSpeed", value);
        currentVariationAmplitude = value != 0f ? defaultVariationAmplitude : 0f;
    }

    public void SetWallWavyness(float value)
    {
        Material meshMat = meshMaterials[0];
        meshMat.SetFloat("_DriftSpeed", value);
        currentVariationAmplitude = value != 0f ? defaultVariationAmplitude : 0f;
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
        if (corridorStretchSound != null) GameManager.current.player.transform.PlayClipAtTransform(corridorStretchSound, true, corridorStretchVolume, true, 0, false);


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

        if (!stretchInProgress)
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

        if (floor) meshMaterials[1].SetFloat("_VariationAmplitude", 0.01f);
        if (wall) meshMaterials[0].SetFloat("_VariationAmplitude", 0.01f);

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

    public void ChangeMesh(Mesh mesh, int meshIndex = 0)
    {
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        navMeshes[1].enabled = meshIndex != 0;
    }

    public void SetMaterialVarient(CorridorMatVarient materialVarient)
    {
        if (meshMaterials[0].GetTexture("_MainTex") != materialVarient.albedo1) meshMaterials[0].SetTexture("_MainTex", materialVarient.albedo1);
        if (meshMaterials[0].GetTexture("_MainTex2") != materialVarient.albedo2) meshMaterials[0].SetTexture("_MainTex2", materialVarient.albedo2);

        if (meshMaterials[1].GetTexture("_MainTex") != materialVarient.albedo1) meshMaterials[1].SetTexture("_MainTex", materialVarient.albedo1);
        if (meshMaterials[1].GetTexture("_MainTex2") != materialVarient.albedo2) meshMaterials[1].SetTexture("_MainTex2", materialVarient.albedo2);
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