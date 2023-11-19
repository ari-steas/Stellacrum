using Godot;
using Stellacrum.Data.CubeObjects;
using System;

namespace Stellacrum.Data.CubeObjects
{
    public partial class CockpitBlock : CubeBlock
    {
        public CockpitBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
        }

        public CockpitBlock() { }

        public override void _Process(double delta)
        {

        }
    }
}