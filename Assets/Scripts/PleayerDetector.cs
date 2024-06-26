using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PleayerDetector : MonoBehaviour
{

    public Transform spawnPoint;
    public GameObject car;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Instantiate(car, spawnPoint.position, spawnPoint.rotation);
            Destroy(gameObject);
        }
    }
}
