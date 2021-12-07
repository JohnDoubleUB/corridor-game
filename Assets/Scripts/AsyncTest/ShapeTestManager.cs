using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShapeTestManager : MonoBehaviour
{
    [SerializeField] private ShapeTest[] _shapes;

    private void Start()
    {
        BeginTest();
        print("lol lol lol");
    }

    //public void BeginTest() 
    //{
    //    for (var i = 0; i < _shapes.Length; i++) 
    //    {
    //        StartCoroutine(_shapes[i].RotateForSeconds(1 + 1 * i));
    //    }
    //}

    public async void BeginTest()
    {
        print("Start spinny");

        var tasks = new Task[_shapes.Length];

        for (var i = 0; i < _shapes.Length; i++)
        {
            tasks[i] = _shapes[i].RotateForSeconds(1 + 1 * i);
        }

        await Task.WhenAll(tasks);

        print("end spinnny");
    }
}
