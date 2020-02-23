using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderPatrol : Order
{
    private Transform target;

    public OrderPatrol()
    {
        Name = "Patrol";
    }

    public override void UpdateState(ShipAI controller)
    {
        SteerAction.SteerTowardsTarget(controller);

        PatrolWaypoints(controller);
    }

    private void PatrolWaypoints(ShipAI controller)
    {
        float distance = Vector3.Distance(controller.wayPointList[controller.nextWayPoint].position, controller.transform.position);

        if (distance < 30)
        {
            controller.nextWayPoint = (controller.nextWayPoint + 1) % controller.wayPointList.Count;
        }

        controller.throttle = 1.0f;
    }


}