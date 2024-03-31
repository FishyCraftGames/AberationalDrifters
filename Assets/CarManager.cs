using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager instance;
    public List<Waypoint> waypoints = new List<Waypoint>();

    void Awake()
    {
        instance = this;
    }
}
