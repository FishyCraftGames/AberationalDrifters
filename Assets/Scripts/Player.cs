using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    [Header("Gliding")]
    public float Speed;
    public float sensitivity = 0.01f;

    [Header("Other Stuff")]
    public car activeCar;

    private Rigidbody rb;

    public static Player instance;

    public List<string> scenes = new List<string>();

    public bool inCar;
    bool isGrounded;

    public Transform camRotator;

    public GameObject Explosion;

    public float kaboom = 0.5f;
    Vector3 explosionPoint;

    public float explosionForce;

    public GameObject gameOver;
    public GameObject menuButton;

    float invincibility = 1f;

    public float swooshVolume = 0;

    private void Start()
    {
        SceneManager.LoadSceneAsync(scenes[Random.Range(0, scenes.Count - 1)], LoadSceneMode.Additive);
        rb = GetComponent<Rigidbody>();
        instance = this;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Explode();
        }

        if(transform.position.y < -7f)
        {
            gameOver.SetActive(true);
            EventSystem.current.SetSelectedGameObject(menuButton);
        }

        if (!inCar)
        {
            kaboom -= Time.deltaTime;
        }

        if(kaboom <= 0 && kaboom > -100)
        {
            kaboom = -101;
            rb.AddForce(Vector3.up * explosionForce, ForceMode.Impulse);
            Debug.LogError("SecondaryWxplosion");
        }

        invincibility -= Time.deltaTime;

        if(invincibility > 0)
        {
            SphereCollider[] sc = transform.GetComponents<SphereCollider>();
            foreach(SphereCollider s in sc)
            {
                if (!s.isTrigger)
                    s.enabled = false;
            }
        }
        else
        {
            SphereCollider[] sc = transform.GetComponents<SphereCollider>();
            foreach (SphereCollider s in sc)
            {
                if (!s.isTrigger)
                    s.enabled = true;
            }
        }

        swooshVolume = rb.velocity.magnitude / activeCar.carTopSpeed;
        swooshVolume *= swooshVolume;
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit);
        if(hit.distance < 0.2f)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (!inCar && !isGrounded)
        {
            Vector3 rot = transform.eulerAngles;
            float percentage;

            rot.x += 10 * Input.GetAxis("Vertical") * sensitivity;
            rot.x = Mathf.Clamp(rot.x, 0, 90);

            rot.y += 20 * Input.GetAxis("Horizontal") * sensitivity;

            rot.z = -5 * Input.GetAxis("Horizontal");
            rot.z = Mathf.Clamp(rot.z, -30, 30);
            transform.rotation = Quaternion.Euler(rot);

            percentage = rot.x / 90;

            float modDrag = (percentage * -2) + 8;
            float modSpeed = percentage * 5f + 5f;

            rb.drag = modDrag;
            Vector3 localV = transform.InverseTransformDirection(rb.velocity);
            localV.z = modSpeed;
            rb.velocity = transform.TransformDirection(localV);
            rb.AddForce(Vector3.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeCar == null && kaboom < -100)
        {
            if (other.gameObject.tag == "Car")
            {
                SceneManager.MoveGameObjectToScene(other.transform.root.gameObject, this.gameObject.scene);

                inCar = true;
                activeCar = other.transform.GetComponent<car>();
                transform.rotation = other.transform.rotation;
                transform.position = other.transform.position;
                FixedJoint fj = transform.AddComponent<FixedJoint>();
                fj.connectedBody = other.transform.GetComponent<Rigidbody>();
                fj.breakForce = 600;
                fj.breakTorque = 300;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag != "Car" && invincibility < 0)
        {
            gameOver.SetActive(true);
            EventSystem.current.SetSelectedGameObject(menuButton);
        }

    }

    private void OnJointBreak(float breakForce)
    {
        Explode();
    }

    public void Explode()
    {
        if (inCar)
        {
            Instantiate(Explosion, transform.position, transform.rotation);
            explosionPoint = transform.position - Vector3.down * 5f;

            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            Vector3 force = activeCar.GetComponent<Rigidbody>().velocity * 3;
            force.x = 0;
            force.z = 0;
            force.y = 40f;
            rb.AddForce(force, ForceMode.Impulse);
            inCar = false;

            SceneManager.MoveGameObjectToScene(activeCar.transform.gameObject, SceneManager.GetActiveScene());

            activeCar = null;
            kaboom = 0.5f;
            invincibility = 1f;
        }
    }

    public void unloadScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    public void loadNextScene()
    {
        SceneManager.LoadSceneAsync(scenes[Random.Range(0, scenes.Count - 1)], LoadSceneMode.Additive);
    }

}
