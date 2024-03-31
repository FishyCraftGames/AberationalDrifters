using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwoshObj : MonoBehaviour
{
    public AudioSource a;

    void Update()
    {
        a.volume = Player.instance.swooshVolume;
    }
}
