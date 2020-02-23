using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderIdle : Order
{
    public OrderIdle()
    {
        Name = "Idle";
    }

    public override void UpdateState(ShipAI controller)
    {
        SteerAction.SteerTowardsTarget(controller);

        if (CheckExitCondition(controller))
            controller.FinishOrder();
    }

    /// <summary>
    /// Checks the exit condition of the order and, if satisfied, finishes the given order.
    /// </summary>
    /// <param name="controller">Reference to ship's AI controller</param>
    /// <returns>True if order should be terminated, false otherwise</returns>
    protected bool CheckExitCondition(ShipAI controller)
    {
        // Note: this order will never end (there is no end condition)!
        if (controller.tempDest == Vector3.zero)
            controller.tempDest = GenerateNextWaypoint(controller.transform);

        float distance = Vector3.Distance(controller.tempDest, controller.transform.position);

        if (distance < 30)
        {
            controller.tempDest = GenerateNextWaypoint(controller.transform);
        }

        controller.throttle = Mathf.MoveTowards(controller.throttle, 0.5f, Time.deltaTime * 0.5f);

        return false;
    }

    /// <summary>
    /// Generates a waypoint at a random relation to the ship in its vicinity.
    /// </summary>
    /// <param name="currPos"></param>
    /// <returns></returns>
    private Vector3 GenerateNextWaypoint(Transform currPos)
    {
        Vector3 randomDirection = new Vector3(Random.Range(-200, 200),
            Random.Range(-200, 200),
            Random.Range(-200, 200));

        randomDirection = currPos.position + randomDirection;

        return randomDirection;
    }

}