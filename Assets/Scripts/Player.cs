using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
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

    public bool inCar;
    bool isGrounded;

    public Transform camRotator;

    public GameObject Explosion;

    public float kaboom = 0.5f;
    Vector3 explosionPoint;

    public float explosionForce;


    private void Start()
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Additive);
        rb = GetComponent<Rigidbody>();
        instance = this;
    }

    private void Update()
    {
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
                activeCar = other.transform.GetComponent<car>();
                transform.rotation = other.transform.rotation;
                transform.position = other.transform.position;
                FixedJoint fj = transform.AddComponent<FixedJoint>();
                fj.connectedBody = other.transform.GetComponent<Rigidbody>();
                fj.breakForce = 850;
                fj.breakTorque = 400;
            }
        }
    }

    private void OnJointBreak(float breakForce)
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

        activeCar = null;
        kaboom = 0.5f;
    }

    public void unloadScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

}
