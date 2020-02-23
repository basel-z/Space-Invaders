using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Class performs the commands issued to the ship and provides an interface to 
/// issue commands to the ship.
/// </summary>
public class ShipAI : MonoBehaviour
{
    // The current order issued to this ship, can be null
    public Order CurrentOrder;

    // Waypoints list or target reference (some orders use only the first element)
    [HideInInspector] public List<Transform> wayPointList;
    // Index of the next waypoint in the wayPointList
    [HideInInspector] public int nextWayPoint;

    // The current position of the destination, used by each of the Orders
    [HideInInspector] public Vector3 tempDest;

    // Throttle value of engines, from 0.0 to 1.0
    [HideInInspector] public float throttle;

    // Yaw, Pitch and Roll torque values
    [HideInInspector] public Vector3 angularTorque;

    [HideInInspector] public PIDController pid_angle, pid_velocity;
    // I've experimentally determined that these parameters work quite well
    [HideInInspector] public float pid_P = 10, pid_I = 0.5f, pid_D = 0.5f;

    // Rigidbody
    [HideInInspector] public Rigidbody rBody;

    // Main Ship script with references to all ship components
    [HideInInspector] public Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        rBody = GetComponent<Rigidbody>();

        // Initialize the PID controllers with preset parameters
        pid_angle = new PIDController(pid_P, pid_I, pid_D);
        pid_velocity = new PIDController(pid_P, pid_I, pid_D);

        wayPointList = new List<Transform>();
    }

    private void Update()
    { 
        // If an order is present, perform it
        if(CurrentOrder != null)
            CurrentOrder.UpdateState(this);
        else
            throttle = 0f;
    }

    /// <summary>
    /// Called when a finishable order (move to) is completed. Includes cleanup.
    /// </summary>
    public void FinishOrder()
    {
        CurrentOrder = null;
        tempDest = Vector3.zero;
        ConsoleOutput.Instance.PostMessage(name + " has completed the order.");
    }

    // Autopilot commands
    #region commands
    /// <summary>
    /// Commands the ship to move to a given object.
    /// </summary>
    /// <param name="destination"></param>
    public void MoveTo(Transform destination)
    {
        if (destination != null)
        {
            wayPointList.Clear();
            wayPointList.Add(destination);
            nextWayPoint = 0;

            CurrentOrder = new OrderMove();
            ConsoleOutput.Instance.PostMessage(name + ": command "+CurrentOrder.Name+" accepted");
        }        
    }

    /// <summary>
    /// Commands the ship to move to a specified position.
    /// </summary>
    /// <param name="position">world position of destination</param>
    public void MoveTo(Vector3 position)
    {
        if (tempDest == Vector3.zero)
            return;

        CurrentOrder = new OrderMove();
        ConsoleOutput.Instance.PostMessage(name + ": command " + CurrentOrder.Name + " accepted");
    }

    /// <summary>
    /// Commands the ship to move through the given waypoints. Once the last one is reached,
    /// the route is restarted from the first waypoint.
    /// </summary>
    /// <param name="waypoints"></param>
    public void PatrolPath(Transform[] waypoints)
    {
        CurrentOrder = new OrderPatrol();

        wayPointList.Clear();
        wayPointList.AddRange(waypoints);
        nextWayPoint = 0;

        ConsoleOutput.Instance.PostMessage(name + ": command " + CurrentOrder.Name + " accepted");
    }

    /// <summary>
    /// Commands the ship to move randomly at low speed, roughly in the same area.
    /// </summary>
    public void Idle()
    {
        CurrentOrder = new OrderIdle();
        tempDest = transform.position;

        ConsoleOutput.Instance.PostMessage(name + ": command " + CurrentOrder.Name + " accepted");
    }

    /// <summary>
    /// Commands the ship to follow a target
    /// </summary>
    public void Follow(Transform target)
    {
        if (target != null)
        {
            wayPointList.Clear();
            wayPointList.Add(target);
            nextWayPoint = 0;

            CurrentOrder = new OrderFollow();
            ConsoleOutput.Instance.PostMessage(name + ": command " + CurrentOrder.Name + " accepted");
        }
       
    }

    #endregion commands
}