using UnityEngine;

/// <summary>
/// Ties all the primary ship components together.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    #region ship components

    // Artificial intelligence controls
    public ShipAI AIController
    {
        get { return aiInput; }
    }
    private ShipAI aiInput;

    // Ship rigidbody physics
    private ShipPhysics physics;

    #endregion ship components

    private void Awake()
    {
        aiInput = GetComponent<ShipAI>();
        physics = GetComponent<ShipPhysics>();
    }

}
