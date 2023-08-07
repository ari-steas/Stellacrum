using Godot;
using System;
using System.Collections.Generic;

public class FileHelper
{
    public static List<string> FindFilesWithExtension(string path, string extension)
	{
		DirAccess cdir = DirAccess.Open(path);

		List<string> files = new();

		if (cdir == null)
			return files;

		foreach (var file in cdir.GetFiles())
		{
			try
			{
				string fR = file.Replace(".import", "");
				if (fR.EndsWith(extension))
					files.Add(fR);
			}
			catch {}
		}

		foreach (var dir in cdir.GetDirectories())
			files.AddRange(FindFilesWithExtension(dir, extension));

		return files;
	}
}
