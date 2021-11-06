using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator doorAnimator;
    public DoorInteractable doorInteractable;
    private int relativePlayerDirection = -1;
    public Transform fakeParent;
    
    public bool openOnInteract;
    public bool doorLocked;

    public bool IsOpen { get { return doorIsOpen; } }

    private bool doorIsOpen;
    private Material[] meshMaterials;
    private bool openOnInteractLast;


    private void Awake()
    {
        meshMaterials = GetComponent<MeshRenderer>().materials;
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
        if (other.gameObject.tag == "Player" && !openOnInteract && doorIsOpen && !doorLocked) PlayDoorCloseAnimation();
    }


    private void PlayDoorOpenAnimation() 
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "openForward" : "openBackward");
        doorIsOpen = true;
    }

    private void PlayDoorCloseAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "closeForward" : "closeBackward");
        doorIsOpen = false;
    }

    private void PlayDoorRattleAnimation() 
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "rattleForward" : "rattleBackward");
    }

    private void UpdateRelativePlayerDirection(Transform player = null) 
    {
        if (player == null) player = GameManager.current.player.transform;
        Vector3 playerPosition = player.position;
        Vector3 heading = playerPosition - transform.position;
        relativePlayerDirection = Vector3.Dot(heading, transform.right) > 0 ? 1 : -1;
    }
}
