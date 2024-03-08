using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalSpawner : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("Portal", LoadSceneMode.Additive);
    }
}
