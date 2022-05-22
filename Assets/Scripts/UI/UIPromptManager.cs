using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPromptManager : MonoBehaviour
{
    public static UIPromptManager current;
    public float DisplaySpeed = 2f;

    [SerializeField]
    private UIPromptObject[] UIObjects;

    private Dictionary<string, UIPromptObject> UIElements;


    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    //Value
    private void Start()
    {
        UIElements = new Dictionary<string, UIPromptObject>();

        foreach (UIPromptObject pO in UIObjects) 
        {
            pO.ImmediatelyMatchValue();
            UIElements.Add(pO.UIName, pO);
        }
    }


    private void Update()
    {
        foreach (KeyValuePair<string, UIPromptObject> pO in UIElements) 
        {
            pO.Value.Update();
        }
    }

    //Returns true if successful
    public bool SetVisibilityOfElement(bool Show, string ElementName) 
    {
        if (UIElements.ContainsKey(ElementName)) 
        {
            UIElements[ElementName].Show = Show;
            return true;
        }

        return false;
    }

    public void SetVisibilityOfElements(bool Show, params string[] ElementNames)
    {
        foreach(string ElementName in ElementNames) 
        {
            SetVisibilityOfElement(Show, ElementName);
        }
    }
}

[System.Serializable]
public class UIPromptObject
{
    public string UIName;
    public MaskableGraphic Graphic;
    public bool Show;
    public float ShowSpeed = 2f;

    public void ImmediatelyMatchValue() 
    {
        if (Graphic != null) Graphic.color = ColourWithAlpha(Graphic.color, Show ? 1 : 0);
    }

    public void Update() 
    {
        if (Show && Graphic.color.a != 1f)
        {
            Graphic.color = ColourWithAlpha(Graphic.color, Graphic.color.a + (ShowSpeed * Time.deltaTime));
        }
        else if (!Show && Graphic.color.a != 0f) 
        {
            Graphic.color = ColourWithAlpha(Graphic.color, Graphic.color.a - (ShowSpeed * Time.deltaTime));
        }
    }

    private Color ColourWithAlpha(Color color, float newAlphaValue = 0f)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp(newAlphaValue, 0f, 1f));
    }

}
