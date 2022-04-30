using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CG_UIHandler : MonoBehaviour
{
    [SerializeField]
    private Image saveGameSymbolPrompt;
    [SerializeField]
    private Image levelChangeSymbolPrompt;
    [SerializeField]
    private Text interactionPrompt;

    [SerializeField]
    private Image playerCrosshair;
    [SerializeField]
    private Sprite crosshairNormal;
    [SerializeField]
    private Sprite crosshairInteract;


    public bool InteractionPromptVisiblity 
    {
        get 
        { 
            return interactionPrompt != null && interactionPrompt.enabled; 
        } 
        set
        {
            if (interactionPrompt != null) interactionPrompt.enabled = value;
        }
    }

    public bool CrosshairVisibilty
    {
        get
        {
            return playerCrosshair != null && playerCrosshair.enabled;
        }
        set
        {
            if (playerCrosshair != null) playerCrosshair.enabled = value;
        }
    }

    public string InteractionText 
    {
        get 
        { 
            return interactionPrompt.text; 
        }
        set 
        {
            if (interactionPrompt != null) interactionPrompt.text = value;
        }
    }

    public CrosshairType CurrentCrosshairType 
    {
        get 
        { 
            return playerCrosshair.sprite == crosshairNormal ? CrosshairType.Default : CrosshairType.Interacting; 
        }
        set 
        {
            if (value == 0)
            {
                playerCrosshair.sprite = crosshairNormal;
            }
            else
            {
                playerCrosshair.sprite = crosshairInteract;
            }
        }
    }

    public async void ShowLevelPrompt()
    {
        if (levelChangeSymbolPrompt != null)
        {
            await levelChangeSymbolPrompt.FadeMaskableGraphic(true);
            await levelChangeSymbolPrompt.FadeMaskableGraphic(false);
        }
    }

    public async void ShowSavePrompt()
    {
        if (saveGameSymbolPrompt != null)
        {
            await saveGameSymbolPrompt.FadeMaskableGraphic(true, 1.5f);
            await saveGameSymbolPrompt.FadeMaskableGraphic(false);
        }
    }

    public void SetInteractionText(string text) 
    {
        if (interactionPrompt != null) interactionPrompt.text = text;
    }

    public void SetCrosshairType(CrosshairType crossHairType) 
    {
        if (crossHairType == 0)
        {
            playerCrosshair.sprite = crosshairNormal;
        }
        else 
        {
            playerCrosshair.sprite = crosshairInteract;
        }
    }
}

public enum CrosshairType 
{
    Default,
    Interacting
}