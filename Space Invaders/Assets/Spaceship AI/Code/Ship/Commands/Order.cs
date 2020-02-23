using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for any ship orders. Ensures there is a name for an order and
/// the update function which receives the ship's context.
/// </summary>
public abstract class Order
{
    // Name of the order, eg. "Attack Enemies" or "Idle"
    public string Name;

    /// <summary>
    /// The main method of the Order class, invoked in the Update function of the 
    /// ship's artificial intelligence controller. Context is provided via the ShipAI
    /// object which has all the needed values.
    /// </summary>
    /// <param name="controller"></param>
    public abstract void UpdateState(ShipAI controller);
}