using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SpriteDecal : MonoBehaviour
{
    private DecalProjector decalProjector;

    public char character;

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
}
