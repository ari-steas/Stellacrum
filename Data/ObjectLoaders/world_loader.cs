using Godot;
using System;
using System.Collections.Generic;
using System.Threading;


public class WorldLoader
{
	public static List<WorldSave> Worlds { get; private set; } = FindWorlds();
	public static WorldSave CurrentSave { get; private set; }

	public static List<WorldSave> FindWorlds()
	{
		List<WorldSave> bufferWorlds = new ();

		if (!DirAccess.DirExistsAbsolute("user://Saves/"))
			DirAccess.MakeDirAbsolute("user://Saves/");

		DirAccess d = DirAccess.Open("user://Saves/");

		foreach (var dir in d.GetDirectories())
			bufferWorlds.Add(GetSaveInfo(dir));

		if (CurrentSave == null)
			CurrentSave = new WorldSave();

		return bufferWorlds;
	}

	public static void SetWorld(int worldId)
	{
		if (worldId == -1)
		{
			CurrentSave = new WorldSave();
			return;
		}
		CurrentSave = Worlds[worldId];
	}

	public static void LoadWorld(GameScene scene)
	{
		ModelLoader.StartLoad("res://Assets/Models");
		//Thread textureThread = TextureLoader.StartLoad("res://Assets/Images/Blocks");

		//modelThread.Join();
		//textureThread.Join();

		CubeBlockLoader.StartLoad("res://Assets/CubeBlocks");

		LoadSaveData(CurrentSave);

		foreach (var grid in CurrentSave.grids)
			scene.SpawnPremadeGrid(grid);
		
		scene.SetPlayerData(CurrentSave.playerObject);
	}

	public static void SaveWorld(WorldSave save, string jsonData)
	{
		save.Update(jsonData);
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
		FileAccess infoFile = FileAccess.Open(save.Path + "/world.scw", FileAccess.ModeFlags.Read);
		if (infoFile == null)
			throw new Exception("Missing world.scw @ " + save.Path);
		if (json.Parse(infoFile.GetAsText()) != Error.Ok)
			throw new Exception("Invalid world.scw @ " + save.Path + " - " + json.GetErrorMessage());

		CurrentSave.ResetData();

		var worldData = json.Data.AsGodotDictionary<string, Variant>();

		if (worldData.ContainsKey("PlayerCharacter"))
			save.playerObject = SaveObject.FromDictionary(worldData["PlayerCharacter"].AsGodotDictionary<string, Variant>());
		else
		{
			GD.PrintErr("Missing PlayerCharacter from save!");
			save.playerObject = new(Vector3.Zero, Vector3.Zero);
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
}
