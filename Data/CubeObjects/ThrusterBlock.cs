using Godot;
using System;
using System.Collections.Generic;

public partial class ThrusterBlock : CubeBlock
{
    public override ThrusterBlock Init(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
    {
        ThrusterBlock block = FromCubeBlock(base.Init(subTypeId, blockData));
        GD.Print("HAHA THURST");

        return block;
    }

    public override void _Process(double delta)
	{
        GetParent<CubeGrid>().LinearVelocity += GlobalTransform.Basis * Vector3.Forward;
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
}
