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
        public HingeJoint3D SubpartJoint { get; internal set; }
        public CubeGrid SubpartGrid { get; internal set; }
        internal string subPartSubType = "";
        internal Vector3I Offset = Vector3I.Forward*2;

        internal float maxSpeed = 10;
        internal float minAngle = 0, maxAngle = 0;
        internal bool rotorLock = false;

        public MechanicalBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
            ReadFromData(blockData, "SubPartId", ref subPartSubType, verbose);
            ReadFromData(blockData, "SubPartOffset", ref Offset, verbose);

            ReadFromData(blockData, "MaxSpeed", ref maxSpeed, verbose);
            ReadFromData(blockData, "MinAngle", ref minAngle, verbose);
            ReadFromData(blockData, "MaxAngle", ref maxAngle, verbose);
            ReadFromData(blockData, "RotorLock", ref rotorLock, verbose);
        }
        public MechanicalBlock() { }

        /// <summary>
        /// Creates the linking joint between main grid and subgrid.
        /// </summary>
        //public abstract void CreateJoint();
        public void CreateJoint()
        {
            SubpartJoint = new HingeJoint3D()
            {
                NodeA = Grid().GetPath(),
                NodeB = SubpartGrid.GetPath(),
                Position = Position + Grid().GridToLocalPosition((Vector3I)(Basis * Offset)),
                Rotation = Rotation,
            };
            SubpartJoint.SetFlag(HingeJoint3D.Flag.EnableMotor, true);
            SubpartJoint.SetFlag(HingeJoint3D.Flag.UseLimit, rotorLock);

            SubpartJoint.SetParam(HingeJoint3D.Param.LimitLower, minAngle);
            SubpartJoint.SetParam(HingeJoint3D.Param.LimitUpper, maxAngle);

            Grid().AddChild(SubpartJoint);
        }

        public override void OnPlace()
        {
            base.OnPlace();

            SubpartGrid = GameScene.GetGameScene(this)
                .SpawnGridWithBlock(
                    subPartSubType,
                    Position + Grid().GridToLocalPosition((Vector3I)(Basis * Offset)),
                    Rotation,
                    true,
                    Grid()
                );

            CallDeferred(MethodName.CreateJoint);
        }

        public override void Close()
        {
            GD.Print(Position);
            SubpartJoint.QueueFree();

            // Seperate subpart grid from parent
            if (SubpartGrid != null)
            {
                SubpartGrid.Reparent(GameScene.GetGameScene(this));
                SubpartGrid.ParentGrid.subGrids.Remove(SubpartGrid);
                SubpartGrid.ParentGrid = null;
            }
            base.Close();
        }

        public void SetSpeed(float speed)
        {
            // Limit speed to max speed.
            if (speed > maxSpeed)
                speed = maxSpeed;
            else if (speed < -maxSpeed)
                speed = -maxSpeed;

            SubpartJoint.SetParam(HingeJoint3D.Param.MotorTargetVelocity, speed);
        }

        public float GetSpeed()
        {
            return SubpartJoint.GetParam(HingeJoint3D.Param.MotorTargetVelocity);
        }

        public float GetAngle()
        {
            return SubpartGrid.Rotation.Z;
        }

        public void SetMinAngle(float angle)
        {
            SubpartJoint.SetParam(HingeJoint3D.Param.LimitLower, angle);
        }

        public float GetMinAngle()
        {
            return SubpartJoint.GetParam(HingeJoint3D.Param.LimitLower);
        }

        public void SetMaxAngle(float angle)
        {
            SubpartJoint.SetParam(HingeJoint3D.Param.LimitUpper, angle);
        }

        public float GetMaxAngle()
        {
            return SubpartJoint.GetParam(HingeJoint3D.Param.LimitUpper);
        }
    }
}
