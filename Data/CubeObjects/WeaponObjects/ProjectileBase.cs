using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stellacrum.Data.CubeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.GameSceneObjects
{
    /// <summary>
    /// Base class for weapons. Initialize with WeaponBase.New();
    /// </summary>
    public partial class ProjectileBase : Node3D
    {
        static int ParticleIntensity = 8;

        private static PackedScene baseScene = GD.Load<PackedScene>("res://Data/CubeObjects/WeaponObjects/WeaponBase.tscn");

        public static ProjectileBase New()
        {
            return baseScene.Instantiate<ProjectileBase>();
        }

        [Obsolete("Not for manual use", true)]
        public ProjectileBase()
        {
        }

        public int Damage { get; private set; } = 100;
        public float Range { get; private set; } = 20;
        public bool Firing = false;
        GameScene scene;

        private RayCast3D rayCast;
        private GpuParticles3D particles;

        public override void _Ready()
        {
            base._Ready();
            scene = GameScene.GetGameScene(this);

            rayCast = GetChild<RayCast3D>(0);
            rayCast.TargetPosition = new(0, 0, -Range);
            rayCast.Enabled = true;

            particles = GetChild<GpuParticles3D>(1);
            ParticleProcessMaterial pm = (ParticleProcessMaterial)particles.ProcessMaterial;
            pm.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
            pm.EmissionBoxExtents = new Vector3(0, 0, Range/2);
            
            particles.Amount = (int)(Range * ParticleIntensity);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (Firing)
            {
                CubeBlock block = GameScene.BlockAt(rayCast);
                if (block == null)
                    return;
                block.Health -= Damage;

                particles.Position = rayCast.TargetPosition / 2;
            }
        }
    }
}
