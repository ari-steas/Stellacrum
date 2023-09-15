using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public class WorldSave
{
	public string Name = "ERROR";
	public string Description = "";
	public DateTime CreationDate { get; private set; } = DateTime.MinValue;
	public DateTime ModifiedDate { get; private set; } = DateTime.MinValue;
	public float Size { get; private set; } = 0;
	public string Path = "";
	public Texture2D Thumbnail;

	public Godot.Collections.Dictionary<string, Variant> playerData;
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

		if (!Path.EndsWith('/'))
			Path += '/';
	}
	
	public WorldSave()
	{
		
	}

	public void SetName(string newName)
	{
		Name = newName;

		string newPath = Path;
		if (newPath.EndsWith("/"))
			newPath = Path[..^1];
		newPath = newPath[..newPath.LastIndexOf('/')] + "/" + newName + "/";
        GD.PrintErr(newPath);

        DirAccess.RenameAbsolute(Path, newPath);
		Path = newPath;

		UpdateInfo();

		GD.Print("Set new worldname " + newName);
	}

	public void SetDescription(string newDescription)
	{
        Description = newDescription;

        UpdateInfo();

        GD.Print("Set new worldDesc " + newDescription);
    }

	public void UpdateInfo(bool updateImage = false)
	{
		UpdateInfo(Json.Stringify(new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "Name", Name },
			{ "Description", Description },
		}));

		if (updateImage)
			Thumbnail.GetImage().SavePng(Path + "thumb.png");
	}

	public void UpdateInfo(string data)
	{
        GD.Print("Writing saveInfo to " + Path + "info.json");
        DirAccess.RemoveAbsolute(Path + "info.json");

        FileAccess worldSaveInfo = FileAccess.Open(Path + "info.json", FileAccess.ModeFlags.Write);

        if (worldSaveInfo == null)
        {
            GD.PrintErr(FileAccess.GetOpenError());
            return;
        }

        worldSaveInfo.StoreString(data);

        worldSaveInfo.Close();
    }

	public void UpdateData(string data)
	{
		GD.Print("Writing save to " + Path + "world.scw");
		DirAccess.RemoveAbsolute(Path + "world.scw");

		FileAccess worldSaveData = FileAccess.Open(Path + "world.scw", FileAccess.ModeFlags.Write);

		if (worldSaveData == null)
		{
			GD.PrintErr(FileAccess.GetOpenError());
			return;
		}

		worldSaveData.StoreString(data);

		worldSaveData.Close();
	}

	public static void Create(WorldSave baseSave)
	{
		DirAccess.MakeDirAbsolute(baseSave.Path);

		FileAccess infoFile = FileAccess.Open(baseSave.Path + "info.json", FileAccess.ModeFlags.Write);

		infoFile.StoreString(Json.Stringify(
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "Name", baseSave.Name },
				{ "Description", baseSave.Description },
			}
		));

		infoFile.Close();

		GD.Print("Saved new worldinfo to " + baseSave.Path);
	}

	public void ResetData()
	{
		playerData = null;
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
