using Godot;
using System;

public partial class PathFollow3D : Godot.PathFollow3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ProgressRatio += 10*(float) delta;
		if (ProgressRatio > 1)
			ProgressRatio = 1;
	}
}
