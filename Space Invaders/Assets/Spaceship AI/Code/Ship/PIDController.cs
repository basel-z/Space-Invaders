using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An implementation of the Proportional-Integral-Derivative controller. PID controllers are used
/// to provide continually modulated input as a feedback of its error value upon which a correction
/// is applied. In other words, to have a physics-controlled spaceship, you need a PID controller
/// to make it turn towards the destination correctly. This class is used directly by the SteerAction.
/// 
/// How the ships handle will depends, in part, on these parameters (P, I and D). Tweaking these 
/// parameters will help you understand how it affects steering.
/// </summary>
public class PIDController
{
    public float pFactor, iFactor, dFactor;

    private Vector3 integral;
    private Vector3 lastError;

    public PIDController(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public Vector3 Update(Vector3 currentError, float timeFrame)
    {
        // Compute the area under the error curve
        integral += currentError * timeFrame;
        // Compute the amount of change of the error value
        var deriv = (currentError - lastError) / timeFrame;
        lastError = currentError;
        // Compute and return the feedback based on error amount and change
        return currentError * pFactor
            + integral * iFactor
            + deriv * dFactor;
    }
}