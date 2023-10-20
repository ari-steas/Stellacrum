using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeObjects
{
    public partial class PowerConduit : CubeNodeBlock
    {
        public PowerConduit(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
        }
    }
}
