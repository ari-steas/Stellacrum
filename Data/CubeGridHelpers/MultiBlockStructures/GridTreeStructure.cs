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
            //Random r = new();
            //color = new Color(r.NextSingle(), r.NextSingle(), r.NextSingle());
        }

        public override bool AddStructureBlock(CubeBlock block)
        {
            if (!base.AddStructureBlock(block))
                return false;

            //GD.Print("Structure added block " + block.Name);
            return true;
        }

        public override bool RemoveStructureBlock(CubeBlock block)
        {
            if (!base.RemoveStructureBlock(block))
                return false;

            if (block is CubeNodeBlock nodeBlock && nodeBlock.GetConnectedBlocks(GetStructureName()).Count > 1)
            {
                //GD.PrintErr("Exploding myself!");
                foreach (CubeNodeBlock structureBlock in StructureBlocks)
                    structureBlock.RemoveStructureRef(GetStructureName());
                foreach (CubeNodeBlock structureBlock in StructureBlocks)
                    structureBlock.CheckConnectedBlocksOfType(GetStructureName());
                
                Destroy();
            }

            return true;
        }

        //Color color;
        public override void Update()
        {
            //foreach (var block in StructureBlocks)
            //    if (block.IsInsideTree())
            //        DebugDraw.Point(block.GlobalPosition, 1, color);
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
