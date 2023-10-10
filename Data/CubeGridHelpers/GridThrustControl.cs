using Godot;
using System;
using System.Collections.Generic;

public class GridThrustControl
{
    Vector3 centerOfMass = new();
    List<ThrusterBlock> ThrusterBlocks = new();
    public bool Dampen = true;

    Vector3 LinearInput = Vector3.Zero;
    Vector3 AngularInput = Vector3.Zero;

    VectorPID angularPID = new(0.5f, 0.0f, 0.1f);

    public void Update(Vector3 LinearVelocity, Vector3 AngularVelocity, double delta)
    {
        Vector3 desiredLinearVel = (Dampen && LinearInput.IsZeroApprox()) ? Vector3.Zero : (LinearInput + LinearVelocity);
        Vector3 desiredAngularVel = angularPID.Update(AngularVelocity, AngularInput, (float) delta);

        foreach (var thruster in ThrusterBlocks)
		{
			thruster.SetDesiredAngularVelocity(desiredAngularVel);
			thruster.SetDesiredLinearVelocity(desiredLinearVel);
		}
    }

    /// <summary>
    /// Sets linear input. Formatted as world-aligned Vector3 direct control input.
    /// </summary>
    /// <param name="input"></param>
    public void SetInputLinear(Vector3 input)
    {
        LinearInput = input;
    }

    /// <summary>
    /// Sets angular input. Formatted as world-aligned Vector3 direction.
    /// </summary>
    /// <param name="input"></param>
    public void SetInputAngular(Vector3 input)
    {
        AngularInput = input;
    }

    public void Init(List<ThrusterBlock> thrusters)
    {
        ThrusterBlocks = thrusters;
    }

    public void SetCenterOfMass(Vector3 CoM)
    {
        centerOfMass = CoM;
    }
}