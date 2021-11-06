using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class CorridorLayoutHandler : MonoBehaviour
{
    public PropScript[] Props;
    private bool layoutIntitated;
    private void Awake()
    {
        Props = GetComponentsInChildren<PropScript>();
        InitiateLayout();
    }

    public void EnsurePositiveScaleValues(Vector3 parentScaleValues) 
    {
        //transform.localScale = new Vector3(parentScaleValues.x > 0 ? 1 : -1, parentScaleValues.y > 0 ? 1 : -1, parentScaleValues.z > 0 ? 1 : -1);
    }

    public void InitiateLayout() 
    {
        if (!layoutIntitated)
        {
            foreach (PropScript prop in Props)
            {
                GameObject trueChild = new GameObject(prop.name + "_TrueChild");
                trueChild.transform.SetParent(transform);
                trueChild.transform.position = prop.transform.position;
                prop.name = prop.name + "_FakeChild";
                prop.transform.SetParent(null);
                prop.FakeParent = trueChild.transform;
                prop.transform.localScale = Vector3.one; //Ensure meshes aren't reversed
            }
            layoutIntitated = true;
        }
    }
}
