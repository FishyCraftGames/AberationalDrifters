using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnImpactCarComponent : MonoBehaviour
{
    public float cooldown = 5f;

    private void Update()
    {
        cooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player" && cooldown < 0)
            Player.instance.Explode();
    }
}
