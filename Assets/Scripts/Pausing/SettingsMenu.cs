using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private bool gameSettingsSaveRequired;

    [SerializeField]
    private CG_GameSettings gameSettings;

    private void OnEnable()
    {
        gameSettingsSaveRequired = true;
    }

    private void OnApplicationQuit()
    {
        SaveGameSettings();
    }

    private void OnDisable()
    {
        SaveGameSettings();
    }

    private void SaveGameSettings() 
    {
        if (gameSettingsSaveRequired == false) return;
        gameSettings.StoreSettings();
        gameSettingsSaveRequired = false;
        print("Settings saved");
    }
}
