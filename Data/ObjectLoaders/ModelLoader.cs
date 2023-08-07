using Godot;
using System;
using System.Collections.Generic;

public class ModelLoader
{
    public static readonly Dictionary<string, List<MeshInstance3D>> Models = new ();

    public static void StartLoad(string path)
	{
		GD.Print("\n\nStart Model load from " + path);

		if (path[^1] != '/')
			path += '/';

		List<string> allModels = FileHelper.FindFilesWithExtension(path, ".glb");

		foreach (var model in allModels)
		{
			int slashIndex = model.LastIndexOf('/');
			string name = model.Substring(slashIndex == -1 ? 0 : slashIndex, model.LastIndexOf('.'));
			if (Models.ContainsKey(name))
			    continue;

			try
			{
				Models.Add(name, UnPackScene(GD.Load<PackedScene>(path + model)));
				GD.Print("Loaded model \"" + name + "\".");
			}
			catch (Exception e)
			{
				GD.PrintErr("Failed to load model \"" + name + "\"!\n" + e.Message);
			}
		}

		GD.Print($"Loaded {Models.Count} Models.");
	}

	public void Clear()
	{
		Models.Clear();
	}

    private static List<MeshInstance3D> UnPackScene(PackedScene p)
	{
		List<MeshInstance3D> meshes = new ();

		foreach (var node in p.Instantiate().GetChildren())
			if (node is MeshInstance3D d)
				meshes.Add(d);

		return meshes;
	}
}
