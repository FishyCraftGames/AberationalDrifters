using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifySteering : MonoBehaviour
{
    public void ChangeTireWeight(float a)
    {
        Player.instance.activeCar.TireMass *= a;
    }
}
