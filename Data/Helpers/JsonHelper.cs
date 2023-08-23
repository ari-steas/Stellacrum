using System.Text.RegularExpressions;
using Godot;

public class JsonHelper
{
	/// <summary>
	/// Converts Vector3 to float[] Variant.
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	public static float[] StoreVec(Vector3 vec)
	{
		return new float[] {vec.X, vec.Y, vec.Z};
	}

	/// <summary>
	/// Converts float[] Variant to Vector3.
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static Vector3 LoadVec(Variant v)
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
