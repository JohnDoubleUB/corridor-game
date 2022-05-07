using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CG_CharacterController : MonoBehaviour, IHuntableEntity
{
    public PSXRendererHandler PSXRenderer;
    public CG_UIHandler UIHandler;

    public bool cutsceneMode;
    public bool enableVariableWalkSpeed;
    public bool enableMouseAcceleration;

    public float crouchingAmount = 4f;
    public AnimationCurve cameraCrouchingAnimationCurve;

    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public bool NotepadPickedUp; //Controls if the notepad can actually be used (if the player has grabbed it in the level)

    private Vector2 velocity;
    public Vector2 acceleration;

    private float huntedTimer;

    [SerializeField]
    [ReadOnlyField]
    private float _appliedSpeed;
    private float AppliedSpeed
    {
        get
        {
            _appliedSpeed = enableVariableWalkSpeed && GameManager.current != null ? speed * GameManager.current.WalkSpeedModifier : speed * GameManager.current.HuntingWalkSpeedModifier;
            return _appliedSpeed;
        }
    }

    private bool isBeingHunted;

    private bool notBeingKilled = true;

    public CG_HeadBob HeadBobber;

    public bool IsJumping { get { return isJumping; } }
    public bool IsIlluminated { get { return isIlluminated; } }
    public bool IsCrouching { get { return isCrouching; } }

    public Transform EntityTransform => transform;

    public EntityType EntityType { get { return EntityType.Player; } }

    public GameObject EntityGameObject => gameObject;

    public GameObject playerPencil;

    public Animator NotepadAnimator;

    public Transform footStepPosition;
    public AudioClip[] playerLandSounds;

    public Transform CameraOffsetTransform;

    CharacterController characterController;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    [HideInInspector]
    public bool canMove = true;

    [HideInInspector]
    public bool canInteract = true;

    private InteractableNote interactingNote;

    private InputMaster controls;

    private GameObject currentInteractableGameObject;
    private InteractableObject currentInteractable;

    private GameObject notepadGameObject;
    public Notepad notepadObject;

    private int pencilLayerMask;

    private List<InteractableCandle> candlesInRangeOfPlayer = new List<InteractableCandle>();
    private float standingColliderHeight;
    private float crouchingColliderHeight;
    private float standingMovementSpeed;
    private float crouchingMovementSpeed;

    private float cameraCrouchingAmount;

    private Vector3 standingCameraTransformOffset;
    private Vector3 crouchingCameraTransformOffset;

    public MouseEntity heldMouse;
    public Transform mouseHand;

    [SerializeField]
    [ReadOnlyField]
    private bool canUncrouch = true;

    [SerializeField]
    [ReadOnlyField]
    private bool isIlluminated;

    [SerializeField]
    [ReadOnlyField]
    private bool isJumping;

    [SerializeField]
    [ReadOnlyField]
    private bool isCrouching;

    [SerializeField]
    [ReadOnlyField]
    private bool isInNotepad;

    public Animator playerCameraAnimator;


    private float DistanceFromTVMan
    {
        get
        {
            Vector3 currentTVManLocation = GameManager.current.tvMan.transform.position;
            currentTVManLocation.y = transform.position.y;

            return Vector3.Distance(transform.position, currentTVManLocation);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Awake()
    {
        controls = new InputMaster();
        controls.Player.Interact.performed += _ => Interact();
        pencilLayerMask = 1 << LayerMask.NameToLayer("Notepad") | 1 << LayerMask.NameToLayer("NonWritingArea");
        CorridorChangeManager.OnLevelChange += OnLevelChange;
        CorridorChangeManager.OnSaveGame += OnSaveGame;
        if (playerCameraAnimator != null) playerCameraAnimator.Play("Idle", 0);
    }

    public void CandleEnterPlayerInRange(InteractableCandle candle)
    {
        if (!candlesInRangeOfPlayer.Contains(candle)) candlesInRangeOfPlayer.Add(candle);
    }

    public void CandleExitPlayerInRange(InteractableCandle candle)
    {
        if (candlesInRangeOfPlayer.Contains(candle)) candlesInRangeOfPlayer.Remove(candle);
    }

    public void Interact(InteractableNote note)
    {
        interactingNote = note;
        bool isInteractingWithNote = interactingNote != null;

        UIHandler.CrosshairVisibilty = !isInteractingWithNote;
        UIHandler.InteractionPromptVisiblity = !isInteractingWithNote;
    }

    private void Interact()
    {
        if (notBeingKilled)
        {
            if (interactingNote == null)
            {
                if (currentInteractable != null && canInteract)
                {
                    currentInteractable.IntiateInteract();
                }
            }
            else
            {
                interactingNote.PutDownItem();
            }
        }
    }

    private void OnLevelChange()
    {
        UIHandler.ShowLevelPrompt();
    }

    private void OnSaveGame()
    {

        UIHandler.ShowSavePrompt();
    }

    void Start()
    {
        PSXRenderer.ResetMat();
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;// ? does this fix the initial mouse visibility issue?
        standingColliderHeight = characterController.height;
        crouchingColliderHeight = standingColliderHeight / crouchingAmount;
        standingMovementSpeed = speed;
        crouchingMovementSpeed = standingMovementSpeed / crouchingAmount;
        standingCameraTransformOffset = CameraOffsetTransform.localPosition;
        crouchingCameraTransformOffset = new Vector3(standingCameraTransformOffset.x, standingCameraTransformOffset.y - crouchingAmount * 0.3f, standingCameraTransformOffset.z);
        if (SaveSystem.NotepadLoadType == GameLoadType.Existing) notepadObject.LoadData();
        else notepadObject.LoadRandomNote();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HideUnderable") canUncrouch = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "HideUnderable") canUncrouch = true;
    }

    private void UpdateHuntedWalkSpeedModifier()
    {
        if (isBeingHunted)
        {
            float currentDistanceFromTVMan = DistanceFromTVMan;
            float dangerZone = GameManager.current.PlayerHuntedDangerZone;
            float tvManMinDistance = GameManager.current.tvMan.minimumDistance;

            if (GameManager.current.tvMan.CanSeeTarget)
            {
                if (currentDistanceFromTVMan < dangerZone) //Check if player is inside of tvman danger zone
                {
                    //float modifiedMinDistance = tvManMinDistance; //+ 0.5f;
                    if (huntedTimer != GameManager.current.TimeToReachFullDanger) huntedTimer = Mathf.Min(huntedTimer + Time.deltaTime, GameManager.current.TimeToReachFullDanger);
                    float remappedDistance = currentDistanceFromTVMan.Remap(tvManMinDistance, dangerZone, 0, 1f) * huntedTimer.Remap(0, GameManager.current.TimeToReachFullDanger, 1, 0);
                    GameManager.current.HuntingWalkSpeedModifier = remappedDistance;
                    //Check how close
                }
            }
            else
            {
                GameManager.current.HuntingWalkSpeedModifier = Mathf.Min(GameManager.current.HuntingWalkSpeedModifier + (Time.deltaTime / 100), 1f);
            }
            //Force tvman to kill player if he can't move, (to prevent softlock)
            if (_appliedSpeed < 0.02f) GameManager.current.tvMan.ForceKillTarget();
        }
        else if (GameManager.current.HuntingWalkSpeedModifier != 1f)
        {
            GameManager.current.HuntingWalkSpeedModifier = Mathf.Min(GameManager.current.HuntingWalkSpeedModifier + (Time.deltaTime / 10), 1f);
        }
    }

    void Update()
    {
        UpdateHuntedWalkSpeedModifier();

        isIlluminated = candlesInRangeOfPlayer.Any(x => x.IsIlluminatingPlayer);
        UIHandler.PlayerVisibilityPrompt = !isIlluminated;

        if (notBeingKilled && !GameManager.current.IsPaused)
        {
            UpdateInteractable();
            if (InventoryManager.current.HasMomento != UIHandler.MomentoTextVisibility) UIHandler.MomentoTextVisibility = InventoryManager.current.HasMomento;
            if (!canMove) UpdateDraw();

            bool playerNotBusy = canMove && interactingNote == null;

            if (!cutsceneMode && characterController.isGrounded)
            {
                // We are grounded, so recalculate move direction based on axes
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);

                float curSpeedX = playerNotBusy ? AppliedSpeed * Input.GetAxis("Vertical") : 0;
                float curSpeedY = playerNotBusy ? AppliedSpeed * Input.GetAxis("Horizontal") : 0;
                moveDirection = (forward * curSpeedX) + (right * curSpeedY);

                if (isJumping)
                {
                    isJumping = false;
                    if (playerLandSounds != null && playerLandSounds.Any()) footStepPosition.PlayClipAtTransform(playerLandSounds[Random.Range(0, playerLandSounds.Length)], false, 0.2f);
                }

                if (Input.GetButtonDown("Jump") && playerNotBusy/*canMove*/)
                {
                    moveDirection.y = jumpSpeed;
                    isJumping = true;
                }

                if (Input.GetMouseButtonDown(0) && heldMouse != null)
                {
                    if (GetLookedAtPoint(out Vector3 target))
                    {
                        heldMouse.transform.SetParent(null);
                        heldMouse.transform.position = playerCamera.transform.position + playerCamera.transform.TransformDirection(Vector3.forward);
                        heldMouse.ThrowAtTarget(target, 10f, playerCamera.transform.forward);
                        heldMouse = null;
                    }
                }

            }

            if (cutsceneMode) moveDirection = Vector3.zero;
            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);


            // Player and Camera rotation
            if (playerNotBusy/*canMove*/)
            {
                Vector2 wantedVelocity = GetInput() * lookSpeed;

                velocity = enableMouseAcceleration ? new Vector2(
                    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime))
                    : wantedVelocity;

                rotation += velocity * Time.deltaTime;
                rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
                transform.eulerAngles = new Vector2(0, rotation.y);
            }

            if (!cutsceneMode && Input.GetKeyDown(KeyCode.LeftShift) && NotepadPickedUp && interactingNote == null)
            {
                ActivateNotepad(canMove);
            }


            if (!isInNotepad && !cutsceneMode && Input.GetButtonDown("Crouch") && ((isCrouching && canUncrouch) || !isCrouching)) 
            {
                ToggleCrouching();
            }

            if (isCrouching && cameraCrouchingAmount != 1 || !isCrouching && cameraCrouchingAmount != 0) 
            {
                //TODO: Curve was being used here but there were issues with this not working as intended needs more investigation if we want to use this to make the animation smoother
                cameraCrouchingAmount = Mathf.Clamp(isCrouching ? cameraCrouchingAmount + Time.deltaTime * crouchingAmount : cameraCrouchingAmount - Time.deltaTime * crouchingAmount, 0, 1);
                CameraOffsetTransform.localPosition = Vector3.Lerp(standingCameraTransformOffset, crouchingCameraTransformOffset, cameraCrouchingAmount);
            }
        }
    }


    private void ToggleCrouching()
    {
        //Crouching amount
        isCrouching = !isCrouching;

        //Tell headbobber we are crouching
        HeadBobber.SetCrouching(isCrouching);
        
        characterController.height = isCrouching ? crouchingColliderHeight : standingColliderHeight;
        characterController.center = isCrouching ? new Vector3(0f, -crouchingColliderHeight * 1.5f, 0f) : Vector3.zero;
        speed = isCrouching ? crouchingMovementSpeed : standingMovementSpeed;
    }

    private Vector2 GetInput()
    {
        return new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
    }

    private void ActivateNotepad(bool activate)
    {
        isInNotepad = activate;
        canMove = !activate;
        canInteract = !activate;
        Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = false;
        NotepadAnimator.Play(canMove ? "Dequip" : "Equip");
        playerPencil.SetActive(!canMove);
        UIHandler.CrosshairVisibilty = canMove;
        if (!activate)
        {
            SaveSystem.NotepadLoadType = GameLoadType.Existing;
            notepadObject.SaveData();
        }
    }
    private bool GetLookedAtPoint(out Vector3 result)
    {
        Vector3 tempOrigin = playerCamera.transform.position;
        Vector3 tempDirection = playerCamera.transform.TransformDirection(Vector3.forward);

        RaycastHit lookedAtObject;

        bool resultBool = Physics.Raycast(tempOrigin, tempDirection, out lookedAtObject, float.MaxValue, LayerMask.GetMask("CorridorCollision", "Default"));


        result = lookedAtObject.point;
        return resultBool;
    }



    private void UpdateInteractable(bool showDebug = false)
    {
        if (playerCamera != null)
        {
            RaycastHit lookedAtObject;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out lookedAtObject, 2f, LayerMask.GetMask("Interactables", "InteractableNoCollide")))
            {
                if (showDebug) Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * lookedAtObject.distance, Color.yellow);

                GameObject inViewInteractable = lookedAtObject.collider.gameObject;

                if (currentInteractableGameObject != inViewInteractable)
                {
                    currentInteractableGameObject = lookedAtObject.collider.gameObject;
                    currentInteractable = currentInteractableGameObject.GetComponent<InteractableObject>();

                    UIHandler.InteractionText = currentInteractable.IsInteractable ? currentInteractable.ObjectName : "";
                    if (currentInteractable.IsInteractable) UIHandler.CurrentCrosshairType = CrosshairType.Interacting;
                }
                else if (currentInteractable.IsInteractable != (UIHandler.CurrentCrosshairType == CrosshairType.Interacting))
                {
                    //This makes it so that the interact crosshair updates if the interactable state changes while being looked at
                    UIHandler.CurrentCrosshairType = currentInteractable.IsInteractable ? CrosshairType.Interacting : CrosshairType.Default;
                }
            }
            else
            {
                if (showDebug) Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * 5, Color.white);

                if (currentInteractableGameObject != null)
                {
                    currentInteractableGameObject = null;
                    currentInteractable = null;
                    UIHandler.InteractionText = string.Empty;
                    UIHandler.CurrentCrosshairType = CrosshairType.Default;
                }
            }
        }
    }

    private void UpdateDraw(bool showDebug = false)
    {
        if (GameManager.current.trueCamera != null)
        {
            RaycastHit lookedAtObject;

            Ray ray = GameManager.current.trueCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out lookedAtObject, 2f, pencilLayerMask))
            {
                if (showDebug) Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * lookedAtObject.distance, Color.green);

                GameObject inViewPencilLayerObj = lookedAtObject.collider.gameObject;

                bool nonWritingArea = inViewPencilLayerObj.layer == LayerMask.NameToLayer("NonWritingArea");

                if (notepadGameObject != inViewPencilLayerObj)
                {
                    notepadGameObject = inViewPencilLayerObj;

                    //Only store the notepad once!
                    if (notepadObject == null) notepadObject = inViewPencilLayerObj.GetComponent<Notepad>();

                    if (notepadObject != null)
                    {
                        notepadObject.PencilOverPadArea = !nonWritingArea;
                        if (nonWritingArea) notepadObject.ClearCurrentLine();
                    }
                }

                if (notepadObject != null && !nonWritingArea) notepadObject.DrawAt(lookedAtObject.point);

                if (playerPencil != null)
                {
                    if (!playerPencil.activeInHierarchy) playerPencil.SetActive(true);
                    playerPencil.transform.position = lookedAtObject.point;
                }
            }
            else if (notepadObject != null)
            {
                if (playerPencil.activeInHierarchy) playerPencil.SetActive(false);
            }
        }
    }

    public void OnBeingHunted(bool beingHunted)
    {
        isBeingHunted = beingHunted;

        if (beingHunted)
        {
            huntedTimer = 0f;
            print("Player is being hunted");
        }
        else
        {
            print("Player is no longer being hunted");
        }
    }

    public void OnEntityKilled()
    {
        SetPlayerIsBeingKilled(true);
        LookTowardsTVMan();
    }

    private void SetPlayerIsBeingKilled(bool beingKilled)
    {
        notBeingKilled = !beingKilled;
        UIHandler.CrosshairVisibilty = !beingKilled;
        CancelAllActions();

    }

    public void CancelAllActions()
    {
        if (IsCrouching && canUncrouch) ToggleCrouching();
        if (interactingNote != null) interactingNote.PutDownItem();
        if (isInNotepad) ActivateNotepad(false);
    }

    public void EnableCutsceneMode(bool cutsceneMode)
    {
        this.cutsceneMode = cutsceneMode;
        UIHandler.CrosshairVisibilty = !cutsceneMode;
        CancelAllActions();
    }

    public void LoadSavedPlayerData(PlayerData playerData)
    {
        NotepadPickedUp = playerData.NotepadPickedUp;
    }

    private async void LookTowardsTVMan()
    {
        float positionValue = 0;
        float smoothedPositionValue;

        //Vector3 newPlayerRotation = transform.rotation.eulerAngles + GameManager.current.tvMan.transform.eulerAngles;

        Vector3 tvManPosition = new Vector3(GameManager.current.tvMan.transform.position.x, transform.position.y, GameManager.current.tvMan.transform.position.z);
        Quaternion newPlayerRotation = Quaternion.LookRotation(tvManPosition - transform.position, Vector3.up);
        Quaternion currentPlayerRotation = transform.rotation;
        Quaternion newCameraRotation = Quaternion.LookRotation((GameManager.current.tvMan.TvManEyeLevel.position - Vector3.up * 0.3f) - playerCamera.transform.position);
        Quaternion currentCameraRotation = playerCamera.transform.rotation;



        while (positionValue < 1f)
        {
            positionValue += Time.deltaTime;
            smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
            transform.rotation = Quaternion.Lerp(currentPlayerRotation, newPlayerRotation, smoothedPositionValue);//Quaternion.RotateTowards(currentPlayerRotation, newPlayerRotation, smoothedPositionValue * 50f);
            playerCamera.transform.rotation = Quaternion.Lerp(currentCameraRotation, newCameraRotation, smoothedPositionValue);
            await Task.Yield();
        }

        print("done");
        transform.rotation = newPlayerRotation;

        //Check if player has a momento

        InventoryManager iM = InventoryManager.current;

        positionValue = 0;

        if (iM.HasMomento)
        {
            InventorySlot iS = iM.momentoSlots[0];


            while (positionValue < 1f)
            {
                //Move momentoSlot To center
                positionValue += Time.deltaTime;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                //iS.SlotTransform
                iS.SlotParentTransform.localPosition = Vector3.Slerp(iS.SlotParentInitialPosition, iM.ScreenCenter.localPosition, smoothedPositionValue);

                await Task.Yield();
            }

            positionValue = 0;

            while (positionValue < 1f)
            {
                //Move momentoSlot To center
                positionValue += Time.deltaTime;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                PSXRenderer.FadeToWhite = smoothedPositionValue;
                await Task.Yield();
            }

            GameManager.current.tvMan.RemoveFromPlay(true);
            Destroy(iS.RemoveItemFromContents().gameObject);
            iM.HasMomento = false;
            SetPlayerIsBeingKilled(false);

            positionValue = 0;

            while (positionValue < 1f)
            {
                //Move momentoSlot To center
                positionValue += Time.deltaTime * 0.5f;
                smoothedPositionValue = Mathf.SmoothStep(1, 0, positionValue);
                PSXRenderer.FadeToWhite = smoothedPositionValue;
                await Task.Yield();
            }


            iS.SlotParentTransform.localPosition = iS.SlotParentInitialPosition;
        }
        else
        {
            if (playerCameraAnimator != null) playerCameraAnimator.Play("Death", 0);
            while (positionValue < 1f)
            {
                positionValue += Time.deltaTime;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                PSXRenderer.AlternateTransistion = smoothedPositionValue;
                await Task.Yield();
            }


        }
    }
}
