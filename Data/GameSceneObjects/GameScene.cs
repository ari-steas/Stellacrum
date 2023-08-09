using Godot;
using System;
using System.Collections.Generic;

public partial class GameScene : Node3D
{
	public bool isActive = false;

	private readonly List<CubeGrid> grids = new();

	private player_character playerCharacter;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		playerCharacter = GetNode<player_character>("PlayerCharacter");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		DebugDraw.Text(grids.Count + " CubeGrids");
	}

	private void _ToggleActive(bool active)
	{
		Visible = active;
		isActive = active;
		GD.Print("GameScene " + (active ? "started" : "stopped"));
		if (isActive)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			ProcessMode = ProcessModeEnum.Inherit;
		}
		else
		{
			ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	private void SpawnGridWithBlock(string blockId, Vector3 position, Vector3 rotation)
	{
		if (!IsShapeEmpty(position, CubeBlockLoader.FromId(blockId).collision))
			return;

		CubeGrid newGrid = new()
		{
			Position = position,
			Rotation = rotation,
		};
		newGrid.AddBlock(Vector3I.Zero, Vector3.Zero, blockId);

		AddChild(newGrid);
		grids.Add(newGrid);
		newGrid.Name = "CubeGrid." + GetIndex();

		GD.Print("Spawned grid " + newGrid.Name + " @ " + newGrid.Position);
	}

	public void TryPlaceBlock(string blockId, RayCast3D cast, Vector3 rotation)
	{
		if (cast.IsColliding())
		{
			if (cast.GetCollider() is CubeGrid grid)
			{
				grid.AddBlock(cast, rotation, blockId);
				return;
			}
		}

		SpawnGridWithBlock(blockId, cast.ToGlobal(cast.TargetPosition), rotation);
	}

	public void RemoveBlock(RayCast3D ray)
	{
		if (ray.IsColliding())
		{
			if (ray.GetCollider() is CubeGrid grid)
			{
				grid.RemoveBlock(ray);

				if (grid.size.Size.IsEqualApprox(Vector3.Zero))
				{
					grid.Close();
					grids.Remove(grid);
				}
			}
		}
	}

	public bool IsPointEmpty(Vector3 point)
	{
		RayCast3D ray = new()
		{
			Position = point,
			TargetPosition = point + Vector3.One*0.01f,
			HitFromInside = true
		};
		AddChild(ray);
		ray.ForceRaycastUpdate();
		RemoveChild(ray);

		if (ray.IsColliding())
			return false;
		return true;
	}

	public bool IsShapeEmpty(Vector3 point, Shape3D shape)
	{
		ShapeCast3D ray = new()
		{
			Position = point,
			TargetPosition = point + Vector3.One*0.01f,
			Shape = shape
		};
		AddChild(ray);
		ray.ForceShapecastUpdate();
		RemoveChild(ray);

		if (ray.IsColliding())
			return false;
		return true;
	}

	public void Close()
	{
		foreach (var grid in grids)
			grid.Close();

		GD.Print("Closed all grids in GameScene.");
	}

	private void _OnTreeExited()
	{
		Close();
	}
}
