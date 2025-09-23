using Godot;
using System;

public class VectorPID
{
    public float Kp, Ki, Kd;
    Vector3 pError = Vector3.Zero, integral = Vector3.Zero;

    public VectorPID(float Kp, float Ki, float Kd)
    {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }

    public Vector3 Update(Vector3 current, Vector3 target, float delta)
    {
        Vector3 error = target - current;
        integral += error * delta;
        Vector3 derivative = (error - pError) / delta;

        Vector3 output = Kp * error + Ki * integral + Kd * derivative;

        pError = error;

        return output;
    }
}