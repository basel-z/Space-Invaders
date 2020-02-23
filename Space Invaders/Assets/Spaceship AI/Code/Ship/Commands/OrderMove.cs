using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderMove : Order
{
    public OrderMove()
    {
        Name = "Move";
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
        Vector3 target;
        // Temporary destination overrides waypoints
        if (controller.tempDest == Vector3.zero && controller.wayPointList.Count > 0)
            target = controller.wayPointList[controller.nextWayPoint].position;
        else
            target = controller.tempDest;

        float distance = Vector3.Distance(target, controller.transform.position);

        if (distance < 10)
        {
            controller.tempDest = Vector3.zero;
            return true;
        }

        float thr = distance > 100f ? 1f : (distance / 100f);
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        return false;
    }

}
