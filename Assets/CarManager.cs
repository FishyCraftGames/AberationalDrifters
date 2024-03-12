using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager instance;
    public List<Transform> waypoints = new List<Transform>();

    void Awake()
    {
        instance = this;
    }
}
