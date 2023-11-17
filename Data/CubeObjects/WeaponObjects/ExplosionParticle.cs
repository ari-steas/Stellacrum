using Godot;
using System;

/// <summary>
/// Explosion particle class. Removes itself after (Lifetime) seconds.
/// </summary>
public partial class ExplosionParticle : GpuParticles3D
{
	double age = 0;

	public ExplosionParticle() : base()
    {
        Emitting = true;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		age += delta;
		if (age > Lifetime)
			QueueFree();
	}
}
