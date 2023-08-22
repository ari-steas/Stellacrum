using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;


public class WorldSave
{
	public string Name { get; private set; } = "ERROR";
	public string Description { get; private set; } = "ERROR";
	public DateTime CreationDate { get; private set; } = DateTime.MinValue;
	public DateTime ModifiedDate { get; private set; } = DateTime.MinValue;
	public float Size { get; private set; } = 0;
	public string Path { get; private set; } = "";
	public Texture2D Thumbnail { get; private set; }

	public SaveObject playerObject;
	public List<CubeGrid> grids = new();

	public WorldSave(string name, string description, DateTime creationDate, DateTime modifiedDate, float size, string path, Texture2D thumbnail)
	{
		Name = name;
		Description = description;
		CreationDate = creationDate;
		ModifiedDate = modifiedDate;
		Size = size;
		Path = path;
		Thumbnail = thumbnail;
	}
	
	public WorldSave()
	{
		
	}

	public void Update(string data)
	{
		GD.PrintErr(data);

		GD.Print("Writing save to " + Path + "/world.scw");
		DirAccess.RemoveAbsolute(Path + "/world.scw");

		FileAccess worldSaveData = FileAccess.Open(Path + "/world.scw", FileAccess.ModeFlags.Write);

		if (worldSaveData == null)
		{
			GD.PrintErr(FileAccess.GetOpenError());
			return;
		}

		worldSaveData.StoreString(data);

		worldSaveData.Close();
	}

	public static void Create(string name)
	{
		GD.PrintErr("TODO Create in WorldSave.cs");
	}
}

public class SaveObject
{
	public Vector3 position;
	public Vector3 rotation;

	public SaveObject(Vector3 position, Vector3 rotation)
	{
		this.position = position;
		this.rotation = rotation;
	}

	public static SaveObject FromDictionary(Godot.Collections.Dictionary<string, Vector3> dict)
	{
		return new SaveObject(dict["Position"], dict["Rotation"]);
	}
}
