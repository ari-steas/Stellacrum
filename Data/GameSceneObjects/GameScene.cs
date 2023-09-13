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
		if(playerCharacter.GlobalPosition.Length() > 2000)
			ShiftOrigin();
	}

	int tick = 0;
	public override void _PhysicsProcess(double delta)
	{
		tick++;

		DebugDraw.Text(grids.Count + " CubeGrids");

		if (Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			GetParent<menus>()._SwitchMenu(3);
			Visible = true;
		}

		if (Input.IsActionPressed("BlockInventory"))
		{
			Input.ActionRelease("BlockInventory");
			GetParent<menus>()._SwitchMenu(5);
			Visible = true;
			playerCharacter.HUD.Visible = true;
		}

		if (Input.IsActionJustPressed("DebugStop"))
		{
			foreach (var grid in grids)
			{
				grid.LinearVelocity = Vector3.Zero;
				grid.AngularVelocity = Vector3.Zero;
			}
		}

		if (tick == 100)
		{
			List<CubeGrid> emptyGrids = new();

			foreach (var grid in grids)
			{
				if (grid.GetChildCount() == 0)
				{
					emptyGrids.Add(grid);
					grid.QueueFree();
				}
			}

			foreach (var grid in emptyGrids)
				grids.Remove(grid);

			tick = 0;
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

			if (isPlayerDataSet)
			{
				GD.Print("Delayed-set playerCharacterData");
				player_character.Load(ref playerCharacter, bufferPlayerData);
				isPlayerDataSet = false;
				bufferPlayerData = null;
			}
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

		// Have to convert global to local coordinates, because Issues:tm:
		SpawnGridWithBlock(blockId, ToLocal(cast.ToGlobal(cast.TargetPosition)), rotation);
	}

	#nullable enable
	public CubeGrid? GetGrid(RayCast3D cast)
	{
		if (cast.IsColliding())
		{
			if (cast.GetCollider() is CubeGrid grid)
			{
				return grid;
			}
		}
		return null;
	}

	public void RemoveBlock(RayCast3D ray)
	{
		if (ray.IsColliding())
		{
			if (ray.GetCollider() is CubeGrid grid)
			{
				grid.RemoveBlock(ray);

				if (grid.IsEmpty())
					grid.Close();
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

	private void ShiftOrigin()
	{
		GlobalPosition -= playerCharacter.GlobalPosition;
	}



	public void Save()
	{
		GD.Print("\n\nStart world save...");

		Godot.Collections.Dictionary<string, Variant> data = new();
		Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> allGridsData = new();

		data.Add("PlayerCharacter", playerCharacter.Save());

		foreach (CubeGrid grid in grids)
			allGridsData.Add(grid.Save());

		data.Add("Grids", allGridsData);

	 	string str = Json.Stringify(data);
		GD.Print($"Saved grid data for {grids.Count} grids.");

		WorldLoader.SaveWorld(WorldLoader.CurrentSave, str);
	}


    Godot.Collections.Dictionary<string, Variant> bufferPlayerData = new();
	bool isPlayerDataSet = false;
	public void SetPlayerData(Godot.Collections.Dictionary<string, Variant> bufferPlayerData)
	{
		this.bufferPlayerData = bufferPlayerData;
		isPlayerDataSet = true;

		GD.Print("Pre-setting player data...");
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
