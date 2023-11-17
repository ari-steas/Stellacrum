using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

public partial class ReactorEffects : Node3D
{
    [Export]
    public Vector3[] ParticlePoints
    {
        get
        {
            List<Vector3> points = new();
            for (int i = 0; i < particlePath.Curve.PointCount - 1; i++)
                points.Add(particlePath.Curve.GetPointPosition(i));
            return points.ToArray();
        }

        set => SetPoints(new(value), true);
    }

    GpuParticles3D particles;
    Path3D particlePath;

    ParticleProcessMaterial particleMaterial;

    Vector3 Velocity;
    Vector3 baseDirection;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        particles = FindChild("GlowParticles") as GpuParticles3D;
        particlePath = FindChild("ParticlePath") as Path3D;
        particleMaterial = particles.ProcessMaterial as ParticleProcessMaterial;

        baseDirection = particleMaterial.Direction.Normalized();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        //particleMaterial.Direction = (baseDirection * 15 + Velocity * particles.GlobalTransform.Basis).Normalized();

        //particleMaterial.InitialVelocityMax = 20 + Velocity.Length();
        //particleMaterial.InitialVelocityMin = 15 + Velocity.Length();
    }

    public void SetVelocity(Vector3 velocity) => Velocity = velocity;

    public void SetIntensity(float pct)
    {
        particles.Lifetime = 1 / pct;
    }

    public void SetPoints(List<Vector3> allPoints, bool local = false)
    {
        if (allPoints.Count == 0)
            return;

        particlePath.Curve.ClearPoints();
        foreach (var point in allPoints)
        {
            particlePath.Curve.AddPoint(local ? point : ToLocal(point));
            // Set point out so that it makes a smooth curve
            if (particlePath.Curve.PointCount > 1)
                particlePath.Curve.SetPointOut(particlePath.Curve.PointCount - 2, local ? point : ToLocal(point));
        }

        // Add final point to make a loop
        particlePath.Curve.AddPoint(local ? allPoints[0] : ToLocal(allPoints[0]));
        particlePath.Curve.SetPointOut(particlePath.Curve.PointCount - 2, local ? allPoints[0] : ToLocal(allPoints[0]));

        for (int i = 0; i < particlePath.Curve.PointCount; i++)
            GD.Print($"{i}: {particlePath.Curve.GetPointPosition(i)} to {particlePath.Curve.GetPointOut(i)}");
    }
}
