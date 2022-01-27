﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CG_CharacterController : MonoBehaviour
{
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public bool NotepadPickedUp; //Controls if the notepad can actually be used (if the player has grabbed it in the level)
    public Text[] DialogueBoxes;

    public bool IsJumping { get { return characterIsJumping; } }
    public bool IsIlluminated { get { return isIlluminated; } }


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
    private Notepad notepadObject;

    private int pencilLayerMask;
    private bool characterIsJumping;

    private List<InteractableCandle> candlesInRangeOfPlayer = new List<InteractableCandle>();
    public bool isIlluminated;
    

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
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isIlluminated = candlesInRangeOfPlayer.Any(x => x.IsIlluminatingPlayer);
        
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

            if (characterIsJumping) 
            {
                characterIsJumping = false;
                if (playerLandSounds != null && playerLandSounds.Any()) footStepPosition.PlayClipAtTransform(playerLandSounds[Random.Range(0, playerLandSounds.Length)], false, 0.2f);
            }

            if (Input.GetButtonDown("Jump") && playerNotBusy/*canMove*/)
            {
                moveDirection.y = jumpSpeed;
                characterIsJumping = true;
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
            canMove = !canMove;
            canInteract = !canInteract;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.Confined;
            Cursor.visible = false;
            NotepadAnimator.Play(canMove ? "Dequip" : "Equip");
            playerPencil.SetActive(!canMove);
            playerCrosshair.enabled = canMove;
        }


        if (Input.GetButtonDown("Cancel"))
        {
            print("End game!");
            Application.Quit();
        }
    }

    private void UpdateInteractable(bool showDebug = false)
    {
        if (playerCamera != null)
        {
            RaycastHit lookedAtObject;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out lookedAtObject, 2f, LayerMask.GetMask("Interactables")))
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
                    if(notepadObject == null) notepadObject = inViewPencilLayerObj.GetComponent<Notepad>();

                    if (notepadObject != null) 
                    { 
                        notepadObject.PencilOverPadArea = !nonWritingArea; 
                        if(nonWritingArea) notepadObject.ClearCurrentLine();
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
}
