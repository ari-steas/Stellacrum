using Godot;
using System;

namespace Stellacrum.Data.CubeObjects.WeaponObjects
{
    public partial class ProjectilePhysical : ProjectileBase
    {
        public float Speed { get; private set; } = 40;
        public float Range { get; private set; } = 100;

        

        [Obsolete("Not for manual use", true)]
        public ProjectilePhysical() : base() { }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            Position += Basis * Vector3.Forward * Speed * (float) delta;
        }
    }
}
