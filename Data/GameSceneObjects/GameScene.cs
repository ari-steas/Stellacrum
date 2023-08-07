using Godot;
using System;
using System.Collections.Generic;

public partial class GameScene : Node3D
{
	public bool isActive = false;

	private readonly List<CubeGrid> grids = new();
	private readonly List<CubeGrid> gridsToRemove = new();

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
		foreach (var grid in gridsToRemove)
		{
			grids.Remove(grid);
			RemoveChild(grid);
		}
		gridsToRemove.Clear();
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

	private void SpawnGridWithBlock(Vector3 position, Vector3 rotation, string blockId)
	{
		if (!IsShapeEmpty(position, CubeBlockLoader.FromId(blockId).collision.Shape))
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
			if ((cast.GetCollider() as Node3D).GetParent() is CubeGrid grid)
			{
				grid.AddBlock(cast, rotation, blockId);
				return;
			}
		}

		SpawnGridWithBlock(cast.ToGlobal(cast.TargetPosition), rotation, blockId);
	}

	public void RemoveBlock()
	{
		if (playerCharacter.interactCast.IsColliding())
		{
			if ((playerCharacter.interactCast.GetCollider() as Node3D).GetParent() is CubeGrid grid)
			{
				grid.RemoveBlock(playerCharacter.interactCast);

				if (grid.size.Size.IsEqualApprox(Vector3.Zero))
					gridsToRemove.Add(grid);
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
}
