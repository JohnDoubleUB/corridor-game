using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager current;
    public GameObject player;
    public CG_CharacterController playerController;
    public CG_HeadBob headBobber;
    public Camera trueCamera;
    public TVManController tvMan;
    public GameObject GameParent;
    public Canvas GameUI;
    public float PlayerHuntedDangerZone = 6f;
    public float TimeToReachFullDanger = 2f;


    public float HuntingWalkSpeedModifier { get { return huntingWalkSpeedModifier; } set { huntingWalkSpeedModifier = Mathf.Abs(value); } }

    public float WalkSpeedModifier { get { return walkSpeedModifier; } }

    [SerializeField]
    [ReadOnlyField]
    private float walkSpeedModifier = 1f;

    [SerializeField]
    [ReadOnlyField]
    private float huntingWalkSpeedModifier = 1f;

    public bool EnableMouseAcceleration 
    {
        get 
        {
            return playerController != null ? playerController.enableMouseAcceleration : false;
        }

        set 
        {
            if (playerController != null && playerController.enableMouseAcceleration != value) 
            {
                playerController.enableMouseAcceleration = value;
            }
        }
    }


    public bool EnableVariableWalkSpeed 
    { 
        get 
        { 
            return enableVariableWalkSpeed; 
        }
        set 
        {
            if (enableVariableWalkSpeed != value) 
            {
                enableVariableWalkSpeed = value;
                walkSpeedModifier = 1f;

                if (headBobber != null) { headBobber.enableVariableWalkSpeed = enableVariableWalkSpeed; }
                if (playerController != null) { playerController.enableVariableWalkSpeed = enableVariableWalkSpeed; }
            }
        }
    }

    public bool EnableGameUI { 
        get 
        { 
            return enableGameUI; 
        }
        set
        {
            if (value != enableGameUI) 
            {
                enableGameUI = value;
                if (GameUI != null) GameUI.enabled = enableGameUI;
            }
        } 
    }

    [ReadOnlyField]
    [SerializeField]
    private bool enableGameUI = true;

    [ReadOnlyField]
    [SerializeField]
    private bool enableVariableWalkSpeed;

    public float maximumTVManEffectDistance = 10f;
    public bool tvManEffectEnabled = true;

    public Sprite[] DecalNumberSprites;

    public Texture[] DecalNumberTextures { get { return decalNumberTextures; } }

    public PickupablesIndex PickupablesIndex;

    private Texture[] decalNumberTextures;

    public bool IsPaused { get { return isPaused; } }

    private bool isPaused;

    private CursorLockMode lastCursorMode;
    private bool lastCursorVisibility;

    public GameObject pauseMenuObject;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        //Load the decal numbers

        if (DecalNumberSprites != null && DecalNumberSprites.Any() && decalNumberTextures == null || !decalNumberTextures.Any()) 
        {
            decalNumberTextures = DecalNumberSprites.Select(x => x.ConvertSpriteToTexture()).ToArray();
            foreach (Texture decalNoTex in decalNumberTextures) decalNoTex.filterMode = FilterMode.Point;
        }

        if (Time.timeScale != 1) Time.timeScale = 1;
    }

    private void Update()
    {

        //if (Input.GetButtonDown("Cancel"))
        //{
        //    print("End game!");
        //    Application.Quit();
        //}
        if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.P))
        {
            TogglePauseGame();
        }

        TVManEffectUpdate();



        UpdateWalkSpeedModifier();
    }


    public void UpdateWalkSpeedModifier() 
    {
        walkSpeedModifier = Mathf.Clamp(walkSpeedModifier + (Input.mouseScrollDelta.y / 50), 0, 2f);
    }

    public void TogglePauseGame() 
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        isPaused = Time.timeScale == 0;
        if (isPaused)
        {
            lastCursorVisibility = Cursor.visible;
            lastCursorMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            //Revert the mouse state to before it was paused
            Cursor.lockState = lastCursorMode;
            Cursor.visible = lastCursorVisibility;
        }

        pauseMenuObject.SetActive(isPaused);
        //Trigger on pause event here?
    }

    private void TVManEffectUpdate() 
    {
        if (tvMan != null && player != null && MaterialManager.current != null && AudioManager.current != null) 
        {
            float distanceFromPlayer = Vector3.Distance(tvMan.transform.position, player.transform.position);

            if (distanceFromPlayer <= maximumTVManEffectDistance && tvManEffectEnabled && tvMan.IsHunting)
            {
                float remappedValue = distanceFromPlayer.Remap(maximumTVManEffectDistance, tvMan.minimumDistance + 0.5f, 0f, 1f);
                MaterialManager.current.alternateBlend = remappedValue;
                AudioManager.current.SetCreakingVolumeAt(AudioSourceType.FirstPersonPlayer, remappedValue);

            }
            else if (MaterialManager.current.alternateBlend != 0 || AudioManager.current.FirstPersonPlayerSource.isPlaying) 
            {
                MaterialManager.current.alternateBlend = 0;
                AudioManager.current.SetCreakingVolumeAt(AudioSourceType.FirstPersonPlayer, 0f);
            }
            //MaterialManager.current.alternateBlend = Mathf.Lerp(0f, 1f, )


        }
    }

    public void RestartCurrentScene()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
