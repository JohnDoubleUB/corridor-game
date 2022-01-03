using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class InteractableNote : InteractableObject
{
    private Material NoteMaterial;
    private bool NoteInitiated;
    private bool inTransitionState;

    private Quaternion defaultRotation;
    private Vector3 defaultPosition;

    [Header("Note Specific Settings")]
    public MeshRenderer NoteMeshRenderer;
    public MeshFilter NoteMeshFilter;

    public Texture NoteTexture;
    public Mesh[] PossibleMeshes;

    public float pickupSpeedMultiplier = 2.5f;
    public float distanceToStopFromPlayer = 0.3f;

    private void Awake()
    {
        //Establish default rotation and position
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;

        if (!NoteInitiated)
        {
            //Set up the material to display the right texture
            if (NoteMeshRenderer != null)
            {
                NoteMaterial = NoteMeshRenderer.material;
                if (NoteTexture != null && NoteMaterial.mainTexture != NoteTexture) NoteMaterial.mainTexture = NoteTexture;
            }

            //Randomly Select a mesh if there is a selection of meshes
            if (PossibleMeshes.Any())
            {
                Mesh selectedMesh = PossibleMeshes[Random.Range(0, PossibleMeshes.Length)];
                if (selectedMesh != NoteMeshFilter.mesh) NoteMeshFilter.mesh = selectedMesh;
            }

            NoteInitiated = true;
        }
    }

    protected override void OnInteract()
    {
        ShowToPlayer();
    }

    private async void ShowToPlayer() 
    {
        IsInteractable = false;
        inTransitionState = true;
        GameManager.current.playerController.Interact(this);

        Vector3 positionAtTimeOfPickup = transform.position;
        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 cameraPosition = Vector3.zero;
        Quaternion lookRotation = Quaternion.identity;

        while (positionValue < 1f) 
        {

            cameraPosition = GameManager.current.playerController.playerCamera.transform.position + (GameManager.current.playerController.playerCamera.transform.forward * distanceToStopFromPlayer);
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            lookRotation = Quaternion.LookRotation(cameraPosition - transform.position);


            transform.SetPositionAndRotation(
                Vector3.Slerp(positionAtTimeOfPickup, cameraPosition, smoothedPositionValue),
                Quaternion.RotateTowards(transform.rotation, lookRotation, smoothedPositionValue * 50f)
                );

            await Task.Yield();
        }

        transform.SetPositionAndRotation(
              cameraPosition,
              lookRotation
              );

        inTransitionState = false;
    }

    public void PutDownItem() 
    {
        if (!inTransitionState) ReturnToDefaultPosition();
    }

    private async void ReturnToDefaultPosition() 
    {
        inTransitionState = true;
        Vector3 positionAtTimeOfPutDown = transform.localPosition;
        Quaternion rotationAtTimeOfPutDown = transform.localRotation;
        float positionValue = 0;
        float smoothedPositionValue;

        while (positionValue < 1f)
        {
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);

            transform.localPosition = Vector3.Slerp(positionAtTimeOfPutDown, defaultPosition, smoothedPositionValue);
            transform.localRotation = Quaternion.Slerp(rotationAtTimeOfPutDown, defaultRotation, smoothedPositionValue);

            await Task.Yield();
        }

        transform.localPosition = defaultPosition;
        transform.localRotation = defaultRotation;

        IsInteractable = true;
        inTransitionState = false;
        
        GameManager.current.playerController.Interact(null);
    }
}
