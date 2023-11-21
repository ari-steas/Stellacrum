using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Stellacrum.Data.CubeGridHelpers;

namespace Stellacrum.Data.CubeObjects.Mechanical
{
    public partial class MechanicalBlock : CubeBlock
    {
        public CubeBlock Subpart { get; internal set; }

        public MechanicalBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
            string subPartSubType = "";
            ReadFromData(blockData, "SubPartId", ref subPartSubType, verbose);

            Vector3I Offset = Vector3I.Up;
            ReadFromData(blockData, "SubPartOffset", ref Offset, verbose);

            GameScene.GetGameScene(this).SpawnGridWithBlock<SubGrid>(subPartSubType, GlobalPosition + Grid().GridToGlobalPosition(Offset), GlobalRotation, true).Reparent(Grid());
        }

        public override void _Ready()
        {
            base._Ready();

        }
    }
}
