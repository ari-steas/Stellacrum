using Godot;
using Godot.Collections;
using System;

namespace Stellacrum.Data.CubeObjects.WeaponObjects
{
    public partial class ProjectilePhysical : ProjectileBase
    {
        internal float Speed = 40;
        internal Vector3 Velocity = Vector3.Zero;

        public ProjectilePhysical(string subTypeId, Dictionary<string, Variant> projectileData, bool verbose = false) : base(subTypeId, projectileData, verbose)
        {
            ReadFromData(projectileData, "Speed", ref Speed, verbose);
        }


        public void SetFirer(Vector3 position, Vector3 direction, Vector3 velocity)
        {
            SetFirer(position, direction);
            Velocity += velocity;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // Dynamically update size to prevent projectile phasing.
            rayCast.TargetPosition = Vector3.Forward * (float) (Size + (Speed + Velocity.Length())*delta);
            Position += (Basis * Vector3.Forward * Speed + Velocity) * (float) delta;
        }
    }
}
