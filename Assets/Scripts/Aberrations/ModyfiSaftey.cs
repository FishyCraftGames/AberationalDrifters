using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModyfiSaftey : MonoBehaviour
{
    public void ModifyBreakForce(float a)
    {
        Player.instance.transform.GetComponent<FixedJoint>().breakForce *= a;
        Player.instance.transform.GetComponent<FixedJoint>().breakTorque *= a;
    }
}
