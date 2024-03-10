using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifySuspension : MonoBehaviour
{
    public void ModifySuspensionStregth(float a)
    {
        Player.instance.activeCar.frontStrength *= a;
        Player.instance.activeCar.backStrength *= a;
    }

    public void ModifySuspensionDampening(float a)
    {
        Player.instance.activeCar.dampening *= a;
    }
}
