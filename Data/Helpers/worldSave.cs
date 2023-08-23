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

	public void ResetData()
	{
		playerObject = null;
		grids.Clear();
	}
}

public class SaveObject
{
	public Vector3 Position, Rotation, LinearVelocity, AngularVelocity;

	public SaveObject(Vector3 Position, Vector3 Rotation, Vector3 LinearVelocity = new(), Vector3 AngularVelocity = new())
	{
		this.Position = Position;
		this.Rotation = Rotation;
		this.LinearVelocity = LinearVelocity;
		this.AngularVelocity = AngularVelocity;
	}

	/// <summary>
	/// Create new SaveObject from Godot.Collections.Dictionary. Checks for:
	/// <list type="bullet">
	/// <item>Vector3 Position</item>
	/// <item>Vector3 Rotation</item>
	/// <item>Vector3 LinearVelocity (default Vector3.Zero)</item>
	/// <item>Vector3 AngularVelocity = (default Vector3.Zero)</item>
	/// </list>
	/// </summary>
	/// <param name="dict"></param>
	/// <returns></returns>
	public static SaveObject FromDictionary(Godot.Collections.Dictionary<string, Variant> dict)
	{
		SaveObject saveObject = new(JsonHelper.LoadVec(dict["Position"]), JsonHelper.LoadVec(dict["Rotation"]));

		if (dict.ContainsKey("LinearVelocity"))
			saveObject.LinearVelocity = JsonHelper.LoadVec(dict["LinearVelocity"]);
		if (dict.ContainsKey("AngularVelocity"))
			saveObject.AngularVelocity = JsonHelper.LoadVec(dict["AngularVelocity"]);

		return saveObject;
	}
}
