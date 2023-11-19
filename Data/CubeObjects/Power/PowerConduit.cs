using Godot;
using Stellacrum.Data.CubeGridHelpers;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeObjects
{
    public partial class PowerConduit : CubeNodeBlock
    {
        internal GridPowerStructure powerStructure;

        public PowerConduit(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
        }

        public override GridTreeStructure CheckConnectedBlocksOfType(string type)
        {
            GridTreeStructure s = base.CheckConnectedBlocksOfType(type);
            if (type == "Power")
                powerStructure = (GridPowerStructure) s;
            return s;
        }

        public override void AddStructureRef(string type, GridMultiBlockStructure structure)
        {
            base.AddStructureRef(type, structure);
            if (type == "Power")
                powerStructure = (GridPowerStructure) structure;
        }
    }
}
