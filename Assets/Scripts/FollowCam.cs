using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FollowCam : MonoBehaviour
{

    public Transform target;
    public float speed = 0.25f;
    public float MinDist;
    public Transform cam;

    public AudioSource wind;

    public float screenshakePosStrength = 0.3f;
    public float screenshakeRotStrength;

    public GameObject volume;
    public LayerMask mask;

    private Vector3 lastTarget;

    void FixedUpdate()
    {
        Physics.Raycast(target.position + Vector3.up * 500f, Vector3.down, out RaycastHit TargetRay, 999f, mask);
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
        transform.LookAt(Vector3.Lerp(lastTarget, target.root.position + speedDir, 0.25f));
        lastTarget = target.root.position + speedDir;

        //post-processing
        volume.GetComponent<UnityEngine.Rendering.Volume>().weight = target.root.GetComponent<Rigidbody>().velocity.magnitude / 260;

        //camera shake
        if (target.root.GetComponent<Player>().activeCar.isGrounded)
        {
            Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * screenshakePosStrength * (target.root.GetComponent<Rigidbody>().velocity.magnitude / 260);
            randomPos.z = 0;
            cam.transform.localPosition = randomPos;

            Vector3 randomRot = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * screenshakeRotStrength * (target.root.GetComponent<Rigidbody>().velocity.magnitude / 260);
            cam.localEulerAngles = randomRot;
        }

        wind.volume = Mathf.Clamp(target.root.GetComponent<Rigidbody>().velocity.magnitude / 80, 0, 1);
    }
}
