using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripModifier : MonoBehaviour
{
    public void ModifyGrip(float a)
    {
        Player.instance.activeCar.gripMultiplyer *= a;
    }
}
