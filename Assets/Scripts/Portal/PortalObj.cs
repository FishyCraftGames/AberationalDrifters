using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalObj : MonoBehaviour 
{ 
    void Start()
    {
        GameObject portalSpawner = GameObject.Find("PortalSpawner");
        this.transform.position = portalSpawner.transform.position;
        this.transform.rotation = portalSpawner.transform.rotation;
        Camera.main.GetComponent<Portal>().portal = this.transform;
    }
}
