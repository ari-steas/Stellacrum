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

        public ProjectileBase(string subTypeId, Godot.Collections.Dictionary<string, Variant> projectileData, bool verbose = false)
        {
            foreach (var child in baseScene.Instantiate().GetChildren())
            {
                child.GetParent().RemoveChild(child);
                AddChild(child);
                //child.Reparent(projectile, false);
            }

            SubTypeId = subTypeId;
            ReadFromData(projectileData, "Damage", ref Damage, verbose);
            ReadFromData(projectileData, "MaxBlockHits", ref MaxBlockHits, verbose);
            ReadFromData(projectileData, "DamageSum", ref DamageSum, verbose);
            ReadFromData(projectileData, "ActiveTime", ref ActiveTime, verbose);
            ReadFromData(projectileData, "Size", ref Size, verbose);
            ReadFromData(projectileData, "AreaEffectEnabled", ref AreaEffectEnabled, verbose);
            ReadFromData(projectileData, "AreaEffectRadius", ref AreaEffectRadius, verbose);
            ReadFromData(projectileData, "AreaEffectDamage", ref AreaEffectDamage, verbose);

            Name = "Projectile." + subTypeId + "." + GetIndex();
        }

        public ProjectileBase() { }

        public void SetFirer(Node3D firer)
        {
            Rotation = firer.Rotation;
            Position = firer.GlobalPosition;
        }

        internal string SubTypeId = "";

        /// <summary>
        /// Single-hit damage. Affected by DamageSum.
        /// </summary>
        internal int Damage = 100;

        /// <summary>
        /// Total possible block hits. Set to a negative number to ignore.
        /// </summary>
        internal int MaxBlockHits = 1;

        internal float Size = 1;

        /// <summary>
        /// Total active time. Set to zero for a one-tick pulse, set to (MaxDistance/Speed) for max distance.
        /// </summary>
        internal float ActiveTime = 4;

        /// <summary>
        /// If true, Damage is subtracted by block HP on hit.
        /// </summary>
        internal bool DamageSum = false;

        /// <summary>
        /// If true, deal AoE damage.
        /// </summary>
        internal bool AreaEffectEnabled = false;

        /// <summary>
        /// Radius of AoE damage sphere.
        /// </summary>
        internal float AreaEffectRadius = 5;

        /// <summary>
        /// Explosive damage applied. Affected by DamageSum.
        /// </summary>
        internal int AreaEffectDamage = 100;


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

            HandleBlockHits();

            // Remove self if too old.
            ActiveTime -= (float)delta;
            if (ActiveTime < 0)
                QueueFree();
        }

        private void HandleBlockHits()
        {
            // Hit all valid blocks in a single tick
            while (rayCast.IsColliding())
            {
                CubeBlock block = GameScene.GetBlockAt(rayCast);
                // If hits self...
                if (block == null)
                    break;

                // Support for DamageSum setting
                if (Damage > 0)
                {
                    // Single hit damage
                    int blockHealthBuffer = block.Health;
                    block.Health -= Damage;
                    if (DamageSum)
                        Damage -= blockHealthBuffer;
                }

                if (AreaEffectEnabled && AreaEffectDamage > 0)
                    HandleExplosiveHits(GameScene.GetGridAt(rayCast), rayCast.GetCollisionPoint());

                MaxBlockHits--;
                if (MaxBlockHits == 0)
                {
                    QueueFree();
                    break;
                }

                rayCast.AddException(block);
            }
            rayCast.ClearExceptions();
        }

        private void HandleExplosiveHits(CubeGrid grid, Vector3 globalHitPosition)
        {
            foreach (var block in grid.GetCubeBlocks())
            {
                // TODO consider collision shapes
                if (block.GlobalPosition.DistanceTo(globalHitPosition) < AreaEffectRadius)
                {
                    // Support for DamageSum setting
                    if (AreaEffectDamage > 0)
                    {
                        // Single hit damage
                        int blockHealthBuffer = block.Health;
                        block.Health -= AreaEffectDamage;
                        if (DamageSum)
                            AreaEffectDamage -= blockHealthBuffer;
                    } 
                }
            }
        }

        protected void ReadFromData<[MustBeVariant] T>(Godot.Collections.Dictionary<string, Variant> blockData, string dataKey, ref T variable, bool verbose)
        {
            if (blockData.ContainsKey(dataKey))
            {
                if (typeof(T) == typeof(Vector3))
                    variable = (T)Convert.ChangeType(JsonHelper.LoadVec(blockData[dataKey]), typeof(T));
                else
                    variable = blockData[dataKey].As<T>();
            }
            else if (verbose)
                GD.PrintErr($"Missing Projectile.{SubTypeId} data [{dataKey}]! Setting to default...");
        }
    }
}
