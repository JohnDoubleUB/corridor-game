using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NumberpadController : PuzzleElementController
{
    public string password = "12345";
    public Text DisplayText;
    public char passwordGapCharacter = '_';

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

    private IEnumerator VerifyEnteredCodeAfterDelay(float waitTime = 1f) 
    {
        checkingPassword = true;

        yield return new WaitForSeconds(waitTime);
        if (currentGuessCharacters == password)
        {
            DisplayText.text = "Access Granted";
            PuzzleSolved = true;
        }
        else 
        {
            DisplayText.text = "Access Denied";
            StartCoroutine(ClearEnteredCodeAfterDelay());
        }
    }

    private void Awake()
    {
        UpdateBlankPassword();
    }

    private void Update()
    {
        if (blankPassword.Length != password.Length || blankPassword[0] != passwordGapCharacter) UpdateBlankPassword();
    }

    private void UpdateBlankPassword() 
    {
        blankPassword = new string(passwordGapCharacter, password.Length);
        if (DisplayText != null) DisplayText.text = blankPassword;
    }

    public void InputCharacter(char character)
    {
        if (!checkingPassword)
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
}
