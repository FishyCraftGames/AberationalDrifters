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

    public AudioSource audio;

    private DepthOfField depthOfField;

    int hasEntered = 1;

    public GameObject sun;

    private void Start()
    {
        volume.profile.TryGet(out depthOfField);
    }

    private void Update()
    {
        if (portal != null)
        {
            float truedistance = Mathf.Abs(Vector3.Distance(transform.position, portal.position));
            float factor = 1 - Mathf.Clamp(100 / distance * truedistance, 0, 100) / 100;
            volume.weight = factor;
            audio.volume = factor * hasEntered;
            if (factor < 1)
            {
                depthOfField.focusDistance.value = distance;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Portal")
        {
            obj.SetActive(true);
            Player.instance.activeCar.GetComponent<Rigidbody>().useGravity = false;
            Player.instance.activeCar.GetComponent<Rigidbody>().freezeRotation = true;
            hasEntered = 0;
            Player.instance.unloadScene();
            sun.SetActive(true);
        }
    }
}
