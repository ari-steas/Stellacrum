using Godot;
using System;

public partial class PlaceBox : Node3D
{
	public string CurrentBlockId { get; private set; }= "";
	public bool IsHoldingBlock { get; private set; } = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		DebugDraw.Text3D("Forward", ToGlobal(Vector3.Forward*1.25f), 0, Colors.Green);
		DebugDraw.Text3D("Right", ToGlobal(Vector3.Right*1.25f), 0, Colors.Red);
		DebugDraw.Text3D("Up", ToGlobal(Vector3.Up*1.25f), 0, Colors.Blue);
	}

	public void SetBlock(string subTypeId)
	{
		// Prevents lag on double-tap
		if (subTypeId == CurrentBlockId)
			return;
		CurrentBlockId = subTypeId;

		IsHoldingBlock = false;

		// If not showing block, don't try to show block...
		if (subTypeId == "")
		{
			IsHoldingBlock = false;
			Visible = false;
			return;
		}
		else
		{
			Visible = true;
			IsHoldingBlock = true;
		}

		// Remove existing block
		foreach (var child in GetChildren())
			RemoveChild(child);

		// Pull mesh from CubeBlockLoader
		foreach (var mesh in CubeBlockLoader.FromId(subTypeId).meshes)
			AddChild(mesh.Duplicate());

		// Make mesh semi-transparent
		foreach (var node in GetChildren())
		{
			if (node is MeshInstance3D mesh)
			{
				mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
				mesh.Transparency = 0.5f;
			}
		}
		GD.Print("Set held block to " + subTypeId);
	}
}
