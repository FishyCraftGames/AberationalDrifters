using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rain : MonoBehaviour
{
    void Update()
    {
        transform.position = Player.instance.transform.position;
        transform.rotation = Player.instance.transform.rotation;
    }
}
