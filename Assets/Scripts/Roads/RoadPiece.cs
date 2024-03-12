using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    public float turn;
    public Transform inDir;
    public Transform outDir;
    public List<Transform> waypoint = new List<Transform>();
}
