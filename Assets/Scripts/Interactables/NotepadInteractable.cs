﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NotepadInteractable : InteractableObject
{
    public AudioClip pickupSound;
    public bool varyPickupSoundPitch;
    public float pickupSoundVolume = 0.5f;
    public float pickupSpeedMultiplier = 2.5f;

    protected override void OnInteract()
    {
        //Do thing here
        GameManager.current.playerController.NotepadPickedUp = true;
        Pickup();
    }

    private async void Pickup()
    {
        OnSuccessfulInteract();
        transform.parent = null;
        IsInteractable = false;

        if (pickupSound != null)
        {
            AudioManager.current.PlayClipAt(pickupSound, transform.position, pickupSoundVolume, varyPickupSoundPitch).transform.SetParent(transform);
        }

        Vector3 positionAtTimeOfPickup = transform.position;
        float positionValue = 0;
        float smoothedPositionValue;
        Vector3 cameraPosition;


        while (positionValue < 1f)
        {
            cameraPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3(0, 0.6f, 0);
            positionValue += Time.deltaTime * pickupSpeedMultiplier;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            transform.SetPositionAndRotation(
                Vector3.Lerp(positionAtTimeOfPickup, cameraPosition, smoothedPositionValue),
                Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(cameraPosition - transform.position), smoothedPositionValue * 50f)
                );
            await Task.Yield();
        }

        Destroy(gameObject);
    }
}
