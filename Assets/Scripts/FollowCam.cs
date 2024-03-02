using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{

    public Transform target;
    public float speed = 0.25f;
    public float MinDist;

    void FixedUpdate()
    {
        Physics.Raycast(target.position + Vector3.up * 500f, Vector3.down, out RaycastHit TargetRay);
        float offset = (target.position + Vector3.up * 2.4f - TargetRay.point).y;
        if (offset > MinDist)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + Vector3.up * 2.4f, speed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, TargetRay.point + Vector3.up * MinDist, speed);
        }

        Vector3 speedDir = target.root.GetComponent<Rigidbody>().velocity;
        if (speedDir.magnitude > 3)
            speedDir = speedDir.normalized * 3;
        transform.LookAt(target.root.position + speedDir);
    }
}
