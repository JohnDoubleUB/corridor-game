using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NumberpadController : PuzzleElementController
{
    public string password = "12345";
    public Text DisplayText;
    public char passwordGapCharacter = '_';

    public AudioClip buttonPressSound;
    public AudioClip numberPadCorrectSound;
    public AudioClip numberPadIncorrectSound;
    public AudioClip incorrectSound;

    public ButtonInteractable[] Buttons;
    public GameObject keyPadInteractor;

    public ButtonInteractable inertKey;

    public List<char> disabledButtons = new List<char>();
    private char[] lastDisabledButtons = new char[0];

    private string blankPassword;
    private string currentGuessCharacters = "";
    private bool checkingPassword;

    private bool placingKey;
    private char keyToPlace;
    private Transform keyToPlaceTarget;
    private float positionValue;

    private IEnumerator ClearEnteredCodeAfterDelay(float waitTime = 1f) 
    {
        yield return new WaitForSeconds(waitTime);
        DisplayText.text = blankPassword;
        currentGuessCharacters = "";
        checkingPassword = false;
    }

    public override void Notify(PuzzleElementNotifier notifier = null)
    {
        if (!placingKey) 
        {
            // Check player inventory for key
            InventorySlot key = InventoryManager.current.inventorySlots.Where(x => x.SlotOccupied && disabledButtons.Contains(x.slotContent.ObjectName[0])).FirstOrDefault();
            
            //Check that a key was found
            if (key != null) 
            {
                inertKey.buttonMesh.enabled = true;
                inertKey.buttonText.enabled = true;
                //Store key value
                keyToPlace = key.slotContent.ObjectName[0];
                keyToPlaceTarget = Buttons.First(x => x.ObjectName[0] == keyToPlace).transform;
                //Destroy inventory object
                Destroy(key.RemoveItemFromContents().gameObject);
                positionValue = 0f;
                placingKey = true;
                inertKey.ObjectName = keyToPlace.ToString();
            }
        }
    }

    private IEnumerator VerifyEnteredCodeAfterDelay(float waitTime = 1f) 
    {
        checkingPassword = true;

        yield return new WaitForSeconds(waitTime);
        if (currentGuessCharacters == password)
        {
            PuzzleSolved = true;
            checkingPassword = false;
            AudioManager.current.PlayClipAt(numberPadCorrectSound, transform.position, 0.5f, false);
        }
        else 
        {
            DisplayText.text = "Access Denied";
            AudioManager.current.PlayClipAt(numberPadIncorrectSound, transform.position, 0.5f, false);
            AudioManager.current.PlayClipAt(incorrectSound, GameManager.current.player.transform.position, 1f, false);
            StartCoroutine(ClearEnteredCodeAfterDelay());
        }
    }

    private void Awake()
    {
        UpdateBlankPassword();
        //UpdateDisabledButtons();
    }

    private void Update()
    {
        if (blankPassword.Length != password.Length || blankPassword[0] != passwordGapCharacter) UpdateBlankPassword();
        if (PuzzleSolved && DisplayText.text != "Access Granted") DisplayText.text = "Access Granted";
        UpdateKeyPlaceAnimation();
        UpdateDisabledButtons();
    }

    private void UpdateKeyPlaceAnimation() 
    {
        if (placingKey) 
        {
            if (positionValue < 0.8f)
            {

                positionValue += Time.deltaTime * 4f;
                float smoothedPositionValue = Mathf.SmoothStep(0, 1, positionValue);
                Vector3 cameraPosition = GameManager.current.playerController.playerCamera.transform.position - new Vector3(0, 0.6f, 0);
                inertKey.transform.position = Vector3.Lerp(cameraPosition, keyToPlaceTarget.position, smoothedPositionValue);
                inertKey.transform.rotation = Quaternion.RotateTowards(GameManager.current.player.transform.rotation, keyToPlaceTarget.rotation, smoothedPositionValue * 50f);
            }
            else 
            {
                disabledButtons.Remove(keyToPlace);
                placingKey = false;
                inertKey.buttonMesh.enabled = false;
                inertKey.buttonText.enabled = false;
                if(LayoutHandler != null) OnPuzzleUpdated();
            }
        }
    }

    private void UpdateDisabledButtons() 
    {
        if (!Enumerable.SequenceEqual(disabledButtons.OrderBy(e => e), lastDisabledButtons.OrderBy(e => e))) 
        {
            print("update!");
            //Enable the interactor if a key is disabled
            if (keyPadInteractor != null) keyPadInteractor.SetActive(disabledButtons.Any());

            //set various elements as active or not
            foreach (ButtonInteractable button in Buttons) button.gameObject.SetActive(!disabledButtons.Contains(button.ObjectName[0]));
            
            //update the last disabled buttons check
            lastDisabledButtons = disabledButtons.ToArray();
        }
    }

    private void UpdateBlankPassword() 
    {
        blankPassword = new string(passwordGapCharacter, password.Length);
        if (DisplayText != null) DisplayText.text = blankPassword;
    }

    public void InputCharacter(char character, Transform inputLocation = null)
    {
        if (!checkingPassword && !PuzzleSolved)
        {
            switch (character)
            {
                case 'C':
                case 'c':
                    //Clear the current guess
                    currentGuessCharacters = "";
                    break;
                case '<':
                    //Backspace the last character
                    if (currentGuessCharacters.Length > 0) currentGuessCharacters = currentGuessCharacters.Remove(currentGuessCharacters.Length - 1);
                    break;
                default:
                    //All other cases
                    if (currentGuessCharacters.Length < password.Length) currentGuessCharacters += character;
                    break;
            }

            if (currentGuessCharacters.Length == password.Length)
            {
                StartCoroutine(VerifyEnteredCodeAfterDelay());
            }

            UpdateDisplay();
            PlayPressSoundAtLocation(inputLocation);
        }

    }

    private void UpdateDisplay()
    {
        if (DisplayText != null)
        {
            string result = "";
            for (int i = 0; i < password.Length; i++) result += i < currentGuessCharacters.Length ? currentGuessCharacters[i] : passwordGapCharacter;
            DisplayText.text = !string.IsNullOrEmpty(result) ? result : blankPassword;
        }
    }

    private void PlayPressSoundAtLocation(Transform locationTransform = null) 
    {
        if (buttonPressSound != null) AudioManager.current.PlayClipAt(buttonPressSound, locationTransform != null ? locationTransform.position : transform.position, 0.5f, true);
    }

    public override void LoadPuzzleData(PuzzleElementControllerData puzzleData)
    {
        NumberpadControllerData numberpadData = puzzleData as NumberpadControllerData;
        if (numberpadData != null) 
        {
            disabledButtons = numberpadData.DisabledButtons.ToList();
        }

        base.LoadPuzzleData(puzzleData);
    }

    public override void OnPuzzleUpdated()
    {
        LayoutHandler.UpdatePuzzleData(this, (NumberpadControllerData)this);
    }
}

public class NumberpadControllerData : PuzzleElementControllerData
{
    public char[] DisabledButtons;

    public NumberpadControllerData(NumberpadController numberpadController) : base(numberpadController)
    {
        DisabledButtons = numberpadController.disabledButtons.ToArray();
    }

    public static implicit operator NumberpadControllerData(NumberpadController numberpadController)
    {
        return new NumberpadControllerData(numberpadController);
    }
}