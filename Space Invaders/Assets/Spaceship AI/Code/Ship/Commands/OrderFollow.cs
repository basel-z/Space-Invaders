using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderFollow : Order
{
    public OrderFollow()
    {
        Name = "Follow";
    }

    public override void UpdateState(ShipAI controller)
    {
        SteerAction.SteerTowardsTarget(controller);

        float distance = Vector3.Distance(controller.wayPointList[controller.nextWayPoint].position, controller.transform.position);

        controller.throttle = distance > 30f ? 1f : 0f;
    }
}