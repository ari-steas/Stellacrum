using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellacrum.Data.CubeObjects;

namespace Stellacrum.Data.CubeGridHelpers.MultiBlockStructures
{
    /// <summary>
    /// Power structure type.
    /// </summary>
    public partial class GridPowerStructure : GridTreeStructure
    {
        public new const string StructureName = "Power";
        public override string GetStructureName() => StructureName;

        public float PowerCapacity { get; private set; } = 0;
        public float PowerUsage { get; private set; } = 0;

        public GridPowerStructure(List<CubeBlock> StructureBlocks) : base(StructureBlocks)
        {
        }

        public override bool AddStructureBlock(CubeBlock block)
        {
            if (!base.AddStructureBlock(block))
                return false;

            if (block is GeneratorBlock generator)
                PowerCapacity += generator.MaxOutput;

            return true;
        }

        public override bool RemoveStructureBlock(CubeBlock block)
        {
            if (!base.RemoveStructureBlock(block))
                return false;

            if (block is GeneratorBlock generator)
                PowerCapacity -= generator.MaxOutput;
            return true;
        }

        public override void Update()
        {
            base.Update();
            foreach (var block in StructureBlocks)
                if (block.IsInsideTree())
                    DebugDraw.Text3D(PowerCapacity, block.GlobalPosition, 0, Colors.Magenta);
        }

        public override void Init()
        {
            base.Init();
            //GD.PrintErr("Power structure inited!");
        }
    }
}
