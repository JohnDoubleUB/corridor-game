using UnityEngine;

public class CorridorBoxTrigger : FakeParentScript
{
    public CorridorSection SectionToReportTo;
    public bool enableTrigger = true;
    private void OnTriggerEnter(Collider other)
    {
        if (enableTrigger && SectionToReportTo != null) SectionToReportTo.OnSectionEnter(other);
    }
}
