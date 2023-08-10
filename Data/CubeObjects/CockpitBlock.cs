using Godot;
using System;

public partial class CockpitBlock : CubeBlock
{
    public override CockpitBlock Init(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
	{
		CockpitBlock block = FromCubeBlock(base.Init(subTypeId, blockData));

		return block;
	}

    public override void _Process(double delta)
    {
        
    }

    public CockpitBlock FromCubeBlock(CubeBlock c)
	{
		CockpitBlock block = new()
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

	public override CockpitBlock Copy()
	{
		CockpitBlock block = FromCubeBlock(base.Copy());

		return block;
	}
}
