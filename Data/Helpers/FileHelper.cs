using Godot;
using System;
using System.Collections.Generic;

public class FileHelper
{
    public static List<string> FindFilesWithExtension(string path, string extension, bool recursive = false)
	{
		DirAccess cdir = DirAccess.Open(path);

		List<string> files = new();
		
		if (path[^1] != '/')
            path += '/';

		if (cdir == null)
			return files;

		foreach (var file in cdir.GetFiles())
		{
			try
			{
				// PCK files add ".import" to end of file name, this fixes.
				string fR = file.Replace(".import", "");
				if (fR.EndsWith(extension) && !files.Contains(path + fR))
					files.Add(path + fR);
			}
			catch {}
		}

		if (recursive)
			foreach (var dir in cdir.GetDirectories())
				files.AddRange(FindFilesWithExtension(path + dir, extension, true));

		return files;
	}

	public static void RecursiveDelete(string path)
	{
		if (!path.EndsWith('/'))
			path += "/";
		
		DirAccess toDelete = DirAccess.Open(path);

		foreach (var dir in toDelete.GetDirectories())
			RecursiveDelete(path + dir);

	    foreach (var file in toDelete.GetFiles())
			DirAccess.RemoveAbsolute(path + file);

		GD.PrintErr(DirAccess.RemoveAbsolute(path));
	}
}
