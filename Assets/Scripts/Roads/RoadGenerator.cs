using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public int roadLength;
    public List<RoadPieceItem> roadPieces;
    private float rotation;
    private Transform lastExit;

    void Start()
    {
        lastExit = transform;

        for(int i = 0; i < roadLength; i++)
        {
            while(true)
            {
                RoadPieceItem newPiece = roadPieces[UnityEngine.Random.Range(0, roadPieces.Count)];
                if(rotation + newPiece.angle < 180 && rotation + newPiece.angle > -180)
                {
                    SpawnNewPiece(newPiece);
                    break;
                }
            }
        }
    }

    void SpawnNewPiece(RoadPieceItem piece)
    {
        GameObject newPiece = Instantiate(piece.variants[UnityEngine.Random.Range(0, piece.variants.Count)]);
        RoadPiece pData = newPiece.GetComponent<RoadPiece>();
        
        var rotDiff = lastExit.rotation * Quaternion.Inverse(pData.inDir.rotation);
        newPiece.transform.rotation = rotDiff * newPiece.transform.rotation;

        var posDiff = lastExit.position - pData.inDir.position;
        newPiece.transform.position += posDiff;

        foreach(Transform t in pData.waypoint) {
            CarManager.instance.waypoints.Add(t);
        }

        lastExit = pData.outDir;
    }
}

[Serializable]
public class RoadPieceItem
{
    public float angle;
    public List<GameObject> variants = new List<GameObject> ();
}