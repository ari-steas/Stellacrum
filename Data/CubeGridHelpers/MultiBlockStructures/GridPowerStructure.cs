using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeGridHelpers.MultiBlockStructures
{
    /// <summary>
    /// Power structure type.
    /// </summary>
    public partial class GridPowerStructure : GridTreeStructure
    {
        public new const string StructureName = "Power";
        public override string GetStructureName() => StructureName;

        public GridPowerStructure(List<CubeBlock> StructureBlocks) : base(StructureBlocks)
        {
        }

        public override void Update()
        {
            base.Update();
            foreach (var block in StructureBlocks)
                DebugDraw.Text3D("Power", block.GlobalPosition);
        }

        public override void Init()
        {
            base.Init();
            //GD.PrintErr("Power structure inited!");
        }
    }
}
