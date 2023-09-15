using Godot;
using System;
using System.Collections.Generic;
using System.Threading;


public static class WorldLoader
{
	public static List<WorldSave> Worlds { get; private set; } = FindWorlds();
	public static WorldSave CurrentSave { get; private set; }

	private static List<WorldSave> FindWorlds()
	{
		List<WorldSave> bufferWorlds = new ();

		if (!DirAccess.DirExistsAbsolute("user://Saves/"))
			DirAccess.MakeDirAbsolute("user://Saves/");

		DirAccess d = DirAccess.Open("user://Saves/");

		foreach (var dir in d.GetDirectories())
		{
			try
			{
				bufferWorlds.Add(GetSaveInfo(dir));
			}
			catch (Exception e)
			{
				GD.PrintErr("Failed to find worldInfo!");
				GD.PrintErr(e);
			}
		}

		if (CurrentSave == null)
			CurrentSave = new WorldSave();

		return bufferWorlds;
	}

	public static void ScanWorlds()
	{
		Worlds = FindWorlds();
	}

	public static void SetWorld(int worldId)
	{
		if (Worlds.Count <= worldId)
			return;

		if (worldId == -1)
		{
			string uniqueName = CreateUniqueWorldName("New Save");
			CurrentSave = new WorldSave()
			{
				Path = $"user://Saves/{uniqueName}/",
				Name = uniqueName,
			};
			return;
		}
		CurrentSave = Worlds[worldId];
	}

	public static void Delete(WorldSave world)
	{
		if (Worlds.Contains(world))
		{
			GD.Print("Deleting world! " + world.Path);
			FileHelper.RecursiveDelete(world.Path);
			Worlds.Remove(world);
		}
	}

	public static string ErrorMessage { get; private set; } = "";
	public static void LoadWorld(GameScene scene)
	{
		ModelLoader.StartLoad("res://Assets/Models");
		//Thread textureThread = TextureLoader.StartLoad("res://Assets/Images/Blocks");

		//modelThread.Join();
		//textureThread.Join();

		CubeBlockLoader.StartLoad("res://Assets/CubeBlocks");

		if (!Worlds.Contains(CurrentSave))
		{
			WorldSave.Create(CurrentSave);
			Worlds.Add(CurrentSave);
		}

		try
		{
			LoadSaveData(CurrentSave);
		}
		catch
		{
			ErrorMessage = "Unable to read save file!";
		}
		

		foreach (var grid in CurrentSave.grids)
			scene.SpawnPremadeGrid(grid);
		
		scene.SetPlayerData(CurrentSave.playerData);
	}

	public static void SaveWorld(WorldSave save, string jsonData)
	{
		save.UpdateData(jsonData);
		save.UpdateInfo(true);
	}






	/// <summary>
	/// Reads info.json of world located at <paramref name="path">.
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	/// <exception cref="UriFormatException"></exception>
	/// <exception cref="Exception"></exception>
	private static WorldSave GetSaveInfo(string path)
	{
		path = "user://Saves/" + path;

		DirAccess dirAccess = DirAccess.Open(path);
		if (dirAccess == null)
			throw new UriFormatException("Unable to load WorldSave @ " + path);
				
		Json json = new();
		FileAccess infoFile = FileAccess.Open(path + "/info.json", FileAccess.ModeFlags.Read);
		if (json.Parse(infoFile.GetAsText()) != Error.Ok)
			throw new Exception("Invalid info.json @ " + path + " - " + json.GetErrorMessage());
		var infoData = json.Data.AsGodotDictionary<string, Variant>();

		string name = (string) infoData["Name"];
		DateTime creationDate = System.IO.Directory.GetCreationTimeUtc(infoFile.GetPathAbsolute());
		DateTime modifiedDate = System.IO.Directory.GetLastWriteTimeUtc(infoFile.GetPathAbsolute());
		string description = (string) infoData["Description"];
		float size = DirSize(new System.IO.DirectoryInfo(infoFile.GetPathAbsolute().Substring(0, infoFile.GetPathAbsolute().LastIndexOf('/'))))/1000;

		Texture2D thumbnail;

		if (FileAccess.FileExists(path + "/thumb.png"))
			thumbnail = ImageTexture.CreateFromImage(Image.LoadFromFile(path + "/thumb.png"));
		else
			thumbnail = TextureLoader.Get("missing.png");

		GD.Print("Read SaveInfo of \"" + name + "\" with description \"" + description + "\", created on " + creationDate.ToLongDateString());

		WorldSave save = new WorldSave(name, description, creationDate, modifiedDate, size, path, thumbnail);

		infoFile.Close();
		
		return save;
	}

	/// <summary>
	/// Reads world.scw of world <paramref name="save">.
	/// </summary>
	/// <param name="save"></param>
	/// <exception cref="UriFormatException"></exception>
	/// <exception cref="Exception"></exception>
	private static void LoadSaveData(WorldSave save)
	{
		DirAccess dirAccess = DirAccess.Open(save.Path);
		if (dirAccess == null)
			throw new UriFormatException("Unable to find WorldSave data @ " + save.Path);

		Json json = new();
		FileAccess dataFile = FileAccess.Open(save.Path + "/world.scw", FileAccess.ModeFlags.Read);
		if (dataFile == null)
			throw new Exception("Missing world.scw @ " + save.Path);
		if (json.Parse(dataFile.GetAsText()) != Error.Ok)
			throw new Exception("Invalid world.scw @ " + save.Path + " - " + json.GetErrorMessage());

		CurrentSave.ResetData();

		var worldData = json.Data.AsGodotDictionary<string, Variant>();

		if (worldData.ContainsKey("PlayerCharacter"))
			save.playerData = worldData["PlayerCharacter"].AsGodotDictionary<string, Variant>();
		else
		{
			GD.PrintErr("Missing PlayerCharacter from save!");
			save.playerData = null;
		}

		if (worldData.ContainsKey("Grids"))
		{
			// I HATE GODOT ARRAYS SO FUCKING MUCH
			// WORK OF THE DEVIL
			foreach (var grid in worldData["Grids"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
			{
				CubeGrid cubeGrid = GridFromData(grid);

				save.grids.Add(cubeGrid);
			}
		}

		dataFile.Close();
	}

	private static CubeGrid GridFromData(Godot.Collections.Dictionary<string, Variant> data)
	{
		CubeGrid grid = new()
		{
			Name = data["Name"].AsString(),
			Position = JsonHelper.LoadVec(data["Position"]),
			Rotation = JsonHelper.LoadVec(data["Rotation"]),
			LinearVelocity = JsonHelper.LoadVec(data["LinearVelocity"]),
			AngularVelocity = JsonHelper.LoadVec(data["AngularVelocity"]),
		};

		foreach (var block in data["Blocks"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
			grid.AddFullBlock(CubeBlockLoader.LoadFromData(block));

		return grid;
	}






	private static List<string> ScanDirectory(string directory)
	{
		List<string> allFiles = new();
		DirAccess dir = DirAccess.Open(directory);

		if (dir == null)
			return allFiles;

		allFiles.AddRange(dir.GetFiles());

		foreach (string subDirectory in dir.GetDirectories())
			allFiles.AddRange(ScanDirectory(subDirectory));

		return allFiles;
	}

	private static long DirSize(System.IO.DirectoryInfo d) 
	{
		long size = 0;    
		// Add file sizes.
		System.IO.FileInfo[] fis = d.GetFiles();
		foreach (System.IO.FileInfo fi in fis) 
		{      
			size += fi.Length;    
		}
		// Add subdirectory sizes.
		System.IO.DirectoryInfo[] dis = d.GetDirectories();
		foreach (System.IO.DirectoryInfo di in dis) 
		{
			size += DirSize(di);   
		}
		return size;
	}

	private static string CreateUniqueWorldName(string name, int iterations = 0)
	{
		foreach (var world in Worlds)
			if (world.Name == name + (iterations > 0 ? $" ({iterations})" : ""))
				return CreateUniqueWorldName(name, iterations + 1);

		return name + (iterations > 0 ? $" ({iterations})" : "");
	}
}
