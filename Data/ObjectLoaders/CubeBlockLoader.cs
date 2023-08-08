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

		List<string> allDataFiles = FileHelper.FindFilesWithExtension(path, ".json");
		
		foreach (var filePath in allDataFiles)
		{
			try
			{
				LoadCubeBlockFromPath(path + filePath);
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
			return CubeBlocks[id];
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
				
			List<MeshInstance3D> model = ModelLoader.Models["ArmorBlock1x1"];
			Vector3 size = Vector3.One*2.5f;
			Texture2D texture = TextureLoader.Get("missing.png");
			Type type = typeof(CubeBlock);

			var blockData = allData[subTypeId];

			// Load model from ModelLoader
			try
			{
				model = ModelLoader.Models[(string) blockData["Model"]];
			}
			catch
			{
				GD.PrintErr($"Missing [Model] in {path}! Setting to default...");
			}

			// Calc BlockSize
			try
			{
				int[] bSize = blockData["BlockSize"].AsInt32Array();
				size = new Vector3(bSize[0], bSize[1], bSize[2]) * 2.5f;
			}
			catch {
				GD.PrintErr($"Missing [Size] in {path}! Setting to default...");
			}

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

			var cube = Activator.CreateInstance(type);

			CubeBlockTextures.Add(subTypeId, texture);

			CubeBlocks.Add(subTypeId, (CubeBlock) cube);
			GD.Print("Loaded block \"" + subTypeId + "\", typeof " + cube.GetType().FullName + ".");
		}
	}
}
