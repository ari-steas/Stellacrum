using Godot;
using System;
using System.Collections.Generic;

public class TextureLoader
{
    private readonly static Dictionary<string, Texture2D> Textures = new();

    public static void StartLoad(string path)
    {
        GD.Print("\n\nStarting texture load from " + path);

        if (path[^1] != '/')
			path += '/';

        List<string> allTextures = FileHelper.FindFilesWithExtension(path, ".png");

        foreach (var texture in allTextures)
        {
            if (Textures.ContainsKey(texture))
                continue;
            try
            {
                Textures.Add(texture, GD.Load<Texture2D>(path + texture));
                GD.Print("Loaded texture \"" + texture + "\".");
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        GD.Print($"Loaded {Textures.Count} Textures.\n\n");
    }

    public static Texture2D Get(string name)
    {
        // Fail-over in case all textures are missing. missing.png should always be in this folder.
        if (Textures.Count == 0)
        {
            StartLoad("res://Assets/Images/GUI");
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
