using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeObjects
{
    public partial class CubeNodeBlock : CubeBlock
    {
        private readonly Dictionary<string, List<Node3D>> connectorNodes = new();

        public CubeNodeBlock() { }
        public CubeNodeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            foreach (Node3D child in GetChildren())
            {
                if (child.Name.ToString().StartsWith("CNode"))
                {
                    string type = child.Name.ToString().Substring(5);
                    if (connectorNodes.ContainsKey(type))
                        connectorNodes[type].Add(child);
                    else
                        connectorNodes.Add(type, new List<Node3D> { child });

                    GD.PrintErr(child.Name);
                }
            }
        }

        /// <summary>
        /// Gets adajent connected blocks. Nodes must overlap. Node types are formatted as: CNode_[name]
        /// </summary>
        /// <param name="nodeType"></param>
        public void GetConnectedBlocks(string nodeType)
        {

        }
    }
}
