using Godot;
using System;

public class PID
{
    readonly float kP, kI, kD;
    float pError = 0, integral = 0;

    public PID(float kP, float kI, float kD)
    {
        this.kP = kP;
        this.kI = kI;
        this.kD = kD;
    }

    public float Update(float current, float target, float delta)
    {
        float error = target - current;
        integral += error * delta;
        float derivative = (error - pError) / delta;

        float output = kP * error + kI * integral + kD * derivative;

        pError = error;

        return output;
    }
}