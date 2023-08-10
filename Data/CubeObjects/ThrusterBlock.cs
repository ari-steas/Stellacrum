using Godot;
using System;
using System.Collections.Generic;

public partial class ThrusterBlock : CubeBlock
{
    CubeGrid parent;
    public float ThrustPercent = 0;
    public float MaximumThrust = 0;

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

        return block;
    }

    public override void _Ready()
    {
        parent = GetParent<CubeGrid>();
    }

    public override void _Process(double delta)
	{
        float pct = parent.MovementInput.Dot(Transform.Basis * Vector3.Forward);
        if (pct >= 0)
            ThrustPercent = pct;
	}

    public override void _PhysicsProcess(double delta)
    {
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

        return block;
    }
}
