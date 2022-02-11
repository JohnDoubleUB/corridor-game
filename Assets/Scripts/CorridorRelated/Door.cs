using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    public Animator doorAnimator;
    public DoorInteractable doorInteractable;
    private int relativePlayerDirection = -1;
    public Transform fakeParent;

    public AudioClip correctOpenSound;

    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip rattleSound;
    public AudioClip slamCloseSound;

    public bool openOnInteract;

    public NavMeshObstacle navMeshObstacle;

    private bool doorIsVisible = true;

    [SerializeField]
    private Collider[] doorCollisions;

    [SerializeField]
    private MeshRenderer[] doorMeshes;

    public bool IsOpen { get { return doorIsOpen; } }

    public bool DoorLocked
    {
        get { return doorLocked; }
        set
        {
            if (!value && doorLocked) justUnlocked = true;
            doorLocked = value;
        }
    }

    private bool doorLocked;
    private bool doorIsOpen;
    private Material[] meshMaterials;
    private bool openOnInteractLast;
    private bool justUnlocked;
    private bool doorIsClosing;

    private bool wavyInProgress;

    private float defaultVariationAmplitude;

    private void Awake()
    {
        meshMaterials = GetComponent<MeshRenderer>().materials;
        doorInteractable.door = this;

        openOnInteractLast = openOnInteract;
        doorInteractable.IsInteractable = openOnInteract;

        defaultVariationAmplitude = meshMaterials[1].GetFloat("_VariationAmplitude");
        meshMaterials[1].SetFloat("_VariationAmplitude", 0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (MaterialManager.current != null) MaterialManager.current.TrackMaterials(meshMaterials);
    }

    // Update is called once per frame
    void Update()
    {
        bool isPathClear = !(openOnInteract ? doorIsOpen : !doorLocked);
        if (fakeParent != null && fakeParent.position != transform.position) transform.position = fakeParent.position;
        if (navMeshObstacle.enabled != isPathClear) navMeshObstacle.enabled = isPathClear;

        if (openOnInteract != openOnInteractLast)
        {
            doorInteractable.IsInteractable = openOnInteract;
            openOnInteractLast = openOnInteract;
        }

        if (doorIsClosing && doorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
        {
            transform.PlayClipAtTransform(slamCloseSound, true, 0.4f, true, 0, false);
            doorIsClosing = false;
        }
    }

    private void _SetWavyness(float value)
    {
        Material meshMat = meshMaterials[1];
        meshMat.SetFloat("_DriftSpeed", value);
        SetMatWaveOnOrOff(value > 0, meshMat);
    }

    public void SetWavyness(float value)
    {
        wavyInProgress = false;
        _SetWavyness(value);
    }

    public void InteractOpenClose(bool ignoreLock = false)
    {
        if (doorIsVisible)
        {
            if (ignoreLock || !doorLocked)
            {
                if (!doorIsOpen)
                {
                    UpdateRelativePlayerDirection();
                    PlayDoorOpenAnimation();
                    if (justUnlocked)
                    {
                        transform.PlayClipAtTransform(correctOpenSound, true, 1f, false, 0.3f, false);
                        justUnlocked = false;
                    }
                }
                else
                {
                    PlayDoorCloseAnimation();
                }
            }
            else
            {
                if (!doorIsOpen)
                {
                    UpdateRelativePlayerDirection();
                    PlayDoorRattleAnimation();
                }
            }
        }
    }

    public void SetDoorVisible(bool doorVisible)
    {
        if (doorIsVisible != doorVisible)
        {
            doorIsVisible = doorVisible;

            foreach (MeshRenderer m in doorMeshes)
            {
                m.enabled = doorVisible;
            }

            foreach (Collider c in doorCollisions)
            {
                c.enabled = doorVisible;
            }
        }
    }

    public void ResetDoor()
    {
        SetDoorVisible(true);
        openOnInteract = false;
        doorAnimator.Play("doorClosed");
        doorIsOpen = false;
        doorLocked = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!openOnInteract && doorIsVisible)
        {
            if (other.gameObject.tag == "Player" && !doorLocked)
            {
                //Figure out which direction the player entered from relative to the door
                UpdateRelativePlayerDirection(other.gameObject.transform);
                PlayDoorOpenAnimation();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (!doorLocked && openOnInteract) openOnInteract = false;
            if (!openOnInteract && doorIsOpen && !doorLocked) PlayDoorCloseAnimation();
        }
    }


    private void PlayDoorOpenAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "openForward" : "openBackward");
        transform.PlayClipAtTransform(openSound, true, 0.4f, true, 0, false);
        doorIsOpen = true;
    }

    private void PlayDoorCloseAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "closeForward" : "closeBackward");
        transform.PlayClipAtTransform(closeSound, true, 0.4f, true, 0, false);
        doorIsClosing = true;
        doorIsOpen = false;
    }

    private void PlayDoorRattleAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "rattleForward" : "rattleBackward");
        transform.PlayClipAtTransform(rattleSound, true, 1, true, 0, false);
    }

    private void UpdateRelativePlayerDirection(Transform player = null)
    {
        if (player == null) player = GameManager.current.player.transform;
        Vector3 playerPosition = player.position;
        Vector3 heading = playerPosition - transform.position;
        relativePlayerDirection = Vector3.Dot(heading, transform.right) > 0 ? 1 : -1;
    }

    public void MakeWave()
    {
        TransitionToWavy();
    }

    private async void TransitionToWavy()
    {
        float wavyTimer = 0;
        float currentWavy = 0;
        float wavySpeed = 0.5f;

        wavyInProgress = true;

        while (currentWavy < 1f && wavyInProgress)
        {
            wavyTimer += Time.deltaTime * wavySpeed;
            currentWavy = Mathf.SmoothStep(0, 1, wavyTimer);

            _SetWavyness(currentWavy);

            await Task.Yield();
        }

        if (!wavyInProgress)
        {
            _SetWavyness(0);
        }
        else
        {
            _SetWavyness(1);
            wavyInProgress = false;
        }

    }

    private void SetMatWaveOnOrOff(bool isWaving, Material mat)
    {
        float variationAmplitudeValue = mat.GetFloat("_VariationAmplitude");

        if (isWaving && variationAmplitudeValue != defaultVariationAmplitude)
        {
            mat.SetFloat("_VariationAmplitude", defaultVariationAmplitude);
        }
        else if (!isWaving && variationAmplitudeValue != 0f)
        {
            mat.SetFloat("_VariationAmplitude", 0f);
        }
    }


    public void SetMaterialVarient(CorridorMatVarient materialVarient)
    {
        if (meshMaterials != null)
        {
            if (meshMaterials[0].GetTexture("_MainTex") != materialVarient.albedo1) meshMaterials[0].SetTexture("_MainTex", materialVarient.albedo1);
            if (meshMaterials[0].GetTexture("_MainTex2") != materialVarient.albedo2) meshMaterials[0].SetTexture("_MainTex2", materialVarient.albedo2);

            if (meshMaterials[1].GetTexture("_MainTex") != materialVarient.albedo1) meshMaterials[1].SetTexture("_MainTex", materialVarient.albedo1);
            if (meshMaterials[1].GetTexture("_MainTex2") != materialVarient.albedo2) meshMaterials[1].SetTexture("_MainTex2", materialVarient.albedo2);
        }
    }
}
