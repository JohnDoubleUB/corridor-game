using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private void Awake()
    {
        meshMaterials = GetComponent<MeshRenderer>().sharedMaterials;
        doorInteractable.door = this;

        openOnInteractLast = openOnInteract;
        doorInteractable.IsInteractable = openOnInteract;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(MaterialManager.current != null) MaterialManager.current.TrackMaterials(meshMaterials);
    }

    // Update is called once per frame
    void Update()
    {
        if (fakeParent != null && fakeParent.position != transform.position) transform.position = fakeParent.position;


        if (openOnInteract != openOnInteractLast) 
        {
            doorInteractable.IsInteractable = openOnInteract;
            openOnInteractLast = openOnInteract;
        }

        if (doorIsClosing && doorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f) 
        {
            AudioManager.current.PlayClipAt(slamCloseSound, transform.position, 0.4f, true);
            doorIsClosing = false;
        }
    }

    public void SetWavyness(float value)
    {
        meshMaterials[1].SetFloat("_DriftSpeed", value);
    }

    public void InteractOpenClose(bool ignoreLock = false) 
    {
        if (ignoreLock || !doorLocked)
        {
            if (!doorIsOpen)
            {
                UpdateRelativePlayerDirection();
                PlayDoorOpenAnimation();
                if (justUnlocked) 
                {
                    AudioManager.current.PlayClipAt(correctOpenSound, transform.position, 1f, false);
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

    public void ResetDoor() 
    {
        openOnInteract = false;
        doorAnimator.Play("doorClosed");
        doorIsOpen = false;
        doorLocked = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!openOnInteract)
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
        AudioManager.current.PlayClipAt(openSound, transform.position, 0.4f, true);
        doorIsOpen = true;
    }

    private void PlayDoorCloseAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "closeForward" : "closeBackward");
        AudioManager.current.PlayClipAt(closeSound, transform.position, 0.4f, true);
        doorIsClosing = true;
        doorIsOpen = false;
    }

    private void PlayDoorRattleAnimation() 
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "rattleForward" : "rattleBackward");
        AudioManager.current.PlayClipAt(rattleSound, transform.position, 1f, true);
    }

    private void UpdateRelativePlayerDirection(Transform player = null) 
    {
        if (player == null) player = GameManager.current.player.transform;
        Vector3 playerPosition = player.position;
        Vector3 heading = playerPosition - transform.position;
        relativePlayerDirection = Vector3.Dot(heading, transform.right) > 0 ? 1 : -1;
    }
}
