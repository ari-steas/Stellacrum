using Godot;

namespace Stellacrum.Data.Helpers.Math
{
    internal static class MathHelper
    {
        public static float MaxComponent(this Vector3 vec)
        {
            if (vec.X >= vec.Y && vec.X >= vec.Z)
                return vec.X;
            if (vec.Y >= vec.Z)
                return vec.Y;
            return vec.Z;
        }
    }
}
