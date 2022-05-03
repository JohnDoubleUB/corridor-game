using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NumberpadEasterEggController : ButtonRecievingPuzzleController
{
    public string EventTag;
    public int passwordLength;
    public EasterEggNumberpadPassword[] PossiblePasswords;
    public Text DisplayText;
    public char passwordGapCharacter = '_';

    public AudioClip buttonPressSound;
    public AudioClip numberPadCorrectSound;
    public AudioClip numberPadIncorrectSound;
    public AudioClip incorrectSound;

    public ButtonInteractable[] Buttons;

    public MeshRenderer numberpadMeshRenderer;
    private Material numberpadMat;

    private string blankPassword;
    private string currentGuessCharacters = "";
    private bool checkingPassword;

    private IEnumerator ClearEnteredCodeAfterDelay(float waitTime = 1f)
    {
        yield return new WaitForSeconds(waitTime);
        DisplayText.text = blankPassword;
        currentGuessCharacters = "";
        checkingPassword = false;
    }

    private bool FindMatchingPassword(string guess, out EasterEggNumberpadPassword MatchingPassword)
    {
        if (PossiblePasswords != null && PossiblePasswords.Any(x => x.password == guess))
        {
            MatchingPassword = PossiblePasswords.First(x => x.password == guess);
            return true;
        }
        MatchingPassword = new EasterEggNumberpadPassword();
        return false;
    }

    private IEnumerator VerifyEnteredCodeAfterDelay(float waitTime = 1f)
    {
        checkingPassword = true;

        yield return new WaitForSeconds(waitTime);

        //if (currentGuessCharacters == password)
        if (FindMatchingPassword(currentGuessCharacters, out EasterEggNumberpadPassword matchingPassword))
        {
            PuzzleSolved = true;
            checkingPassword = false;

            transform.PlayClipAtTransform(numberPadCorrectSound, true, 0.5f, false);
            
            //Add tag
            CorridorChangeManager.current.AddEventTag(EventTag);

            //Change level
            print("Password matches! Going to level " + matchingPassword.levelChange);
            
            CorridorChangeManager.current.LevelChange(matchingPassword.levelChange);
        }
        else
        {
            DisplayText.text = "Invalid Code";
            transform.PlayClipAtTransform(numberPadIncorrectSound, true, 0.5f, false);
            GameManager.current.player.transform.PlayClipAtTransform(incorrectSound, true, 1f, false);
            StartCoroutine(ClearEnteredCodeAfterDelay());
        }
    }

    private void Awake()
    {
        UpdateBlankPassword();
    }

    private void Update()
    {
        if (blankPassword.Length != passwordLength || blankPassword[0] != passwordGapCharacter) UpdateBlankPassword();
        if (PuzzleSolved && DisplayText.text != "Code Accepted") DisplayText.text = "Code Accepted";
    }

    private void Start()
    {
        PuzzleSolved = CorridorChangeManager.current.EventTagPresent(EventTag); //Check if the panel has already been used
        numberpadMat = numberpadMeshRenderer.sharedMaterials[0];
        MaterialManager.current.TrackMaterials(numberpadMat);
    }

    private void OnDestroy()
    {
        MaterialManager.current.UntrackMaterials(numberpadMat);
    }

    private void UpdateBlankPassword()
    {
        blankPassword = new string(passwordGapCharacter, passwordLength);
        if (DisplayText != null) DisplayText.text = blankPassword;
    }

    public override void InputCharacter(char character, Transform inputLocation = null)
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
                    if (currentGuessCharacters.Length < passwordLength) currentGuessCharacters += character;
                    break;
            }

            if (currentGuessCharacters.Length == passwordLength)
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
            for (int i = 0; i < passwordLength; i++) result += i < currentGuessCharacters.Length ? currentGuessCharacters[i] : passwordGapCharacter;
            DisplayText.text = !string.IsNullOrEmpty(result) ? result : blankPassword;
        }
    }

    private void PlayPressSoundAtLocation(Transform locationTransform = null)
    {
        if (buttonPressSound != null) (locationTransform != null ? locationTransform : transform).PlayClipAtTransform(buttonPressSound, true, 0.5f);
    }

    protected bool TriggerEvent()
    {
        return CorridorChangeManager.current.AddEventTag(EventTag);
    }

}

[System.Serializable]
public struct EasterEggNumberpadPassword
{
    public string password;
    public int levelChange;
}