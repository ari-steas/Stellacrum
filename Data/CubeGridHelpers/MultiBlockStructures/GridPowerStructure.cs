using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeGridHelpers.MultiBlockStructures
{
    public partial class GridPowerStructure : GridTreeStructure
    {
        public GridPowerStructure(List<CubeBlock> StructureBlocks) : base(StructureBlocks)
        {
        }

        public override void Init()
        {
            base.Init();
            AddStructureType("power", GetType());
        }
    }
}
