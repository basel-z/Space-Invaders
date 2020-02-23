using UnityEngine;

/// <summary>
/// This class gives an order to each of the AI ships when the 
/// scene is started. References to the main Ship script are held here
/// as well as the waypoints around which the ships will fly.
/// </summary>
public class AIShipController : MonoBehaviour {

    [Header("AI Ships")]
    [Tooltip("References to ships in the scene (tag <i>Ship</i>)")]
    public Ship[] AIShips;
    [Tooltip("References to waypoints (tag <i>Waypoint</i>)")]
    public Transform[] Waypoints;

	void Start () {
        // Give orders to AI ships
		for(int i=0; i<AIShips.Length; i++)
        {
            AIShips[i].AIController.PatrolPath(Waypoints);
        }
    }

}
