using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SpriteDecalImage : MonoBehaviour
{
    private DecalProjector decalProjector;

    public char character;
    public DecalProjector DecalProjector { get { return decalProjector; } }


    private char lastCharacter;
    private string decalNumbers = "1234567890";

    private void Start()
    {
        decalProjector = GetComponent<DecalProjector>();
        
        //Instance the material because for some reason it doesn't do this automatically?
        decalProjector.material = new Material(decalProjector.material);
    }

    private void Update()
    {
        UpdateDecal();
    }


    private void UpdateDecal() 
    {
        if (lastCharacter != character)
        {
            Texture newTexture = null;
            if (char.IsDigit(character))
            {
                newTexture = GameManager.current.DecalNumberTextures[decalNumbers.IndexOf(character)];
            }
            decalProjector.material.SetTexture("_MainTex", newTexture);
            lastCharacter = character;
        }
    }

    private void OnDestroy()
    {
        //print("I'm being destroyed, but don't worry, the material I have won't stick around! (Delete this message later)");
        Destroy(decalProjector.material);
    }
}
