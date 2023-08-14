using Godot;
using System;

public class VectorPID
{
    PID pX, pY, pZ;

    public VectorPID(float kP, float kI, float kD)
    {
        pX = new(kP, kI, kD);
        pY = new(kP, kI, kD);
        pZ = new(kP, kI, kD);
    }

    public Vector3 Update(Vector3 current, Vector3 target, float delta)
    {
        return new(
            pX.Update(current.X, target.X, delta),
            pY.Update(current.Y, target.Y, delta),
            pX.Update(current.Z, target.Z, delta)
        );
    }
}