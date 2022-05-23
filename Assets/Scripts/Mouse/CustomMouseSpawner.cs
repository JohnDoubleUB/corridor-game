using System.Collections;
using UnityEngine;

public class CustomMouseSpawner : MonoBehaviour
{
    public MouseEntity MouseToSpawn;

    private void Start()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay(float delayInSeconds = 1.5f) 
    {
        yield return new WaitForSeconds(delayInSeconds);
        if (MouseToSpawn != null)
        {
            MouseEntity tempEntity = Instantiate(MouseToSpawn, transform.position, transform.rotation, transform);
            CorridorChangeManager.current.RegisterHuntableEntity(tempEntity as IHuntableEntity);
        }
    }

}
