using Godot;
using System;

public partial class PlaceBox : Node3D
{
	public string CurrentBlockId { get; private set; }= "";
	public bool IsHoldingBlock { get; private set; } = false;

	GpuParticles3D MirrorParticlesX, MirrorParticlesY, MirrorParticlesZ;
	public bool IsPlacingMirror = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		MirrorParticlesX = FindChild("MirrorParticlesX") as GpuParticles3D;
		MirrorParticlesY = FindChild("MirrorParticlesY") as GpuParticles3D;
		MirrorParticlesZ = FindChild("MirrorParticlesZ") as GpuParticles3D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Visible)
			return;
		DebugDraw.Text3D("Forward", ToGlobal(Vector3.Forward*1.25f), 0, Colors.Green);
		DebugDraw.Text3D("Right", ToGlobal(Vector3.Right*1.25f), 0, Colors.Red);
		DebugDraw.Text3D("Up", ToGlobal(Vector3.Up*1.25f), 0, Colors.Blue);
	}

	/// <summary>
	/// Display a mirror plane on a grid at position. AddToGrid controls whether or not the grid retains the mirror.
	/// </summary>
	/// <param name="grid"></param>
	/// <param name="position"></param>
	/// <param name="mode"></param>
	/// <param name="addToGrid"></param>
	public void SetMirror(CubeGrid grid, Vector3I position, MirrorMode mode, bool addToGrid = false)
	{
		if (addToGrid)
			grid.SetMirror(position, mode);
		
		switch (mode)
		{
			case MirrorMode.X:
				MirrorParticlesX.GetParent().RemoveChild(MirrorParticlesX);
				grid.AddChild(MirrorParticlesX);
				MirrorParticlesX.Position = grid.GridToLocalCoordinates(position);
				MirrorParticlesX.Emitting = true;
				break;
			case MirrorMode.Y:
				MirrorParticlesY.GetParent().RemoveChild(MirrorParticlesY);
				grid.AddChild(MirrorParticlesY);
				MirrorParticlesY.Position = grid.GridToLocalCoordinates(position);
				MirrorParticlesY.Emitting = true;
				break;
			case MirrorMode.Z:
				MirrorParticlesZ.GetParent().RemoveChild(MirrorParticlesZ);
				grid.AddChild(MirrorParticlesZ);
				MirrorParticlesZ.Position = grid.GridToLocalCoordinates(position);
				MirrorParticlesZ.Emitting = true;
				break;
			case MirrorMode.HalfX:
				MirrorParticlesX.GetParent().RemoveChild(MirrorParticlesX);
				grid.AddChild(MirrorParticlesX);
				MirrorParticlesX.Position = grid.GridToLocalCoordinates(position) + new Vector3(1.25f, 0.0f, 0.0f);
				MirrorParticlesX.Emitting = true;
				break;
			case MirrorMode.HalfY:
				MirrorParticlesY.GetParent().RemoveChild(MirrorParticlesY);
				grid.AddChild(MirrorParticlesY);
				MirrorParticlesY.Position = grid.GridToLocalCoordinates(position) + new Vector3(0.0f, 1.25f, 0.0f);
				MirrorParticlesY.Emitting = true;
				break;
			case MirrorMode.HalfZ:
				MirrorParticlesZ.GetParent().RemoveChild(MirrorParticlesZ);
				grid.AddChild(MirrorParticlesZ);
				MirrorParticlesZ.Position = grid.GridToLocalCoordinates(position) + new Vector3(0.0f, 0.0f, 1.25f);
				MirrorParticlesZ.Emitting = true;
				break;
		}
	}

	/// <summary>
	/// Completely removes a mirror plane from a grid.
	/// </summary>
	/// <param name="grid"></param>
	/// <param name="mode"></param>
	public void RemoveMirror(CubeGrid grid, MirrorMode mode)
	{
		grid.RemoveMirror(mode);

		switch (mode)
		{
			case MirrorMode.X:
			case MirrorMode.HalfX:
				MirrorParticlesX.Emitting = false;
				break;
			case MirrorMode.Y:
			case MirrorMode.HalfY:
				MirrorParticlesY.Emitting = false;
				break;
			case MirrorMode.Z:
			case MirrorMode.HalfZ:
				MirrorParticlesZ.Emitting = false;
				break;
		}
	}

	public void HideMirrors()
	{
		// Only disables visibility, doesn't really matter where they are
		MirrorParticlesX.Visible = false;
		MirrorParticlesY.Visible = false;
		MirrorParticlesZ.Visible = false;

		MirrorParticlesX.Emitting = false;
		MirrorParticlesY.Emitting = false;
		MirrorParticlesZ.Emitting = false;
	}

	public void ShowMirrors(CubeGrid grid)
	{
		MirrorParticlesX.Visible = true;
		MirrorParticlesY.Visible = true;
		MirrorParticlesZ.Visible = true;

		// Attach all mirrors to the grid
		foreach (var mirror in grid.GridMirrors)
			SetMirror(grid, mirror.Value, mirror.Key, false);
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

	public void SnapLocal()
	{
		Rotation = Rotation.Snapped(Vector3.One*nd);

		//Vector3 rotation = Rotation;
		//Vector3 mod = rotation % nd;
		

		// Attempts to round to closest snap rotation
		//if (mod.X > nd/2)
		//	rotation.X += nd - mod.X;
		//else
		//	rotation.X -= mod.X;
		//
		//if (mod.Y > nd/2)
		//	rotation.Y += nd - mod.Y;
		//else
		//	rotation.Y -= mod.Y;
		//
		//if (mod.Z > nd/2)
		//	rotation.Z += nd - mod.Z;
		//else
		//	rotation.Z -= mod.Z;
		//
		//Rotation = rotation;
	}

	private const float nd = Mathf.Pi/2;

	public enum MirrorMode
	{
		X,
		Y,
		Z,
		HalfX,
		HalfY,
		HalfZ
	}
}
