using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoBreaks : MonoBehaviour
{
    public void RemoveBreaks()
    {
        Player.instance.activeCar.noHandbreak = true;
    }
}
