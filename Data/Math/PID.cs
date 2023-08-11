using Godot;
using System;

public class PID
{
    float kP, kI, kD;
    Vector3 pError = Vector3.Zero, integral = Vector3.Zero;

    public PID(float kP, float kI, float kD)
    {
        this.kP = kP;
        this.kI = kI;
        this.kD = kD;
    }

    public Vector3 Update(Vector3 current, Vector3 target, float delta)
    {
        Vector3 error = target - current;
        integral += error * delta;
        Vector3 derivative = (error - pError) / delta;

        Vector3 output = kP * error + kI * integral + kD * derivative;

        pError = error;

        return output;
    }
}