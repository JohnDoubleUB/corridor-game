using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshChangeTestInteractable : InteractableObject
{
    public bool toggleOn;
    private MeshFilter thisMesh;
    private MeshCollider thisCollider;

    public Mesh meshOn;
    public Mesh meshOff;


    private void Awake()
    {
        thisMesh = GetComponent<MeshFilter>();
        thisCollider = GetComponent<MeshCollider>();

        UpdateMesh();
    }

    protected override void OnInteract()
    {
        toggleOn = !toggleOn;
        UpdateMesh();
      
    }

    private void UpdateMesh() 
    {
        thisMesh.sharedMesh = toggleOn ? meshOn : meshOff;
        thisCollider.sharedMesh = toggleOn ? meshOn : meshOff;
    }
}
