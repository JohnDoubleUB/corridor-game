using UnityEngine.UI;

public class ButtonPickup : PickupableInteractable
{
    public Text buttonText;

    protected new void Awake()
    {
        base.Awake();
        UpdateButtonText();
    }

    protected new void Update()
    {
        base.Update();
        UpdateButtonText();
    }

    private void UpdateButtonText() 
    {
        if (buttonText != null && buttonText.text != ObjectName)
        {
            buttonText.text = ObjectName;
        }
    }
}