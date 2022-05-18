[System.Serializable]
public class TVManData
{
    public bool MomentoDelayActive;
    public float CurrentMomentoDelayTimer;

    public TVManData() { }

    public TVManData(TVManController tvManController)
    {
        MomentoDelayActive = tvManController.MomentoEffectActive;
        CurrentMomentoDelayTimer = tvManController.CurrentMomentoDelayTimer;
    }
}
