using Godot;
using System;
using System.Collections.Generic;

public class WorldSave
{
	public string Name { get; private set; } = "ERROR";
	public string Description { get; private set; } = "ERROR";
	public DateTime CreationDate { get; private set; } = DateTime.MinValue;
	public DateTime ModifiedDate { get; private set; } = DateTime.MinValue;
	public float Size { get; private set; } = 0;
	public string Path { get; private set; } = "";
	public Texture2D Thumbnail { get; private set; }

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

	public void Create(string name)
	{
		GD.PrintErr("TODO Create in WorldSave.cs");
	}
}
