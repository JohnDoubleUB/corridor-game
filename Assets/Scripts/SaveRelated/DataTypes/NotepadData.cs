using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable][XmlRootAttribute("NotepadData")]
public class NotepadData
{
    public NotepadLineData[] LineData;

    public NotepadData() 
    {

    }
    public NotepadData(IEnumerable<LineRenderer> lineRenderers)
    {
        LineData = lineRenderers.Where(x => x != null).Select(lineRenderer =>
        {
            Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(linePoints);
            return new NotepadLineData(linePoints.Select(x => x.Serialized()), lineRenderer.transform.localRotation.eulerAngles.Serialized(), lineRenderer.transform.localScale.Serialized());
        }).ToArray();
    }
}
