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
        GameManager.current.playerController.CandleExitPlayerInRange(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.current.playerController.CandleEnterPlayerInRange(this);
            inRangeOfPlayer = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (candleInitial && other.gameObject.tag == "Player")
        {
            GameManager.current.playerController.CandleEnterPlayerInRange(this);
            
            inRangeOfPlayer = true;
            candleInitial = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.current.playerController.CandleExitPlayerInRange(this);

            inRangeOfPlayer = false;
        }
    }

    private void Update()
    {
        lineOfSightToPlayer = toggleLight && 
            inRangeOfPlayer &&
            Physics.Linecast(CandleParticle.transform.position, GameManager.current.playerController.transform.position, out RaycastHit hitResult, lineOfSightMask) &&
            hitResult.collider.gameObject.tag == "Player";
    }

}
