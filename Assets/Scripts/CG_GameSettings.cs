﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CG_GameSettings : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Slider SoundVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider LookSensitivitySlider;

    //ResolutionSettings
    public List<string> resolutionStrings;
    public int currentResolutionIndex;
    Resolution[] resolutions;

    private string sVolumeSliderPref = "CG_FSoundVolume";
    private string mVolumeSliderPref = "CG_FMusicVolume";
    private string lookSensitivityPref = "CG_FLookSensitivity";

    private void Start()
    {
        IntializeResolutionSetting();
        InitializeFullscreenSetting();
        InitializeSoundAndMusicVolumeSetting();
        InitializeLookSensitivitySetting();
    }

    private void IntializeResolutionSetting() 
    {
        resolutions = Screen.resolutions;
        resolutionStrings = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string newResolutionText = resolutions[i].width + " x " + resolutions[i].height + " ~ " + resolutions[i].refreshRate + "Hz";
            resolutionStrings.Add(newResolutionText);
            if (resolutions[i].height == Screen.currentResolution.height && resolutions[i].width == Screen.width && resolutions[i].refreshRate == Screen.currentResolution.refreshRate) currentResolutionIndex = i;
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionStrings);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    private void InitializeFullscreenSetting() 
    {
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void InitializeSoundAndMusicVolumeSetting() 
    {
        if (PlayerPrefs.HasKey(sVolumeSliderPref)) 
        {
            float soundVol = PlayerPrefs.GetFloat(sVolumeSliderPref);
            if (SoundVolumeSlider != null) SoundVolumeSlider.value = soundVol;
            SetSoundVolume(soundVol);
        }

        if (PlayerPrefs.HasKey(mVolumeSliderPref)) 
        {
            float musicVol = PlayerPrefs.GetFloat(mVolumeSliderPref);
            if (MusicVolumeSlider != null) MusicVolumeSlider.value = musicVol;
            SetMusicVolume(musicVol);
        }
    }

    private void InitializeLookSensitivitySetting() 
    {
        if (PlayerPrefs.HasKey(lookSensitivityPref)) 
        {
            float sensitivity = PlayerPrefs.GetFloat(lookSensitivityPref);
            if (LookSensitivitySlider != null) LookSensitivitySlider.value = sensitivity;
            SetLookSensitivity(sensitivity);
        }
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.current.SetMasterMusicVolume(volume);
    }

    public void SetSoundVolume(float volume)
    {
        AudioManager.current.SoundVolumeMultiplier = volume;
    }

    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution newResolution = resolutions[resolutionIndex];

        if (newResolution.height != Screen.currentResolution.height || newResolution.width != Screen.currentResolution.width)
        {
            Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
        }
    }

    public void SetLookSensitivity(float sensitivity) 
    {
        GameManager.current.playerController.lookSpeed = sensitivity;
    }

    private void OnApplicationQuit()
    {
        StoreSettings();
    }

    private void OnDisable()
    {
        StoreSettings();
    }

    private void StoreSettings() 
    {
        if (SoundVolumeSlider != null) PlayerPrefs.SetFloat(sVolumeSliderPref, SoundVolumeSlider.value);
        if (MusicVolumeSlider != null) PlayerPrefs.SetFloat(mVolumeSliderPref, MusicVolumeSlider.value);
        if (LookSensitivitySlider != null) PlayerPrefs.SetFloat(lookSensitivityPref, LookSensitivitySlider.value);
    }

    //Continue this tutorial https://youtu.be/YOaYQrN1oYQ?t=604
}
