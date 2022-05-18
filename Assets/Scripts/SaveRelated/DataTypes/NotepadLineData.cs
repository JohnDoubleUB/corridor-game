using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct NotepadLineData
{
    public Vector3Serialized[] Positions;
    public Vector3Serialized LocalScale;
    public Vector3Serialized LocalRotationEuler;

    public NotepadLineData(IEnumerable<Vector3Serialized> Positions, Vector3Serialized LocalRotationEuler, Vector3Serialized LocalScale)
    {
        this.Positions = Positions.ToArray();
        this.LocalRotationEuler = LocalRotationEuler;
        this.LocalScale = LocalScale;
    }
}
