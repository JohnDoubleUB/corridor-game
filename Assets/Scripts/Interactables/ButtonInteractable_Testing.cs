using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractable_Testing : InteractableObject
{
    public Transform ButtonMeshTransform;
    public float buttonInAmount = 0.009f;
    public float timeInMultiplier = 4;
    public bool makeButtonInert = false;

    public Text buttonText;

    public MeshRenderer buttonMesh;

    private Material buttonMaterial;

    public Color activated;
    public Color deactivated;

    private bool buttonPushed;
    private float timer = 0;

    private Vector3 buttonOutPosition;
    private Vector3 buttonInPosition;

    public TestButtonEffectType testEffectType;

    private bool IsButtonPropertyActive { 
        get 
        {
            switch (testEffectType) 
            {
                case TestButtonEffectType.Spin:
                    return GameParentTestScript.current.dospin;

                case TestButtonEffectType.TVMan:
                    return GameManager.current.tvMan.IsHunting;

                default:
                    return AudioManager.current.GetAmbientTrackByIndex(GetAmbientTrackIndex(testEffectType)).trackOn;
            }
        } 
    }

    private int GetAmbientTrackIndex(TestButtonEffectType TestButtonEffectIndex) 
    {
        switch (TestButtonEffectIndex) 
        {
            case TestButtonEffectType.ToggleTrack1:
                return 1;

            case TestButtonEffectType.ToggleTrack2:
                return 2;

            case TestButtonEffectType.ToggleTrack3:
                return 3;

            case TestButtonEffectType.ToggleTrack4:
                return 4;

            case TestButtonEffectType.ToggleTrack5:
                return 5;

            default:
                return 0;
        }
    }

    private void Awake()
    {
        if (ButtonMeshTransform != null) 
        {
            buttonOutPosition = ButtonMeshTransform.localPosition;
            buttonInPosition = new Vector3(buttonOutPosition.x + buttonInAmount, buttonOutPosition.y, buttonOutPosition.z);
        }
    }

    protected override void OnInteract() 
    {
        if (!buttonPushed) 
        {
            buttonPushed = true;

            switch (testEffectType) 
            {
                case TestButtonEffectType.Spin:
                    GameParentTestScript.current.dospin = !GameParentTestScript.current.dospin;
                    break;

                case TestButtonEffectType.TVMan:
                    GameManager.current.tvMan.IsHunting = !GameManager.current.tvMan.IsHunting;
                    GameManager.current.tvMan.teleportAwayWhenAtMinimumDistance = !GameManager.current.tvMan.teleportAwayWhenAtMinimumDistance;
                    break;

                default:
                    MusicMixerTrack track = AudioManager.current.GetAmbientTrackByIndex(GetAmbientTrackIndex(testEffectType));
                    track.trackOn = !track.trackOn;
                    break;
            }

            buttonMaterial.SetColor("_BaseColor", IsButtonPropertyActive ? activated : deactivated);
        }
    }

    private void Start()
    {
        if (buttonMesh != null) { 
            buttonMaterial = buttonMesh.material;
            buttonMaterial.SetColor("_BaseColor", IsButtonPropertyActive ? activated : deactivated);
        }
    }

    private void Update()
    {
        if (!makeButtonInert)
        {
            if (buttonPushed && ButtonMeshTransform.position.x != buttonInPosition.x)
            {
                ButtonMeshTransform.localPosition = buttonInPosition;
            }
            else if (!buttonPushed && ButtonMeshTransform.position.x != buttonOutPosition.x)
            {
                ButtonMeshTransform.localPosition = buttonOutPosition;
            }

            if (buttonPushed)
            {
                if (timer < 1)
                {
                    timer += Time.deltaTime * timeInMultiplier;
                }
                else
                {
                    buttonPushed = false;
                    timer = 0;
                }
            }
        }
    }
}

public enum TestButtonEffectType 
{
    Spin,
    TVMan,
    ToggleTrack0,
    ToggleTrack1,
    ToggleTrack2,
    ToggleTrack3,
    ToggleTrack4,
    ToggleTrack5
}