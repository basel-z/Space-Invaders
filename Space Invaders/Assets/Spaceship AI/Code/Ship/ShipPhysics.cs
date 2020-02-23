using UnityEngine;

/// <summary>
/// Applies linear and angular forces to a ship.
/// This is based on the ship physics from https://github.com/brihernandez/UnityCommon/blob/master/Assets/ShipPhysics/ShipPhysics.cs
/// </summary>
public class ShipPhysics : MonoBehaviour
{
    [Tooltip("X: Lateral thrust\nY: Vertical thrust\nZ: Longitudinal Thrust")]
    public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);

    [Tooltip("X: Pitch\nY: Yaw\nZ: Roll")]
    public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);

    [Range(0.0f, 1.0f)]
    [Tooltip("Multiplier for longitudinal thrust when reverse thrust is requested.")]
    private float reverseMultiplier = 1.0f;

    [Tooltip("Multiplier for all forces. Can be used to keep force numbers smaller and more readable.")]
    private float forceMultiplier = 100.0f;

    public Rigidbody Rigidbody { get { return rbody; } }

    private Vector3 appliedLinearForce = Vector3.zero;
    private Vector3 appliedAngularForce = Vector3.zero;

    private Vector3 maxAngularForce;

    private Rigidbody rbody;

    private float rBodyDrag;

    // Keep a reference to the ship this is attached to just in case.
    private Ship ship;

    // Use this for initialization
    void Awake()
    {
        ship = GetComponent<Ship>();
        rbody = GetComponent<Rigidbody>();

        rBodyDrag = rbody.drag;
        maxAngularForce = angularForce * forceMultiplier;
    }

    void FixedUpdate()
    {
        if (rbody != null)
        {
            // Ship is moved by linear (throttle and strafe) and angular (yaw, pitch, roll) forces
            rbody.AddRelativeForce(
                appliedLinearForce, 
                ForceMode.Force);
            rbody.AddRelativeTorque(
                ClampVector3(appliedAngularForce, -1 * maxAngularForce, maxAngularForce),
                ForceMode.Force);
        }
    }

    private void Update()
    {
        // Read throttle and torque values from ship's AI Controller
        Vector3 linearInput = new Vector3(0, 0, ship.AIController.throttle);
        appliedLinearForce = MultiplyByComponent(linearInput, linearForce) * forceMultiplier;
        appliedAngularForce = ship.AIController.angularTorque;
        appliedAngularForce.z = 0;
    }

    #region helper methods
    /// <summary>
    /// Returns a Vector3 where each component of Vector A is multiplied by the equivalent component of Vector B.
    /// </summary>
    private Vector3 MultiplyByComponent(Vector3 a, Vector3 b)
    {
        Vector3 ret;

        ret.x = a.x * b.x;
        ret.y = a.y * b.y;
        ret.z = a.z * b.z;

        return ret;
    }

    /// <summary>
    /// Clamps vector components to a value between the minimum and maximum values given in min and max vectors.
    /// </summary>
    /// <param name="vector">Vector to be clamped</param>
    /// <param name="min">Minimum vector components allowed</param>
    /// <param name="max">Maximum vector components allowed</param>
    /// <returns></returns>
    private Vector3 ClampVector3(Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z)
            );
    }
    #endregion helper methods
}