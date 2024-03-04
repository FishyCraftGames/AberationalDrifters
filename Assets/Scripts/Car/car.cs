using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// thank you to ToyfullGames for the amazing tutorial used for the base code
// https://www.toyfulgames.com/blog/deep-dive-car-physics

public class car : MonoBehaviour
{

    [SerializeField] LayerMask ground;

    [Header("Springs")]
    [SerializeField] float length;
    [SerializeField] float frontStrength;
    [SerializeField] float backStrength;
    [SerializeField] float dampening;

    [Header("Steering")]
    [SerializeField] AnimationCurve FrontGrip; 
    [SerializeField] AnimationCurve BackGrip; 
    [SerializeField] float TireMass;
    [SerializeField] float turnSpeed;
    [SerializeField] float steeringSensitivity;

    [Header("Engine")]
    [SerializeField] AnimationCurve powerCurve;
    [SerializeField] AnimationCurve engineVolume;
    [SerializeField] AnimationCurve enginePitch;
    [SerializeField] AudioSource engineA;
    [SerializeField] float carTopSpeed;
    [SerializeField] float rollGrip;
    [SerializeField] float breakForce;
    public float accelInput;

    [Header("Audio")]
    [SerializeField] AudioSource handbreak;
    [SerializeField] AudioSource tire1;
    [SerializeField] AudioSource tire2;
    [SerializeField] AudioSource tire3;
    [SerializeField] AudioSource tire4;

    [Header("Transforms")]
    [SerializeField] Transform s1;
    [SerializeField] Transform s2;
    [SerializeField] Transform s3;
    [SerializeField] Transform s4;

    [SerializeField] Transform w1;
    [SerializeField] Transform w2;
    [SerializeField] Transform w3;
    [SerializeField] Transform w4;

    [SerializeField] TrailRenderer tm1;
    [SerializeField] TrailRenderer tm2;
    [SerializeField] TrailRenderer tm3;
    [SerializeField] TrailRenderer tm4;

    [SerializeField] ParticleSystem p1;
    [SerializeField] ParticleSystem p2;
    [SerializeField] ParticleSystem p3;
    [SerializeField] ParticleSystem p4;

    public float lastoffsets1;
    public float lastoffsets2;
    public float lastoffsets3;
    public float lastoffsets4;

    [Space]
    [SerializeField] Rigidbody rb;

    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        isGrounded = false;

        Debug.Log(rb.velocity.magnitude * 3.6f + " km/h");

        accelInput = Mathf.Clamp((Input.GetAxis("Vertical") + (Input.GetAxis("R2")+1) - (Input.GetAxis("L2")+1)), -1, 1) * 35000;

        bool handbrake = false;
        if (Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1"))
            handbrake = true;

        float x = rb.velocity.magnitude / carTopSpeed;

        engineA.volume = engineVolume.Evaluate(x) * Mathf.Clamp(accelInput/35000, 0.3f, 1);
        engineA.pitch = enginePitch.Evaluate(x) * Mathf.Clamp(accelInput/35000, 0.8f, 1);

        if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
        {
            handbreak.pitch = Random.Range(0.9f, 1.1f);
            handbreak.Play();
        }

        lastoffsets1 = Spring(s1, lastoffsets1, accelInput, frontStrength, FrontGrip, false, w1, tm1, p1, tire1);
        lastoffsets2 = Spring(s2, lastoffsets2, accelInput, frontStrength, FrontGrip, false, w2, tm2, p2, tire2);
        lastoffsets3 = Spring(s3, lastoffsets3, accelInput, backStrength, BackGrip, handbrake, w3, tm3, p3, tire3);
        lastoffsets4 = Spring(s4, lastoffsets4, accelInput, backStrength, BackGrip, handbrake, w4, tm4, p4, tire4);

        float steering = Input.GetAxis("Horizontal") * -25 * steeringSensitivity;

        p1.transform.LookAt(p1.transform.position + rb.velocity);
        p2.transform.LookAt(p2.transform.position + rb.velocity);
        p3.transform.LookAt(p3.transform.position + rb.velocity);
        p4.transform.LookAt(p4.transform.position + rb.velocity);

        s1.localRotation = Quaternion.Euler(s1.localEulerAngles.x, -steering, s1.localEulerAngles.z);
        s2.localRotation = Quaternion.Euler(s2.localEulerAngles.x, -steering, s2.localEulerAngles.z);

        w1.localRotation = Quaternion.Euler(s1.localEulerAngles.x, -steering, s1.localEulerAngles.z);
        w2.localRotation = Quaternion.Euler(s2.localEulerAngles.x, -steering, s2.localEulerAngles.z);
    }

    float Spring(Transform a, float b, float c, float strength, AnimationCurve tireGrip, bool handbreak, Transform wheel, TrailRenderer r, ParticleSystem p, AudioSource audio)
    {
        Ray ray = new Ray(a.position, -a.up);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.distance < length)
            {
                isGrounded = true;
            }

            //Springs

            float offset = length - Mathf.Clamp(hit.distance, 0, length);
            offset = Mathf.Clamp(offset, 0, length);
            float velocity = (b - offset) / Time.fixedDeltaTime;
            float force = (offset * strength) - (velocity * dampening);
            //rb.AddForceAtPosition(new Vector3(0, force, 0), a.position, ForceMode.Acceleration);
            rb.AddForceAtPosition(transform.up * force, a.position, ForceMode.Acceleration);

            //update tire position
            Vector3 tirePos = a.position + -a.up * Mathf.Clamp(hit.distance, 0.22f, 0.8f) + a.up * 0.35f;
            wheel.position = tirePos;

            //Steering
            Vector3 steeringDir = a.right;
            Vector3 tireVel = rb.GetPointVelocity(a.position);

            float steeringVel = Vector3.Dot(steeringDir, tireVel);

            float tireGripPercent = Mathf.Abs(100 / tireVel.magnitude * steeringVel)/100;

            if (isGrounded && tireGripPercent > 0.4f) 
            {
                audio.volume = Mathf.Clamp((tireGripPercent * (tireVel.magnitude / 15)), 0, 1);
                r.emitting = true;
                p.Play();
            }
            else
            {
                audio.volume = 0;
                r.emitting = false;
                p.Stop();
            }


            float velChange = 0;
            //i know that what im about to do is wrong but it feels better
            //dond tjudge me for doing fake math
            velChange = -steeringVel * (tireGrip.Evaluate(tireGripPercent) + tireGrip.Evaluate(rb.velocity.magnitude / carTopSpeed)) / 2;
            float velAcceleration = velChange / Time.fixedDeltaTime;

            if (isGrounded)
            {
                rb.AddForceAtPosition(steeringDir * TireMass * velAcceleration, a.position);
            }
            else
            {
                rb.AddForceAtPosition(steeringDir * TireMass * velAcceleration * 0.5f, a.position);
            }

            //Driving
            if (isGrounded)
            {
                if (!handbreak)
                {
                    if (c > 0f)
                    {
                        Vector3 accelDir = a.forward;

                        float carSpeed = Vector3.Dot(transform.forward, rb.velocity);
                        float normalizeSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
                        float avaliableTorque = powerCurve.Evaluate(normalizeSpeed) * c * 2;

                        rb.AddForceAtPosition(accelDir * avaliableTorque, a.position);
                    }
                    else if (c < 0f)
                    {
                        Vector3 accelDir = -a.forward;

                        float carSpeed = Vector3.Dot(transform.forward, rb.velocity);
                        float normalizeSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
                        float avaliableTorque = powerCurve.Evaluate(normalizeSpeed) * Mathf.Abs(c / 2);

                        rb.AddForceAtPosition(accelDir * avaliableTorque, a.position);
                    }
                    else
                    {
                        Vector3 accelDir = a.forward;
                        Vector3 tireVel2 = rb.GetPointVelocity(a.position);

                        float steeringVel2 = Vector3.Dot(accelDir, tireVel2);
                        float velChange2 = -steeringVel2 * rollGrip;
                        float velAcceleration2 = velChange2 / Time.fixedDeltaTime;

                        rb.AddForceAtPosition(accelDir * TireMass * velAcceleration2, a.position);
                    }
                }
                else
                {
                    Vector3 accelDir = a.forward;
                    Vector3 tireVel2 = rb.GetPointVelocity(a.position);

                    float steeringVel2 = Vector3.Dot(accelDir, tireVel2);
                    float velChange2 = -steeringVel2 * 1;
                    float velAcceleration2 = velChange2 / Time.fixedDeltaTime;

                    rb.AddForceAtPosition(accelDir * TireMass * velAcceleration2, a.position);
                }
            }

            return (offset);
        }
        else
        {
            return (length);
        }
    }
}
