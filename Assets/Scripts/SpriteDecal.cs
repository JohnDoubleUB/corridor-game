using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SpriteDecal : MonoBehaviour
{
    DecalProjector decalProjector;

    public char character;

    private char lastCharacter;
    private string decalNumbers = "1234567890";

    private void Start()
    {
        decalProjector = GetComponent<DecalProjector>();
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
