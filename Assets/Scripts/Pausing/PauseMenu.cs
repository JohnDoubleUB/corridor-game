using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void PauseGame()
    {
        GameManager.current.TogglePauseGame();
    }

    public void QuitGame() 
    {
        print("End game!");
        Application.Quit();
    }

    public void RevertToLastCheckpoint() 
    {
        GameManager.current.RestartCurrentScene();
    }

    public void StartNewGame() 
    {
        SaveSystem.LoadType = GameLoadType.New;
        SaveSystem.NotepadLoadType = GameLoadType.New;
        PauseGame();
        RevertToLastCheckpoint();
    }

    public void SaveGame_Debug() 
    {
        PauseGame();
        CorridorChangeManager.current.SaveGame();
    }
}
