using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public GameObject obj;
    public FollowCam cam;

    public Transform portal;
    public Volume volume;
    public float distance;

    public AudioSource audio;

    private DepthOfField depthOfField;

    int hasEntered = 1;

    public GameObject sun;

    bool inSpace = false;
    bool lockView = false;
    Vector3 entrySpeed;

    public Animator anim;

    public Animator AntiPortal;

    Scene portalScene;

    private void Start()
    {
        volume.profile.TryGet(out depthOfField);
    }

    private void Update()
    {
        if (portal != null && !inSpace)
        {
            float truedistance = Mathf.Abs(Vector3.Distance(transform.position, portal.position));
            float factor = 1 - Mathf.Clamp(100 / distance * truedistance, 0, 100) / 100;
            volume.weight = factor;
            audio.volume = factor;
            if (factor < 1)
            {
                depthOfField.focusDistance.value = distance;
            }
        }
        else
        {
            audio.volume = 0f;
        }

        if (inSpace)
        {
            volume.weight -= volume.weight * Time.deltaTime;

            Transform car = Player.instance.activeCar.transform;
            car.GetComponent<Rigidbody>().velocity -= car.GetComponent<Rigidbody>().velocity * Time.deltaTime;

            car.rotation = Quaternion.Lerp(car.rotation, Quaternion.Euler(0f, 0f, 0f), 0.5f * Time.deltaTime);

            if (lockView)
            {
                car.eulerAngles = Vector3.zero;

                Rigidbody carRB = car.GetComponent<Rigidbody>();

                if (carRB.velocity.magnitude < entrySpeed.magnitude*2 + 5)
                {
                    carRB.velocity =  new Vector3(carRB.velocity.x, 0, carRB.velocity.z) + Vector3.forward * entrySpeed.magnitude * Time.deltaTime;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Portal")
        {
            obj.SetActive(true);
            Rigidbody carRB = Player.instance.activeCar.GetComponent<Rigidbody>();
            carRB.useGravity = false;
            carRB.freezeRotation = true;
            entrySpeed = carRB.velocity;
            hasEntered = 0;
            Player.instance.unloadScene();
            sun.SetActive(true);
            inSpace = true;
            //LoadNewLevel();
            anim.SetTrigger("In");
        }
        else if(other.gameObject.tag == "AntiPortal")
        {
            obj.SetActive(false);
            inSpace = false;
            lockView = false;
            Rigidbody carRB = Player.instance.activeCar.GetComponent<Rigidbody>();
            carRB.useGravity = true;
            carRB.freezeRotation = false;
            hasEntered = 1;

            AntiPortal.SetTrigger("Out");

            SceneManager.UnloadSceneAsync(portalScene);
        }
    }

    public void LoadNewLevel()
    {
        Debug.LogError("Button Pressed");

        anim.SetTrigger("Out");

        lockView = true;

        Vector3 newPos = new Vector3(0, 1, -50);
        //Player.instance.transform.position = newPos;
        //Player.instance.activeCar.transform.position = newPos;
        Player.instance.GetComponent<Rigidbody>().MovePosition(newPos);
        Player.instance.activeCar.GetComponent<Rigidbody>().MovePosition(newPos);
        this.transform.root.position = cam.target.position;

        AntiPortal.SetTrigger("In");
        sun.SetActive(false);

        try
        {
            GameObject portal = GameObject.Find("Portal");
            portalScene = portal.scene;
            Destroy(portal);
        }
        catch { };

        Player.instance.loadNextScene();

        try
        {
            Player.instance.activeCar.transform.GetComponent<ExplodeOnImpactCarComponent>().cooldown = 5f;
        }
        catch { };
    }
}
