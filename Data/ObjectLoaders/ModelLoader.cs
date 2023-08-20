using Godot;
using System;
using System.Collections.Generic;
using System.Threading;


public class ModelLoader
{
    public static readonly Dictionary<string, List<Node3D>> Models = new ();

	#nullable enable
    public static Thread? StartLoad(string path, bool threaded = false)
	{
		if (threaded)
		{
			Thread t = new(Load);
			t.Start(path);
			return t;
		}

		Load(path);
		return null;
	}

	static void Load(object data)
	{
		if (data is string path)
		{
			GD.Print("\n\nStart Model load from " + path);

			List<string> allModels = FileHelper.FindFilesWithExtension(path, ".glb", true);

			foreach (var model in allModels)
			{
				GD.Print(model);
				int slashIndex = model.LastIndexOf('/') + 1;
				string name = model.Substring(slashIndex, model.LastIndexOf('.') - slashIndex);

				try
				{
					Models.Add(name, UnPackScene(GD.Load<PackedScene>(model)));
					GD.Print("Loaded model \"" + name + "\".");
				}
				catch (Exception e)
				{
					GD.PrintErr("Failed to load model \"" + name + "\"!\n" + e.Message);
				}
			}

			GD.Print($"Loaded {Models.Count} Models.");
		}
	}

	public void Clear()
	{
		Models.Clear();
	}

    private static List<Node3D> UnPackScene(PackedScene p)
	{
		List<Node3D> meshes = new ();

		foreach (var node in p.Instantiate().GetChildren())
			if (node is Node3D d)
				meshes.Add(d);

		return meshes;
	}
}
