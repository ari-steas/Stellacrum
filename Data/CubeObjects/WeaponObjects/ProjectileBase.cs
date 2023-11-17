using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stellacrum.Data.CubeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeObjects.WeaponObjects
{
    /// <summary>
    /// Base class for hitscan projectiles. Initialize with WeaponBase.New();
    /// </summary>
    public partial class ProjectileBase : Node3D
    {
        static int ParticleIntensity = 8;

        private static PackedScene baseScene = GD.Load<PackedScene>("res://Data/CubeObjects/WeaponObjects/ProjectileBase.tscn");

        /// <summary>
        /// Instantiate new projectile of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T New<T>(Node3D firer) where T : ProjectileBase
        {
            // GROSS
            T projectile = Activator.CreateInstance<T>();
            foreach (var child in baseScene.Instantiate().GetChildren())
            {
                child.GetParent().RemoveChild(child);
                projectile.AddChild(child);
                //child.Reparent(projectile, false);
            }
            projectile.Rotation = firer.Rotation;
            projectile.Position = firer.GlobalPosition;
            return projectile;
        }

        [Obsolete("Not for manual use", true)]
        public ProjectileBase() { }

        public int Damage { get; internal set; } = 100;
        /// <summary>
        /// Total possible block hits. Set to a negative number to ignore.
        /// </summary>
        public int MaxBlockHits = 4;
        public float Size { get; internal set; } = 1;
        /// <summary>
        /// Total active time. Set to zero for a one-tick pulse, set to (MaxDistance/Speed) for max distance.
        /// </summary>
        public float ActiveTime = 4;
        /// <summary>
        /// If true, Damage is subtracted by block HP on hit.
        /// </summary>
        public bool DamageSum = false;

        internal RayCast3D rayCast;
        internal GpuParticles3D particles;

        public override void _Ready()
        {
            base._Ready();

            rayCast = GetChild<RayCast3D>(0);
            rayCast.TargetPosition = Vector3.Forward * Size;
            rayCast.Enabled = true;

            particles = GetChild<GpuParticles3D>(1);
            ParticleProcessMaterial pm = (ParticleProcessMaterial)particles.ProcessMaterial;
            pm.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
            pm.EmissionBoxExtents = new Vector3(0, 0, Size/2);
            
            particles.Amount = (int)(Size * ParticleIntensity);
            particles.Position = rayCast.Position + rayCast.TargetPosition / 2;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            // Hit all valid blocks in a single tick
            while (rayCast.IsColliding())
            {
                CubeBlock block = GameScene.BlockAt(rayCast);
                // Support for DamageSum setting
                if (Damage > 0)
                {
                    int blockHealthBuffer = block.Health;
                    block.Health -= Damage;
                    if (DamageSum)
                        Damage -= blockHealthBuffer;
                }
                MaxBlockHits--;
                if (MaxBlockHits == 0)
                {
                    QueueFree();
                    break;
                }
                rayCast.AddException(block);
            }
            rayCast.ClearExceptions();

            ActiveTime -= (float)delta;
            if (ActiveTime < 0)
                QueueFree();
        }
    }
}
