using UnityEngine;

public class InteractableCandle : InteractableObject
{
    public ParticleSystem CandleParticle;
    public Light CandleLight;
    public MeshRenderer CandleMesh;
    public AudioClip candleOnSound;
    public AudioClip candleOffSound;
    public float candleSoundVolume = 1f;
    public float candleSoundRadius = 0.2f;

    public bool IsIlluminatingPlayer
    {
        get { return lineOfSightToPlayer; }
    }

    private Material meshMat;
    private bool toggleLight = true;
    private bool candleInitial = true;
    private int lineOfSightMask;
    
    [SerializeField]
    [ReadOnlyField]
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
            if (candleOnSound != null) transform.PlayClipAtTransform(candleOnSound, true, candleSoundVolume, true, 0, true, candleSoundRadius);
            CandleParticle.Play(true);
            meshMat.SetFloat("BaseMap_EmissionAmount", 1);
        }
        else 
        {
            if (candleOffSound != null) transform.PlayClipAtTransform(candleOffSound, true, candleSoundVolume, true, 0, true, candleSoundRadius);
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

        RaycastHit hitResult = new RaycastHit();
        lineOfSightToPlayer = toggleLight && 
            inRangeOfPlayer &&
            Physics.Linecast(CandleParticle.transform.position, GameManager.current.playerController.transform.position, out hitResult, lineOfSightMask) &&
            hitResult.collider.tag == "Player";

        if (lineOfSightToPlayer) Debug.DrawLine(transform.position, hitResult.point, Color.red);
    }

}
