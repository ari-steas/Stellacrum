using GameSceneObjects;
using Godot;
using Stellacrum.Data.CubeGridHelpers;
using Stellacrum.Data.CubeObjects;
using Stellacrum.Data.ObjectLoaders;
using System;
using System.Collections.Generic;


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

        //GDScript script = GD.Load<GDScript>("user://Mods/test.gd");
		//Node n = new();
		//n.SetScript(script);
		//AddChild(n);
    }

	long tick = 0;
	public override void _PhysicsProcess(double delta)
	{
		tick++;

        if (playerCharacter.GlobalPosition.Length() > 2000)
            ShiftOrigin();

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


    public CubeGrid? SpawnGridWithBlock(string blockId, Vector3 position, Basis rotation, bool force = false, Node parent = null)
    {
        if (!force && !IsShapeEmpty(position, CubeBlockLoader.BlockFromId(blockId).collision))
            return null;

        CubeGrid newGrid = new()
        {
            Position = position,
            Rotation = rotation.GetEuler()
        };

        newGrid.AddBlock(Vector3I.Zero, Basis.Identity, blockId);

        if (parent == null)
            AddChild(newGrid);
        else
            parent.CallDeferred(Node.MethodName.AddChild, newGrid);
        grids.Add(newGrid);
        newGrid.Name = "CubeGrid." + GetIndex();

        GD.Print("Spawned grid " + newGrid.Name + " @ " + newGrid.Position);
        return newGrid;
    }

    public void SpawnPremadeGrid(CubeGrid grid)
	{
		CallDeferred(Node.MethodName.AddChild, grid);
		grids.Add(grid);

		GD.Print("Spawned existing grid " + grid.Name + " @ " + grid.Position);
	}

	public void TryPlaceBlock(string blockId, RayCast3D cast, Basis rotation)
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
	public static bool TryGetGridAt(RayCast3D cast, out CubeGrid grid)
    {
        grid = null;
        if (cast.IsColliding())
        {
            if (cast.GetCollider() is CubeGrid g)
            {
                grid = g;
                return true;
            }
        }
            
		return false;
	}

    public static bool TryGetGridAt(ShapeCast3D cast, out CubeGrid grid, int index = 0)
    {
        grid = null;
        if (cast.IsColliding())
        {
            if (cast.GetCollider(index) is CubeGrid g)
            {
                grid = g;
                return true;
            }
        }
            
        return false;
    }

    public static bool TryGetBlockAt(RayCast3D cast, out CubeBlock block)
    {
        block = null;
		return TryGetGridAt(cast, out CubeGrid grid) && grid.TryGetBlockAt(cast, out block);
	}

    public static bool TryGetBlockAt(ShapeCast3D cast, out CubeBlock block, int index = 0)
    {
        block = null;
        return TryGetGridAt(cast, out CubeGrid grid) && grid.TryGetBlockAt(cast, out block);
    }

    public static void RemoveBlock(RayCast3D ray)
	{
		if (ray.IsColliding())
			if (ray.GetCollider() is CubeGrid grid)
				grid.RemoveBlock(ray);
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

        WorldLoader.CurrentSave.Thumbnail = GetViewport().GetTexture();
		GD.Print("Updated thumbnail");

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

	public static GameScene? GetGameScene(Node node)
	{

        Node parent = node.GetParent();
        if (parent == null)
            return null;

		if (parent is menus m)
			return m.GetChild<GameScene>(m.GetChildCount()-1);

        if (parent is GameScene gameScene2)
            return gameScene2;
        else
            return GetGameScene(parent);
    }

	public void Close()
	{
		foreach (var grid in grids.ToArray())
			grid.Close();

		grids.Clear();
		GD.Print("Closed all grids in GameScene.");

        GridMultiBlockStructure.ClearStructureTypes();
        GD.Print("Closed all GridMultiBlockStructures.");

        playerCharacter = null;
		GD.Print("Closed player in GameScene.\nFinished closing!\n\n");

        CubeBlockLoader.Clear();
        ModelLoader.Clear();
        TextureLoader.Clear();
		ProjectileDefinitionLoader.Clear();

        GetTree().ReloadCurrentScene();
	}
}
