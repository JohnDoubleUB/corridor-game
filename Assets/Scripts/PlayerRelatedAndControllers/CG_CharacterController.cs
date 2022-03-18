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
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public bool NotepadPickedUp; //Controls if the notepad can actually be used (if the player has grabbed it in the level)
    public Text[] DialogueBoxes;

    private bool notBeingKilled = true;

    public CG_HeadBob HeadBobber;

    public bool IsJumping { get { return isJumping; } }
    public bool IsIlluminated { get { return isIlluminated; } }
    public bool IsCrouching { get { return isCrouching; } }

    public Transform EntityTransform => transform;

    public EntityType EntityType { get { return EntityType.Player; } }

    public GameObject EntityGameObject => gameObject;

    public GameObject playerPencil;
    public Image playerCrosshair;
    public Text momentoText;

    public Sprite crosshairNormal;
    public Sprite crosshairInteract;

    public Text interactionPrompt;
    public Text levelChangePrompt;

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
    private float defaultColliderHeight;
    private float defaultMovementSpeed;

    private Vector3 defaultCameraTransformOffset;

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

    public Material pSXMaterial;
    public Animator playerCameraAnimator;

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

        playerCrosshair.enabled = !isInteractingWithNote;
        interactionPrompt.enabled = !isInteractingWithNote;
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
        ShowLevelChangePrompt();
    }

    private async void ShowLevelChangePrompt()
    {
        if (levelChangePrompt != null)
        {
            float positionValue = 0f;
            float positionSmoothed;
            Color newColor;

            while (positionValue < 1)
            {
                positionValue += Time.deltaTime * 0.1f;
                positionSmoothed = Mathf.SmoothStep(0, 255, positionValue);
                newColor = levelChangePrompt.color;
                newColor.a = positionSmoothed;
                levelChangePrompt.color = newColor;
                await Task.Yield();
            }

            newColor = levelChangePrompt.color;
            newColor.a = 0f;
            levelChangePrompt.color = newColor;
        }
    }

    void Start()
    {
        pSXMaterial.SetFloat("_TransitionToAlternate", 0);
        pSXMaterial.SetFloat("_FadeToWhite", 0);
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;// ? does this fix the initial mouse visibility issue?
        defaultColliderHeight = characterController.height;
        defaultMovementSpeed = speed;
        defaultCameraTransformOffset = CameraOffsetTransform.localPosition;
        notepadObject.LoadData();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HideUnderable") canUncrouch = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "HideUnderable") canUncrouch = true;
    }

    void Update()
    {
        isIlluminated = candlesInRangeOfPlayer.Any(x => x.IsIlluminatingPlayer);

        if (notBeingKilled)
        {
            UpdateInteractable();
            if (InventoryManager.current.HasMomento != momentoText.enabled) momentoText.enabled = InventoryManager.current.HasMomento;
            if (!canMove) UpdateDraw();

            bool playerNotBusy = canMove && interactingNote == null;

            if (characterController.isGrounded)
            {
                // We are grounded, so recalculate move direction based on axes
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                //float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
                //float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;

                float curSpeedX = playerNotBusy ? speed * Input.GetAxis("Vertical") : 0;
                float curSpeedY = playerNotBusy ? speed * Input.GetAxis("Horizontal") : 0;
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

                //Just for testing
                if (Input.GetMouseButtonDown(0) && heldMouse != null)
                {
                    if (GetLookedAtPoint(out Vector3 target))
                    {
                        heldMouse.transform.SetParent(null);
                        heldMouse.transform.position = playerCamera.transform.position + playerCamera.transform.TransformDirection(Vector3.forward);
                        heldMouse.ThrowAtTarget(target, 10f, playerCamera.transform.forward);
                        heldMouse = null;
                    }
                    //Rigidbody newProjectile = Instantiate(testProjectilePrefab, playerCamera.transform.position + playerCamera.transform.TransformDirection(Vector3.forward), Quaternion.identity);
                    //if (GetLookedAtPoint(out Vector3 target)) newProjectile.LaunchAtTarget(target, 10f);
                }

            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);



            // Player and Camera rotation
            if (playerNotBusy/*canMove*/)
            {
                rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
                rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
                rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
                transform.eulerAngles = new Vector2(0, rotation.y);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && NotepadPickedUp && interactingNote == null)
            {
                ActivateNotepad(canMove);
            }




            bool crouchButtonHeld = Input.GetButton("Crouch");

            if (crouchButtonHeld != isCrouching && ((isCrouching && canUncrouch) || !isCrouching))
            {
                ToggleCrouching();
            }
        }
        else 
        {

            //transform.rotation = Quaternion.LookRotation(GameManager.current.tvMan.transform.position - transform.position, Vector3.up);
        }

        if (Input.GetButtonDown("Cancel"))
        {
            print("End game!");
            Application.Quit();
        }
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
        playerCrosshair.enabled = canMove;

        if (!activate)
        {
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

    private void ToggleCrouching()
    {
        float downAmount = 4f;
        isCrouching = !isCrouching;
        HeadBobber.SetCrouching(isCrouching);
        characterController.height = isCrouching ? defaultColliderHeight / downAmount : defaultColliderHeight;
        speed = isCrouching ? defaultMovementSpeed / downAmount : defaultMovementSpeed;
        if (!isCrouching) transform.position += Vector3.up * (defaultColliderHeight / (downAmount / 2));
        CameraOffsetTransform.localPosition = isCrouching ? new Vector3(defaultCameraTransformOffset.x, defaultCameraTransformOffset.y / downAmount, defaultCameraTransformOffset.z) : defaultCameraTransformOffset;
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

                    interactionPrompt.text = currentInteractable.IsInteractable ? currentInteractable.ObjectName : "";
                    if (currentInteractable.IsInteractable) playerCrosshair.sprite = crosshairInteract;
                }
                else if (currentInteractable.IsInteractable != (playerCrosshair.sprite == crosshairInteract))
                {
                    //This makes it so that the interact crosshair updates if the interactable state changes while being looked at
                    playerCrosshair.sprite = currentInteractable.IsInteractable ? crosshairInteract : crosshairNormal;
                }
            }
            else
            {
                if (showDebug) Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward) * 5, Color.white);

                if (currentInteractableGameObject != null)
                {
                    currentInteractableGameObject = null;
                    currentInteractable = null;
                    interactionPrompt.text = "";
                    playerCrosshair.sprite = crosshairNormal;
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
        print("Player is being hunted");
    }

    public void OnEntityKilled()
    {
        SetPlayerIsBeingKilled(true);
        //notBeingKilled = false;
        ////Uncrouch, un look at note, put away notepad
        //if (IsCrouching && canUncrouch) ToggleCrouching();
        //if (interactingNote != null) interactingNote.PutDownItem();
        //if (isInNotepad) ActivateNotepad(false);

        LookTowardsTVMan();
    }

    private void SetPlayerIsBeingKilled(bool beingKilled) 
    {
        notBeingKilled = !beingKilled;
        playerCrosshair.enabled = !beingKilled;
        //Uncrouch, un look at note, put away notepad
        if (IsCrouching && canUncrouch) ToggleCrouching();
        if (interactingNote != null) interactingNote.PutDownItem();
        if (isInNotepad) ActivateNotepad(false);
        
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
                pSXMaterial.SetFloat("_FadeToWhite", smoothedPositionValue);
                await Task.Yield();
            }

            GameManager.current.tvMan.RemoveFromPlay();
            Destroy(iS.RemoveItemFromContents().gameObject);
            iM.HasMomento = false;
            SetPlayerIsBeingKilled(false);

            positionValue = 0;

            while (positionValue < 1f)
            {
                //Move momentoSlot To center
                positionValue += Time.deltaTime * 0.5f;
                smoothedPositionValue = Mathf.SmoothStep(1, 0, positionValue);
                pSXMaterial.SetFloat("_FadeToWhite", smoothedPositionValue);
                await Task.Yield();
            }


            iS.SlotParentTransform.position = iS.SlotParentInitialPosition;
        }
        else 
        {
            if (playerCameraAnimator != null) playerCameraAnimator.Play("Death", 0);
            while (positionValue < 1f)
            {
                positionValue += Time.deltaTime;
                smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                pSXMaterial.SetFloat("_TransitionToAlternate", smoothedPositionValue);
                await Task.Yield();
            }

            
        }

        //transform.rotation = newPlayerRotation;
    }
}
