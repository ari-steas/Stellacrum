using Godot;
using Stellacrum.Data.CubeObjects;
using System;

public partial class CockpitBlock : CubeBlock
{
    public CockpitBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
    {
    }

	public CockpitBlock() { }

    public override void _Process(double delta)
    {
        
    }
}
