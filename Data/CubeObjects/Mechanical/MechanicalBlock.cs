using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Stellacrum.Data.CubeGridHelpers;

namespace Stellacrum.Data.CubeObjects.Mechanical
{
    // TODO: Make abstract
    public partial class MechanicalBlock : CubeBlock
    {
        public Generic6DofJoint3D SubpartJoint { get; internal set; }
        public CubeGrid SubpartGrid { get; internal set; }
        internal string subPartSubType = "";
        internal Vector3I Offset = Vector3I.Up;

        internal bool JointAngularX_Limited = false, JointAngularY_Limited = false, JointAngularZ_Limited = false;

        public MechanicalBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
            ReadFromData(blockData, "SubPartId", ref subPartSubType, verbose);
            ReadFromData(blockData, "SubPartOffset", ref Offset, verbose);

            ReadFromData(blockData, "JointAngularX_Limited", ref JointAngularX_Limited, verbose);
            ReadFromData(blockData, "JointAngularY_Limited", ref JointAngularY_Limited, verbose);
            ReadFromData(blockData, "JointAngularZ_Limited", ref JointAngularZ_Limited, verbose);
        }
        public MechanicalBlock() { }

        /// <summary>
        /// Creates the linking joint between main grid and subgrid.
        /// </summary>
        //public abstract void CreateJoint();
        public void CreateJoint()
        {
            SubpartJoint = new Generic6DofJoint3D()
            {
                NodeA = Grid().GetPath(),
                NodeB = SubpartGrid.GetPath(),
                Position = Position + Grid().GridToLocalPosition(Offset),
            };

            SubpartJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, JointAngularX_Limited);
            SubpartJoint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, JointAngularY_Limited);
            SubpartJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, JointAngularZ_Limited);

            Grid().AddChild(SubpartJoint);
        }

        public override void _Ready()
        {
            base._Ready();

            SubpartGrid = GameScene.GetGameScene(this)
                .SpawnGridWithBlock(
                    subPartSubType,
                    Position + Grid().GridToLocalPosition(Offset),
                    Rotation,
                    true,
                    Grid()
                );

            CallDeferred(MethodName.CreateJoint);
        }

        public override void Close()
        {
            SubpartJoint.QueueFree();

            // Seperate subpart grid from parent
            SubpartGrid.Reparent(GameScene.GetGameScene(this));
            SubpartGrid.ParentGrid.subGrids.Remove(SubpartGrid);
            SubpartGrid.ParentGrid = null;

            base.Close();
        }
    }
}
