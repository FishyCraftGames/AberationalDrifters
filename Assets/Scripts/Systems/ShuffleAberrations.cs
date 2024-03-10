using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShuffleAberrations : MonoBehaviour
{

    public List<GameObject> AberrationCards = new List<GameObject>();

    public List<Transform> SpanwPoints = new List<Transform>();

    public void Shuffle()
    {
        foreach(Transform t in SpanwPoints)
        {
            GameObject obj = AberrationCards[Random.Range(0, AberrationCards.Count - 1)];
            GameObject objSelectede = Instantiate(obj, t.position, t.rotation, t);
            EventSystem.current.SetSelectedGameObject(objSelectede.transform.GetChild(0).gameObject);
        }
    }

    public void Disable()
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (Transform t in SpanwPoints)
        {
            Destroy(t.GetChild(0).gameObject);
        }
    }
}
