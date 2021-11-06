using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : MonoBehaviour
{
    public Transform FakeParent;

    // Update is called once per frame
    void Update()
    {
        if (FakeParent != null && FakeParent.position != transform.position) transform.position = FakeParent.position;
    }
}
