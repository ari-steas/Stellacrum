using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


public class ModelLoader
{
    private static readonly Dictionary<string, PackedScene> Models = new ();

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

	public static List<Node3D>? GetModel(string id)
	{
		if (Models.ContainsKey(id))
			return UnPackScene(Models[id]);
		return null;
	}

	static void Load(object? data)
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
					PackedScene p = GD.Load<PackedScene>(model);
					p.ResourceLocalToScene = true;
                    Models.Add(name, p);
					GD.Print("Loaded model \"" + name + "\".");
				}
				catch (Exception e)
				{
					GD.PrintErr("Failed to load model \"" + name + "\"!\n" + e.Message);
				}
			}

			GD.Print($"Loaded {Models.Count} Models.");
		}

		//foreach (var model in Models.Values)
		//	foreach (var mesh in model)
		//		if (mesh is MeshInstance3D mI)
		//			for (int i = 0; i < mI.Mesh.GetSurfaceCount(); i++)
		//				mI.Mesh.SurfaceGetMaterial(i).ResourceLocalToScene = true;
	}

	public static void Clear()
	{
		Models.Clear();
	}

    private static List<Node3D> UnPackScene(PackedScene p)
	{
        return p.Instantiate().GetChildren().Select(x => (Node3D)x.Duplicate()).ToList();
	}
}
