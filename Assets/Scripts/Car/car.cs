using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] float carTopSpeed;
    [SerializeField] float rollGrip;
    [SerializeField] float breakForce;
    public float accelInput;

    [Header("Transforms")]
    [SerializeField] Transform s1;
    [SerializeField] Transform s2;
    [SerializeField] Transform s3;
    [SerializeField] Transform s4;

    [SerializeField] Transform w1;
    [SerializeField] Transform w2;
    [SerializeField] Transform w3;
    [SerializeField] Transform w4;

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

        Debug.Log(rb.velocity.magnitude * 3.6f);

        accelInput = Mathf.Clamp((Input.GetAxis("Vertical") + (Input.GetAxis("R2")+1) - (Input.GetAxis("L2")+1)), -1, 1) * 35000;

        bool handbrake = false;
        if (Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1"))
            handbrake = true;

        lastoffsets1 = Spring(s1, lastoffsets1, accelInput, frontStrength, FrontGrip, false, w1);
        lastoffsets2 = Spring(s2, lastoffsets2, accelInput, frontStrength, FrontGrip, false, w2);
        lastoffsets3 = Spring(s3, lastoffsets3, accelInput, backStrength, BackGrip, handbrake, w3);
        lastoffsets4 = Spring(s4, lastoffsets4, accelInput, backStrength, BackGrip, handbrake, w4);

        float steering = Input.GetAxis("Horizontal") * -25 * steeringSensitivity;

        s1.localRotation = Quaternion.Euler(s1.localEulerAngles.x, -steering, s1.localEulerAngles.z);
        s2.localRotation = Quaternion.Euler(s2.localEulerAngles.x, -steering, s2.localEulerAngles.z);

        w1.localRotation = Quaternion.Euler(s1.localEulerAngles.x, -steering, s1.localEulerAngles.z);
        w2.localRotation = Quaternion.Euler(s2.localEulerAngles.x, -steering, s2.localEulerAngles.z);
    }

    float Spring(Transform a, float b, float c, float strength, AnimationCurve tireGrip, bool handbreak, Transform wheel)
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

            float tireGripPercent = 100 / tireVel.magnitude * steeringVel;

            float velChange = 0;
            if (true) 
            {
                velChange = -steeringVel * tireGrip.Evaluate(tireGripPercent);
            }
            else
            {
                velChange = -steeringVel * 0.05f;
            }
            float velAcceleration = velChange / Time.fixedDeltaTime;

            rb.AddForceAtPosition(steeringDir * TireMass * velAcceleration, a.position);

            //Driving
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

            return (offset);
        }
        else
        {
            return (length);
        }
    }

    float ToFloat(bool a)
    {
        float b = 0;
        if (a)
            b = 1;
        return b;
    }
}
