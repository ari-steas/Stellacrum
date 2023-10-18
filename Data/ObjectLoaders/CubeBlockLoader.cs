using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FileAccess = Godot.FileAccess;

public class CubeBlockLoader
{

    //private static readonly Dictionary<string, CubeBlock> CubeBlocks = new();
	private static readonly Dictionary<string, Texture2D> CubeBlockTextures = new();
    private static Dictionary<string, Type> typeIds = new();
    private static Dictionary<string, Godot.Collections.Dictionary<string, Variant>> blockDefinitions = new();
	private static readonly Dictionary<string, CubeBlock> baseBlocks = new();

    public static void StartLoad(string path)
	{
		GD.Print("\n\nStart CubeBlock load from " + path);

        blockDefinitions.Clear();
        baseBlocks.Clear();
        typeIds.Clear();
        CubeBlockTextures.Clear();

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
		
		GD.Print($"Loaded {typeIds.Count} CubeBlocks.\n\n");
	}

	public static void Clear()
	{
		CubeBlockTextures.Clear();
        typeIds.Clear();
        blockDefinitions.Clear();
    }

	public static string[] GetAllIds()
	{
		return typeIds.Keys.ToArray();
	}

	public static CubeBlock BaseFromId(string id)
	{
		if (typeIds.ContainsKey(id))
		{
			return DefinitionLoader(id);
		}
		else
		{
			GD.PrintErr("Missing CubeBlock " + id);
			return baseBlocks["ArmorBlock"];
		}
	}

	/// <summary>
	/// Returns a 'base' static CubeBlock. Primarily used in PlaceBox.cs
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static CubeBlock ExistingBaseFromId(string id)
	{
        if (typeIds.ContainsKey(id))
        {
            return baseBlocks[id];
        }
        else
        {
            GD.PrintErr("Missing CubeBlock " + id);
            return DefinitionLoader("ArmorBlock");
        }
    }

	// Don't want to copy the texture for every single cubeblock
	public static Texture2D GetTexture(string subTypeId)
	{
		if (typeIds.ContainsKey(subTypeId))
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
			if (typeIds.ContainsKey(subTypeId))
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
			
			if (type == typeof(CubeBlock) || type.IsSubclassOf(typeof(CubeBlock)))
			{
				CubeBlockTextures.Add(subTypeId, texture);
				typeIds.Add(subTypeId, type);
				blockDefinitions.Add(subTypeId, blockData);

				GD.Print("Loaded block \"" + subTypeId + "\", typeof " + type.FullName + ".");
			}
			else
			{
				GD.PrintErr($"Type {type.Name} does not inherit CubeBlock!");
			}

			baseBlocks.Add(subTypeId, DefinitionLoader(subTypeId));
		}
	}

	public static CubeBlock LoadFromData(Godot.Collections.Dictionary<string, Variant> data)
	{
		GD.PrintErr("TODO CubeBlockLoader.cs LoadFromData for inherited types");
		CubeBlock block = BaseFromId(data["SubTypeId"].AsString());
		block.Position = JsonHelper.LoadVec(data["Position"]);
		block.Rotation = JsonHelper.LoadVec(data["Rotation"]);

		return block;
	}

	/// <summary>
	/// Creates new CubeBlock from stored definition.
	/// </summary>
	/// <param name="subTypeId"></param>
	/// <returns></returns>
	/// <exception cref="MissingMemberException"></exception>
	private static CubeBlock DefinitionLoader(string subTypeId)
	{
		if (!typeIds.ContainsKey(subTypeId))
			throw new MissingMemberException();

        Type type = typeIds[subTypeId];

        var blockData = blockDefinitions[subTypeId];

        dynamic cube = Activator.CreateInstance(type, args: new object[] { subTypeId, blockData});

		return cube;
    }
}
