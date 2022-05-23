public class SignificantObjectController : PuzzleElementController
{
    public InteractableObject SignificantInteractable;
    public bool RemoveObjectIfPuzzleSolvedWhenLoaded;


    private void Start()
    {
        if (SignificantInteractable != null) SignificantInteractable.puzzleObjectToNotifyOnInteract = this;
    }
    public void RegisterInteract() 
    {
        if(!PuzzleSolved) PuzzleSolved = true;
    }

    public override void LoadPuzzleData(PuzzleElementControllerData puzzleData)
    {
        base.LoadPuzzleData(puzzleData);
        if (PuzzleSolved && RemoveObjectIfPuzzleSolvedWhenLoaded && SignificantInteractable != null) Destroy(SignificantInteractable.gameObject);
    }
}
