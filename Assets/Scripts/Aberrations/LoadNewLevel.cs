using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadNewLevel : MonoBehaviour
{
    public void Continue()
    {
        Camera.main.GetComponent<Portal>().LoadNewLevel();
    }
}
