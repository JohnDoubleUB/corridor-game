using UnityEngine;

[System.Serializable]
public struct Vector3Serialized
{
    public float x;
    public float y;
    public float z;

    public Vector3 Deserialized()
    {
        return new Vector3(x, y, z);
    }

    public Vector3Serialized(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}
