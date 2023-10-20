using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Stellacrum.Data.CubeObjects;

namespace Stellacrum.Data.CubeGridHelpers.MultiBlockStructures
{
    /// <summary>
    /// Base class for a 'branching' multiblock structure; i.e. inventory or power.
    /// </summary>
    public partial class GridTreeStructure : GridMultiBlockStructure
    {
        public new const string StructureName = "Tree";
        public override string GetStructureName() => StructureName;

        public GridTreeStructure(List<CubeBlock> StructureBlocks) : base(StructureBlocks)
        {
        }

        public override void AddStructureBlock(CubeBlock block)
        {
            base.AddStructureBlock(block);
            GD.Print("Structure added block " + block.Name);
        }

        public override void Update()
        {
            foreach (var block in StructureBlocks)
                DebugDraw.Point(block.GlobalPosition, 1, Colors.Magenta);
        }

        public override void Update10()
        {
            
        }

        public override void Update60()
        {
            
        }

        public static void CheckConnection(CubeBlock block)
        {

        }

        public override void Init()
        {
            base.Init();
        }
    }
}
