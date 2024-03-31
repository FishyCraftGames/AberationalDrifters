using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    public float turn;
    public Transform inDir;
    public Transform outDir;
    public List<Waypoint> waypoints = new List<Waypoint>();
}

[Serializable]
public class Waypoint
{
    public Transform p;
    public float speedFactor = 1;
}
