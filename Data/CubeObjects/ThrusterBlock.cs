using Godot;
using System;
using System.Collections.Generic;

public partial class ThrusterBlock : CubeBlock
{
	CubeGrid parent;
	public float ThrustPercent { get; private set; } = 0;
	public float MaximumThrust { get; private set; } = 0;
	public bool Dampen = true;
	//public Vector3 ThrustForwardVector { get; private set; } = Vector3.Zero;
	private Node3D thrustNode;
	private GpuParticles3D particles;

	public void SetThrustPercent(float pct)
	{
		ThrustPercent = Mathf.Clamp(pct, 0, 1);
	}

	public override void _Ready()
	{
		parent = GetParent<CubeGrid>();
		particles = new()
		{
			Position = thrustNode.Position,
			Rotation = thrustNode.Rotation,
			ProcessMaterial = GD.Load<ParticleProcessMaterial>("res://Assets/Images/Particles/ThrusterProcessMaterial.tres"),
			DrawPass1 = GD.Load<Mesh>("res://Assets/Images/Particles/ThrusterDrawPass.tres"),
			Amount = 64,
			TransformAlign = GpuParticles3D.TransformAlignEnum.ZBillboardYToVelocity,
			Emitting = false
		};
		AddChild(particles);
	}

	VectorPID pid;
	Vector3 angularDesired = Vector3.Zero;
	Vector3 linearDesired = Vector3.Zero;


	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Dampen)
		{
			//SetThrustPercent(AngularControl());
			//SetThrustPercent(LinearControl());
			SetThrustPercent(AngularControl() + LinearControl());
		}

		particles.Emitting = ThrustPercent > 0;
		if (!((int) (64*ThrustPercent) > 0.01))
			particles.Emitting = false;

		parent.ApplyForce(GlobalTransform.Basis * Vector3.Forward * ThrustPercent * MaximumThrust * (float) delta, GlobalPosition - parent.GlobalPosition);
		
		// Make particles inherit velocity of parent
		particles.ProcessMaterial.Set("initial_velocity_min", parent.Speed + 30);
		particles.ProcessMaterial.Set("initial_velocity_max", parent.Speed + 40);
	}

	float AngularControl()
	{
		//float pct = parent.MovementInput.Dot(Transform.Basis * Vector3.Forward);
		//Vector3 pidOut = pid.Update(parent.AngularVelocity, desired, (float) delta);
		Vector3 angularDiff = -parent.AngularVelocity - angularDesired;

		float rotationThrottle = angularDiff.Dot(parent.Basis * GetAngularAccel());

		return rotationThrottle;
	}

	float LinearControl()
	{
		Vector3 linearDiff = linearDesired - parent.LinearVelocity;

		float linearThrottle = linearDiff.Dot(parent.Basis * ThrustForwardVector());
		
		return linearThrottle;
	}

	/// <summary>
	/// Sets local desired angular velocity for PID control.
	/// </summary>
	public void SetDesiredAngularVelocity(Vector3 vel)
	{
		angularDesired = vel;
	}

	public void SetDesiredLinearVelocity(Vector3 vel)
	{
		linearDesired = vel;
	}

	public Vector3 ThrustForwardVector()
	{
		return Basis * Vector3.Forward;
	}

	public Vector3 GetForce()
	{
		return ThrustForwardVector() * MaximumThrust;
	}

	public Vector3 GetTorque()
	{
		return (Position - parent.CenterOfMass).Cross(GetForce());
	}

	public Vector3 GetAngularAccel()
	{
		return GetTorque() / PhysicsServer3D.BodyGetDirectState(parent.GetRid()).InverseInertia.Inverse();
	}

	#region boilerplate

	public override ThrusterBlock Init(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
	{
		ThrusterBlock block = FromCubeBlock(base.Init(subTypeId, blockData));
		
		try
		{
			block.MaximumThrust = blockData["ThrusterStrength"].As<float>();
		}
		catch
		{
			GD.PrintErr($"Missing [ThrusterStrength] in {subTypeId}! Setting to default...");
		}

		// ThrustNode shows where particles should be emitted
		foreach (var node in block.meshes)
		{
			if (node.Name.ToString().StartsWith("ThrustNode"))
			{
				block.thrustNode = node;
				break;
			}
		}
		if (block.thrustNode == null)
			throw new($"{subTypeId} missing ThrustNode!");

		return block;
	}

	public ThrusterBlock FromCubeBlock(CubeBlock c)
	{
		ThrusterBlock block = new()
		{
			collision = c.collision,
			meshes = c.meshes,
			subTypeId = c.subTypeId,
			size = c.size,
			Name = c.Name
		};

		foreach (var child in c.GetChildren())
		{
			c.RemoveChild(child);
			block.AddChild(child);
		}

		return block;
	}

	public override ThrusterBlock Copy()
	{
		ThrusterBlock block = FromCubeBlock(base.Copy());

		block.ThrustPercent = ThrustPercent;
		block.MaximumThrust = MaximumThrust;
		block.thrustNode = thrustNode;

		return block;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		particles.QueueFree();
	}

	#endregion
}
