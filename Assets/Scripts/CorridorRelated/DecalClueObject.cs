using System.Linq;
using UnityEngine;

public class DecalClueObject : MonoBehaviour
{
    public int ClueLevel = -1; //If this is different from -1 then we know it is a clue linked to a different level
    public int ClueNumber;

    public string clue;
    private string lastClue;

    public SpriteDecal[] decals;
    public bool IsForCurrentLevel { get { return ClueLevel == -1; } }

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
