using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Portal : MonoBehaviour
{
    public GameObject obj;

    public Transform portal;
    public Volume volume;
    public float distance;

    private DepthOfField depthOfField;

    private void Start()
    {
        volume.profile.TryGet(out depthOfField);
    }

    private void Update()
    {
        float truedistance = Mathf.Abs(Vector3.Distance(transform.position, portal.position));
        float factor = 1 - Mathf.Clamp(100 / distance * truedistance, 0, 100)/100;
        volume.weight = factor;
        if(factor < 1)
        {
            depthOfField.focusDistance.value = distance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Portal")
        {
            obj.SetActive(true);
        }
    }
}
