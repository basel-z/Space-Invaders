using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Allows the user to issue an order to the ship selected on the user interface.
/// The selected ship is stored in the HUDMarkers script.
/// </summary>
public class OrderMenu : MonoBehaviour
{
    [Tooltip("Buttons which give orders to currently selected ship")]
    public Button[] OrderButtons;

    // Just some consts for easy access
    private const int MOVE_TO = 0, PATROL = 1, IDLE = 2;

    // Waypoints in scene
    private Transform[] Waypoints;

    private void Awake()
    {
        // Get all waypoints in scene
        var wps = GameObject.FindGameObjectsWithTag("Waypoint");
        // Get Transform references instead of GameObject references
        Waypoints = new Transform[wps.Length];
        for(int i=0; i<wps.Length; i++){
            Waypoints[i] = wps[i].transform;
        }
    }

    /// <summary>
    /// Below are three examples on how to issue orders to a ship. 
    /// </summary>
    void Start()
    {
        if (OrderButtons.Length < 3)
            Debug.LogError("Order buttons not assigned to OrderMenu object");

        OrderButtons[MOVE_TO].onClick.AddListener(() =>
        {
            if (HUDMarkers.Instance.Target != null)
            {
                var RandomWaypoint = Waypoints[Random.Range(0, Waypoints.Length - 1)];
                HUDMarkers.Instance.Target.GetComponent<ShipAI>().MoveTo(RandomWaypoint);
            }
            else
            {
                ShowError();
            }
        });
        OrderButtons[PATROL].onClick.AddListener(() =>
        {
            if (HUDMarkers.Instance.Target != null)
            {
                HUDMarkers.Instance.Target.GetComponent<ShipAI>().PatrolPath(Waypoints);
            }
            else
            {
                ShowError();
            }
        });
        OrderButtons[IDLE].onClick.AddListener(() =>
        {
            if (HUDMarkers.Instance.Target != null)
            {
                HUDMarkers.Instance.Target.GetComponent<ShipAI>().Idle();
            }
            else
            {
                ShowError();
            }
        });
    }

    private static void ShowError()
    {
        ConsoleOutput.Instance.PostMessage("Error: No ship is targeted!", Color.red);
    }

}
