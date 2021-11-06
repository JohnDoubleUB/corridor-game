using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CG_CharacterController : MonoBehaviour
{
    private InputMaster controls;
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public GameObject playerPencil;

    public Text interactionPrompt;

    public Animator NotepadAnimator;

    CharacterController characterController;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    [HideInInspector]
    public bool canMove = true;

    private GameObject currentInteractableGameObject;
    private InteractableObject currentInteractable;

    private GameObject notepadGameObject;
    private Notepad notepadObject;

    private int pencilLayerMask;

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
    }

    private void Interact()
    {
        if (currentInteractable != null)
        {
            currentInteractable.IntiateInteract();
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
        UpdateInteractable();

        if (!canMove) UpdateDraw();

        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButtonDown("Jump") && canMove)
            {
                moveDirection.y = jumpSpeed;
            }
        }
        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
        }


        if (Input.GetButtonDown("Cancel"))
        {
            print("End game!");
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            canMove = !canMove;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.Confined;
            Cursor.visible = false;
            NotepadAnimator.Play(canMove ? "Dequip" : "Equip");
            playerPencil.SetActive(!canMove);
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
                    interactionPrompt.text = controls.Player.Interact.GetBindingDisplayString(0) + " to " + controls.Player.Interact.name + " with " + currentInteractable.ObjectName;
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
