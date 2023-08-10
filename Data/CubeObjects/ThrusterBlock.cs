using Godot;
using System;
using System.Collections.Generic;

public partial class ThrusterBlock : CubeBlock
{
	CubeGrid parent;
	public float ThrustPercent = 0;
	public float MaximumThrust { get; private set; }= 0;
	private Node3D thrustNode;
	private GpuParticles3D particles;

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
			if (node.Name.ToString().Equals("ThrustNode"))
			{
				block.thrustNode = node;
				break;
			}
		}
		if (block.thrustNode == null)
			throw new($"{subTypeId} missing ThrustNode!");

		return block;
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
			Amount = 128,
			TransformAlign = GpuParticles3D.TransformAlignEnum.ZBillboardYToVelocity,
			Emitting = false
		};
		AddChild(particles);
	}

	public override void _Process(double delta)
	{
		float pct = parent.MovementInput.Dot(Transform.Basis * Vector3.Forward);
		if (pct > 0)
		{
			ThrustPercent = pct;
			particles.Emitting = true;
		}
		else
		{
			ThrustPercent = 0;
			particles.Emitting = false;
		}

		if (pct > 0.004)
				particles.Amount = (int) (128*pct);
			else
				particles.Emitting = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Make particles inherit velocity of parent
		particles.ProcessMaterial.Set("initial_velocity_min", parent.Speed + 30);
		particles.ProcessMaterial.Set("initial_velocity_max", parent.Speed + 40);

		parent.ApplyForce(GlobalTransform.Basis * Vector3.Forward * ThrustPercent * MaximumThrust * (float) delta, GlobalPosition - parent.GlobalPosition);
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
}
