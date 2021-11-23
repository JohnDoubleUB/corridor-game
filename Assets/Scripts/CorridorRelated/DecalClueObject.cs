using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DecalClueObject : MonoBehaviour
{
    public int ClueNumber;

    public string clue;
    private string lastClue;

    public SpriteDecal[] decals;

    private void Awake()
    {
        DisableAllDecals();
    }

    private void Update()
    {
        UpdateDecals();
    }

    private void UpdateDecals() 
    {
        if (clue != lastClue) 
        {
            DisableAllDecals();
            for (int i = 0; i < clue.Length && i < decals.Length; i++) 
            {
                SpriteDecal currentDecalObject = decals[i];
                currentDecalObject.character = clue[i];
                currentDecalObject.gameObject.SetActive(true);
            }

            lastClue = clue;
        }
    }

    private void DisableAllDecals() 
    {
        foreach (GameObject decalObj in decals.Select(x => x.gameObject)) decalObj.SetActive(false);
    }
}
