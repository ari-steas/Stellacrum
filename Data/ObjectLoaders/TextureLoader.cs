using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


public class TextureLoader
{
    private readonly static Dictionary<string, Texture2D> Textures = new();

    public static Thread StartLoad(string path)
    {
        Thread t = new(Load);
		t.Start(path);
		return t;
    }

    static void Load(object data)
    {
        if (data is string path)
        {
            GD.Print("\n\nStarting texture load from " + path);

            List<string> allTextures = FileHelper.FindFilesWithExtension(path, ".png", true);

            foreach (var texture in allTextures)
            {
                string t = texture[(texture.LastIndexOf('/') + 1)..];
                if (!Textures.TryAdd(t, GD.Load<Texture2D>(texture)))
                    GD.PrintErr("Duplicate texture " + t);
                else
                    GD.Print("Loaded texture \"" + t + "\".");
            }

            GD.Print($"Loaded {Textures.Count} Textures.\n\n");

            if (!Textures.ContainsKey("missing.png"))
            {
                GD.PrintErr("Loading GUI textures [missing.png]");
                Load("res://Assets/Images/GUI");
            }
        }
    }

    public static Texture2D Get(string name)
    {
        // Fail-over in case all textures are missing. missing.png should always be in this folder.
        if (Textures.Count == 0)
        {
            GD.PrintErr("Critical missing [missing.png]!");
            Load("res://Assets/Images/GUI");
            return Get(name);
        }

        if (Textures.ContainsKey(name))
			return Textures[name];
		else
        {
            GD.PrintErr("Missing texture " + name);
			return Textures["missing.png"];
        }
    }

    public static void Clear()
    {
        Textures.Clear();
    }
}
