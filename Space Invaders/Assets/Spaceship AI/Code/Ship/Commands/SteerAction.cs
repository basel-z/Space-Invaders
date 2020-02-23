using UnityEngine;

/// <summary>
/// This class uses the PID controller to provide steering input to the 
/// <b>ship controller</b>. Turning the ship towards a certain target is 
/// a basis of ALL commands that can be issued to that ship.
/// </summary>
public class SteerAction
{
    /// <summary>
    /// Called by each of the orders to turn the ship towards a certain target
    /// </summary>
    /// <param name="controller"></param>
    public static void SteerTowardsTarget(ShipAI controller)
    {
        Vector3 target;

        // Temporary destination overrides waypoints (zeroed tempDest means it's not in use)
        if (controller.tempDest == Vector3.zero && controller.wayPointList.Count > 0)
            target = controller.wayPointList[controller.nextWayPoint].position;
        else
            target = controller.tempDest;


        float distance = Vector3.Distance(target, controller.transform.position);

        if (distance > 10)
        {
            // Control the angular velocity (how quickly you are changing direction)
            Vector3 angularVelocityError = controller.rBody.angularVelocity * -1;
            Vector3 angularVelocityCorrection = controller.pid_velocity.Update(angularVelocityError, Time.deltaTime);

            // Convert angular velocity correction to local space
            Vector3 lavc = controller.transform.InverseTransformVector(angularVelocityCorrection);

            Vector3 desiredHeading = target - controller.transform.position;
            Vector3 currentHeading = controller.transform.forward;
            // Control the angle itself (which direction you are facing)
            Vector3 headingError = Vector3.Cross(currentHeading, desiredHeading);
            Vector3 headingCorrection = controller.pid_angle.Update(headingError, Time.deltaTime);
            
            // Convert heading correction to local space to apply relative angular torque
            Vector3 lhc = controller.transform.InverseTransformVector(headingCorrection*200f);

            controller.angularTorque = lavc + lhc;
        }
        else
        {
            // Do not attempt to turn towards a really close target or things will become weird
            controller.angularTorque = Vector3.zero;
        }

    }
}