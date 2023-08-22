using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;


public class CubeBlockLoader
{

	private static readonly Dictionary<string, CubeBlock> CubeBlocks = new();
	private static readonly Dictionary<string, Texture2D> CubeBlockTextures = new();

	public static void StartLoad(string path)
	{
		GD.Print("\n\nStart CubeBlock load from " + path);

		if (path[^1] != '/')
			path += '/';

		List<string> allDataFiles = FileHelper.FindFilesWithExtension(path, ".json", true);
		
		foreach (var filePath in allDataFiles)
		{
			try
			{
				LoadCubeBlockFromPath(filePath);
			}
			catch (Exception e)
			{
				GD.PrintErr(e);
			}
		}
		
		GD.Print($"Loaded {CubeBlocks.Count} CubeBlocks.\n\n");
	}

	public static void Clear()
	{
		CubeBlocks.Clear();
	}

	public static CubeBlock FromId(string id)
	{
		if (CubeBlocks.ContainsKey(id))
		{
			return CubeBlocks[id];
		}
		else
		{
			GD.PrintErr("Missing CubeBlock " + id);
			return CubeBlocks["ArmorBlock"];
		}
	}

	// Don't want to copy the texture for every single cubeblock
	public static Texture2D GetTexture(string subTypeId)
	{
		if (CubeBlocks.ContainsKey(subTypeId))
			return CubeBlockTextures[subTypeId];
		else
		{
			GD.PrintErr("Missing CubeBlock icon for " + subTypeId);
			return TextureLoader.Get("missing.png");
		}
	}

	private static void LoadCubeBlockFromPath(string path)
	{
		Json json = new ();
		FileAccess infoFile = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		if (json.Parse(infoFile.GetAsText()) != Error.Ok)
			throw new Exception("Unable to load CubeBlock @ " + path + " - " + json.GetErrorMessage());

		var allData = json.Data.AsGodotDictionary<string, Godot.Collections.Dictionary<string, Variant>>();

		foreach (var subTypeId in allData.Keys)
		{
			if (CubeBlocks.ContainsKey(subTypeId))
				continue;
				
			Texture2D texture = TextureLoader.Get("missing.png");
			Type type = typeof(CubeBlock);

			var blockData = allData[subTypeId];

			// Find image from ImageLoader
			try
			{
				texture = TextureLoader.Get((string) blockData["Icon"]);
			}
			catch
			{
				GD.PrintErr($"Missing [Icon] in {path}! Setting to default...");
			}

			try
			{
				Assembly asm = typeof(CubeBlock).Assembly;
				type = asm.GetType((string) blockData["TypeId"]);
			}
			catch
			{
				GD.PrintErr($"Missing [Type] in {path}! Setting to default...");
			}


			CubeBlockTextures.Add(subTypeId, texture);

			dynamic cube = Activator.CreateInstance(type);
			
			if (type == typeof(CubeBlock) || type.IsSubclassOf(typeof(CubeBlock))) {
				try
				{
					CubeBlocks.Add(subTypeId, cube.Init(subTypeId, blockData));
					GD.Print("Loaded block \"" + subTypeId + "\", typeof " + type.FullName + ".");
				}
				catch (Exception e)
				{
					GD.PrintErr($"Block {subTypeId} failed to init!\n" + e.Message);
				}
			}
			else
			{
				GD.PrintErr($"Type {type.Name} does not inherit CubeBlock!");
			}
		}
	}

	public static CubeBlock LoadFromData(Godot.Collections.Dictionary<string, Variant> data)
	{
		CubeBlock block = CubeBlocks[data["SubTypeId"].AsString()];
		block.Position = data["Position"].AsVector3();
		block.Rotation = data["Position"].AsVector3();

		return block;
	}
}
