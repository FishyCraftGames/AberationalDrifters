using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplodeOnImpact : MonoBehaviour
{
    public void Explsive()
    {
        if (Player.instance.activeCar.GetComponent<ExplodeOnImpactCarComponent>() == null)
        {
            Player.instance.activeCar.AddComponent<ExplodeOnImpactCarComponent>();
        }
    }
}
