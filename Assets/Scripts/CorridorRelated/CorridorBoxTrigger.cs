using UnityEngine;

public class CorridorBoxTrigger : FakeParentScript
{
    public CorridorSection SectionToReportTo;
    private void OnTriggerEnter(Collider other)
    {
        if (SectionToReportTo != null) SectionToReportTo.OnSectionEnter(other);
    }
}
