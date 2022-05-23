using System.Linq;
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
    private Text momentoText;
    [SerializeField]
    private Text visibilityPrompt;

    [SerializeField]
    private Image playerCrosshair;
    [SerializeField]
    private Sprite crosshairNormal;
    [SerializeField]
    private Sprite crosshairInteract;

    [SerializeField]
    public Text[] dialogueBoxes;
    [SerializeField]
    private Image DialogueBoxBackground;

    private float dialogueBoxValue = 0;

    public Text[] DialogueBoxes 
    {
        get 
        {
            return dialogueBoxes;
        }
    }

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
            if (visibilityPrompt != null) visibilityPrompt.enabled = value;
        }
    }

    public bool PlayerVisibilityPrompt 
    {
        get
        {
            return visibilityPrompt != null && visibilityPrompt.enabled && !string.IsNullOrEmpty(visibilityPrompt.text);
        }
        set
        {
            if (visibilityPrompt != null) visibilityPrompt.text = value ? "HIDDEN" : string.Empty;
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

    public bool MomentoTextVisibility 
    {
        get 
        {
            return momentoText != null && momentoText.enabled; 
        }
        set 
        {
            if (momentoText != null) momentoText.enabled = value;
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

    private void UpdateDialogueBoxBackground()
    {
        if (dialogueBoxes.Any(x => !string.IsNullOrEmpty(x.text)) && dialogueBoxValue != 1f)
        {
            dialogueBoxValue = Mathf.Min(dialogueBoxValue + Time.deltaTime, 1f);
            DialogueBoxBackground.color = new Color(DialogueBoxBackground.color.r, DialogueBoxBackground.color.g, DialogueBoxBackground.color.b, dialogueBoxValue);
        }
        else if (dialogueBoxValue != 0f)
        {
            dialogueBoxValue = Mathf.Max(dialogueBoxValue - Time.deltaTime, 0f);
            DialogueBoxBackground.color = new Color(DialogueBoxBackground.color.r, DialogueBoxBackground.color.g, DialogueBoxBackground.color.b, dialogueBoxValue);
        }
    }

    private void Update()
    {
        UpdateDialogueBoxBackground();
    }

    private void Start()
    {
        DialogueBoxBackground.color = new Color(DialogueBoxBackground.color.r, DialogueBoxBackground.color.g, DialogueBoxBackground.color.b, 0);
    }
}

public enum CrosshairType 
{
    Default,
    Interacting
}