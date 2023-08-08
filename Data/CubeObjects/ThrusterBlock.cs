using Godot;
using System;
using System.Collections.Generic;

public partial class ThrusterBlock : CubeBlock
{
    public static ThrusterBlock Create(CubeBlock c)
    {
        return (ThrusterBlock) c;
    }

    public override void _Process(double delta)
	{
        DebugDraw.Text("HI " + Name);
	}
}
