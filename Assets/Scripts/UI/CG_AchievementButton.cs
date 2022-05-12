using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CG_AchievementButton : UIBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CG_AchievementData Achievement;
    public Image AchievementImage;
    public Text NiceName;
    public Text Description;
    public float textFadeSpeed = 4f;

    private bool isHovered;

    private float currentTextTransparencyValue = 0f;
    protected override void Start()
    {
        base.Start();
        if (Achievement != null)
        {
            if (AchievementImage != null)
            {
                AchievementImage.color = Color.white;
            }

            if (NiceName != null) 
            { 
                NiceName.text = Achievement.NiceName;
                NiceName.color = new Color(NiceName.color.r, NiceName.color.g, NiceName.color.b, currentTextTransparencyValue);
            }

            if (Description != null) 
            {
                Description.text = Achievement.Description;
                Description.color = new Color(Description.color.r, Description.color.g, Description.color.b, currentTextTransparencyValue);
            }

        }
    }

    protected override void OnEnable()
    {
        if (AchievementIntegrationManager.current.IsAchieved(Achievement.Identifier))
        {
            SetAchieved();
        }
        else 
        {
            SetUnachieved();
        }
    }

    public void SetAchieved()
    {
        AchievementImage.sprite = Achievement.UnlockedImage;
    }

    public void SetUnachieved()
    {
        AchievementImage.sprite = Achievement.LockedImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AchievementImage != null) 
        { 
            AchievementImage.color = Color.grey;
            isHovered = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (AchievementImage != null) 
        { 
            AchievementImage.color = Color.white;
            isHovered = false;
        }
    }

    public void Update()
    {
        bool valueHasChanged = false;
        if (isHovered && currentTextTransparencyValue != 1f)
        {
            currentTextTransparencyValue = Mathf.Min(currentTextTransparencyValue + (Time.unscaledDeltaTime * textFadeSpeed), 1f);
            valueHasChanged = true;
        }
        else if(!isHovered && currentTextTransparencyValue != 0f)
        {
            currentTextTransparencyValue = Mathf.Max(currentTextTransparencyValue - (Time.unscaledDeltaTime * textFadeSpeed), 0f);
            valueHasChanged = true;
        }

        if (valueHasChanged) 
        {
            if (NiceName != null) NiceName.color = new Color(NiceName.color.r, NiceName.color.g, NiceName.color.b, currentTextTransparencyValue);
            if (Description != null) Description.color = new Color(Description.color.r, Description.color.g, Description.color.b, currentTextTransparencyValue);
        }
    }
}
