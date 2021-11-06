using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator doorAnimator;
    private int relativePlayerDirection = -1;
    public Transform fakeParent;

    private Material[] meshMaterials;


    private void Awake()
    {
        meshMaterials = GetComponent<MeshRenderer>().materials;
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
    }

    public void SetWavyness(float value)
    {
        meshMaterials[1].SetFloat("_DriftSpeed", value);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Figure out which direction the player entered from relative to the door
            Vector3 playerPosition = other.gameObject.transform.position;
            Vector3 heading = playerPosition - transform.position;
            relativePlayerDirection = Vector3.Dot(heading, transform.right) > 0 ? 1 : -1;

            PlayDoorOpenAnimation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") PlayDoorCloseAnimation();
    }


    private void PlayDoorOpenAnimation() 
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "openForward" : "openBackward");
    }

    private void PlayDoorCloseAnimation()
    {
        doorAnimator.Play(relativePlayerDirection == -1 ? "closeForward" : "closeBackward");
    }
}
