using System.Text.RegularExpressions;
using Godot;

public class JsonHelper
{
	public static float[] StoreVec(Vector3 vec)
	{
		return new float[] {vec.X, vec.Y, vec.Z};
	}
	public static Vector3 VariantToVec3(Variant v)
	{
		try
		{
			float[] f = v.AsFloat32Array();
			return new(f[0], f[1], f[2]);
		}
		catch
		{
			return Vector3.Zero;
		}
	}
}
