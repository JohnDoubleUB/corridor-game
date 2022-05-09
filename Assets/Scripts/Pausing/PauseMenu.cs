using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Color toggleOnColour;
    public Color toggleOffColour;

    public Image ToggleUI_DebugButtonImage;
    public Image ToggleVariableWalkSpeed_DebugButtonImage;
    public Image ToggleMouseAcceleration_DebugButtonImage;

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

    public void GenerateTestingLog() 
    {
        GameManager.current.ConsoleLogger.GenerateTestingLog();
    }

    public void SaveGame_Debug() 
    {
        PauseGame();
        CorridorChangeManager.current.SaveGame();
    }

    public void ToggleUI_Debug() 
    {
        GameManager.current.EnableGameUI = !GameManager.current.EnableGameUI;
        if (ToggleUI_DebugButtonImage != null) ToggleUI_DebugButtonImage.color = GameManager.current.EnableGameUI ? toggleOffColour : toggleOnColour;
    }

    public void ToggleVariableWalkSpeed_Debug() 
    {
        GameManager.current.EnableVariableWalkSpeed = !GameManager.current.EnableVariableWalkSpeed;
        if (ToggleVariableWalkSpeed_DebugButtonImage != null) ToggleVariableWalkSpeed_DebugButtonImage.color = GameManager.current.EnableVariableWalkSpeed ? toggleOnColour : toggleOffColour;
    }

    public void ToggleMouseAcceleration_Debug() 
    {
        GameManager.current.EnableMouseAcceleration = !GameManager.current.EnableMouseAcceleration;
        if (ToggleMouseAcceleration_DebugButtonImage != null) ToggleMouseAcceleration_DebugButtonImage.color = GameManager.current.EnableMouseAcceleration ? toggleOnColour : toggleOffColour;
    }

    public void GibMeAchievementPlz_Debug() 
    {
        if (Steamworks.SteamClient.IsValid) 
        {
            Steamworks.Data.Achievement Ach = Steamworks.SteamUserStats.Achievements.FirstOrDefault(x => x.Identifier == "ACH_WIN_ONE_GAME");
            Ach.Trigger();
        }
    }


    public async void DisplayIcon(Steamworks.Data.Achievement ach) 
    {
        Task<Steamworks.Data.Image?> icon = ach.GetIconAsync();
        await icon;

        if (icon != null && icon.Result != null && icon.Result.HasValue) 
        {

            Steamworks.Data.Image img = icon.Result.Value;
            Texture2D newTexture = new Texture2D((int)img.Width, (int)img.Height, TextureFormat.RGBA32, false, false);
            newTexture.LoadRawTextureData(img.Data);
            newTexture.Apply();
        }
    }

    public void GetThatAchievementAwayFromMe_Debug()
    {
        if (Steamworks.SteamClient.IsValid)
        {
            Steamworks.Data.Achievement Ach = Steamworks.SteamUserStats.Achievements.FirstOrDefault(x => x.Identifier == "ACH_WIN_ONE_GAME");
            Ach.Clear();
        }
    }
}
