using Godot;
using Stellacrum.Data.CubeGridHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stellacrum.Data.CubeObjects;
using FileAccess = Godot.FileAccess;

namespace Stellacrum.Data.ObjectLoaders
{
	public class CubeBlockLoader
	{
		//private static readonly Dictionary<string, CubeBlock> CubeBlocks = new();
		private static readonly Dictionary<string, Texture2D> CubeBlockTextures = new();
		private static Dictionary<string, Type> typeIds = new();
		private static Dictionary<string, Godot.Collections.Dictionary<string, Variant>> blockDefinitions = new();
		private static readonly Dictionary<string, CubeBlock> baseBlocks = new();

		private static readonly List<Type> validBlockTypes = new();

		public static void StartLoad(string path)
		{
			GD.Print("\n\nStart CubeBlock load from " + path);

			blockDefinitions.Clear();
			baseBlocks.Clear();
			typeIds.Clear();
			CubeBlockTextures.Clear();
			validBlockTypes.Clear();

			GD.Print("\nValid TypeIds:");
            foreach (var item in Assembly.GetAssembly(typeof(CubeBlock)).GetTypes())
            {
				if (item.IsAssignableTo(typeof(CubeBlock)) || item == typeof(CubeBlock))
				{
					GD.Print("  " + item.Name);
					validBlockTypes.Add(item);
				}
            };
			GD.Print();

			if (path[^1] != '/')
				path += '/';

			List<string> allDataFiles = FileHelper.FindFilesWithExtension(path, ".json", true);

			foreach (var filePath in allDataFiles)
			{
				try
				{
					LoadCubeBlocksFromFile(filePath);
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

		public static CubeBlock BlockFromId(string id)
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

		private static void LoadCubeBlocksFromFile(string filePath)
		{
			Json json = new();
			FileAccess infoFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

			if (json.Parse(infoFile.GetAsText()) != Error.Ok)
				throw new Exception("Unable to load CubeBlock @ " + filePath + " - " + json.GetErrorMessage());

			// holy mother of Godot.Collections.Dictionary
			var allData = json.Data.AsGodotDictionary<string, Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>>();

			if (allData.ContainsKey("CubeBlockDefinitions"))
				foreach (var blockDefinition in allData["CubeBlockDefinitions"])
					LoadCubeBlock(blockDefinition.Key, blockDefinition.Value);
		}

		/// <summary>
		/// Loads a single CubeBlock from a definition.
		/// </summary>
		/// <param name="blockDefinition"></param>
		private static void LoadCubeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
		{
			if (typeIds.ContainsKey(subTypeId))
				return;

			Texture2D texture = TextureLoader.Get("missing.png");
			Type type = typeof(CubeBlock);

			// Find image from ImageLoader
			try
			{
				texture = TextureLoader.Get((string)blockData["Icon"]);
			}
			catch
			{
				GD.PrintErr($"Missing [Icon] in {subTypeId}! Setting to default...");
			}

			try
			{
				Assembly asm = typeof(CubeBlock).Assembly;
				foreach (var validType in validBlockTypes)
				{
					if (validType.Name == (string)blockData["TypeId"])
					{
						type = validType;
						break;
					}
				}
				//type = asm.GetType("Stellacrum.Data.CubeObjects." + (string)blockData["TypeId"]);
			}
			catch
			{
				GD.PrintErr($"Missing [Type] in {subTypeId}! Setting to default...");
			}

			if (type == null)
			{
				GD.PrintErr($"{subTypeId}'s Type ({blockData["TypeId"]}) is null!");
				return;
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

			baseBlocks.Add(subTypeId, DefinitionLoader(subTypeId, true));
		}

		public static CubeBlock LoadFromData(Godot.Collections.Dictionary<string, Variant> data)
		{
			GD.PrintErr("TODO CubeBlockLoader.cs LoadFromData for inherited types");
			CubeBlock block = BlockFromId(data["SubTypeId"].AsString());
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
		private static CubeBlock DefinitionLoader(string subTypeId, bool verbose = false)
		{
			if (!typeIds.ContainsKey(subTypeId))
				throw new MissingMemberException();

			Type type = typeIds[subTypeId];

			var blockData = blockDefinitions[subTypeId];

			dynamic cube = Activator.CreateInstance(type, args: new object[] { subTypeId, blockData, verbose });

			return cube;
		}
	}
}