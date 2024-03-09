using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreasedTopSpeed : MonoBehaviour
{
    public void Speed(float a)
    {
        Player.instance.activeCar.carTopSpeed *= a;
    }

    public void Acceleration(float a)
    {
        Player.instance.activeCar.accMultiplier *= a;
    }
}
