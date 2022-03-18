using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomNotepadStarts", menuName = "ScriptableObjects/CustomNotepadStarts", order = 1)]
public class CustomNotepadStarts : ScriptableObject
{
    public List<NotepadData> Varients = new List<NotepadData>();
}
