using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading;


public partial class GameScene : Node3D
{
	public bool isActive = false;

	public readonly List<CubeGrid> grids = new();

	public player_character playerCharacter;


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

		if (Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			GetParent<menus>()._SwitchMenu(3);
			Visible = true;
		}
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
			playerCharacter.HUD.Visible = true;
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			ProcessMode = ProcessModeEnum.Disabled;
			playerCharacter.HUD.Visible = false;
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

	public void SpawnPremadeGrid(CubeGrid grid)
	{
		AddChild(grid);
		grids.Add(grid);

		GD.Print("Spawned existing grid " + grid.Name + " @ " + grid.Position);
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

				if (grid.IsEmpty())
				{
					grid.Close();
					grids.Remove(grid);
					RemoveChild(grid);
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

	public void Save()
	{
		GD.Print("\n\nStart world save...");

		Godot.Collections.Dictionary<string, Variant> data = new()
		{
			{
				"PlayerCharacter", new Godot.Collections.Dictionary<string, Variant>()
				{
					{ "Position", JsonHelper.StoreVec(playerCharacter.GlobalPosition) },
					{ "Rotation", JsonHelper.StoreVec(playerCharacter.GlobalRotation) },
				}
			},
		};
		GD.Print("Saved data for player.");

		Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> allGridsData = new();

		foreach (CubeGrid grid in grids)
			allGridsData.Add(grid.Save());

		data.Add("Grids", allGridsData);

	 	string str = Json.Stringify(data);
		GD.Print($"Saved grid data for {grids.Count} grids." + str);

		WorldLoader.SaveWorld(WorldLoader.CurrentSave, str);
	}

	public void Close()
	{
		foreach (var grid in grids)
			grid.Close();

		grids.Clear();
		GD.Print("Closed all grids in GameScene.");

		playerCharacter = null;
		GD.Print("Closed player in GameScene.\nFinished closing!\n\n");

		GetTree().ReloadCurrentScene();
	}
}
