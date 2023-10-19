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
    public partial class CubeNodeBlock : CubeBlock
    {
        private readonly Dictionary<string, List<Node3D>> connectorNodes = new();
        private Dictionary<string, List<CubeBlock>> connectedBlocks = new();
        private Dictionary<string, GridTreeStructure> memberStructures = new();

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
                }
            }
        }

        /// <summary>
        /// Gets adajent connected blocks. Nodes must overlap. Node types are formatted as: CNode_[name]
        /// </summary>
        /// <param name="nodeType"></param>
        public List<CubeBlock> GetConnectedBlocks(string nodeType)
        {
            if (connectedBlocks.ContainsKey(nodeType))
                return connectedBlocks[nodeType];

            return new();
        }

        public Dictionary<string, List<Node3D>> GetNodes()
        {
            return connectorNodes;
        }

        private void GetAllConnectedBlocks()
        {
            CubeGrid grid = GetParent() as CubeGrid;
            Vector3 halfSize = size / 2;
            foreach (var typePair in connectorNodes)
            {
                foreach (var node in typePair.Value)
                {
                    Vector3 checkPos = Vector3.Zero;
                    
                    // Offset the check position towards the block in front of the node
                    if (node.Position.X == halfSize.X)
                        checkPos.X += 1.25f;
                    else if (node.Position.X == -halfSize.X)
                        checkPos.X -= 1.25f;

                    else if (node.Position.Y == halfSize.Y)
                        checkPos.Y += 1.25f;
                    else if (node.Position.Y == -halfSize.Y)
                        checkPos.Y -= 1.25f;

                    else if (node.Position.Z == halfSize.Z)
                        checkPos.Z += 1.25f;
                    else if (node.Position.Z == -halfSize.Z)
                        checkPos.Z -= 1.25f;

                    // Rotate to account for block rotation
                    checkPos *= Basis;

                    checkPos += Position;
                    CubeBlock adajent = grid.BlockAt(grid.LocalToGridCoordinates(checkPos));
                    if (adajent == null) continue;

                    if (adajent is CubeNodeBlock adajentNode)
                    {
                        // Check every node of same type on adajent block. Hopefully doesn't have that big of a performance impact?

                        // Make sure the connection node types match up, ofc
                        if (!adajentNode.connectorNodes.ContainsKey(typePair.Key))
                            return;

                        foreach (var aNode in adajentNode.connectorNodes[typePair.Key])
                        {
                            // Check if node positions line up
                            if (!aNode.GlobalPosition.Equals(node.Position))
                                continue;

                            // TODO check for existing structures. if one exists, join. if already in one, merge.

                        }
                    }
                }

                // TODO correct structure types... register with static gridnodestructure?
                if (!memberStructures.ContainsKey(typePair.Key))
                {
                    memberStructures.Add(typePair.Key, (GridTreeStructure) GridMultiBlockStructure.New(typePair.Key, new List<CubeBlock> { this }));
                    memberStructures[typePair.Key]?.Init();
                }
            }
        }

        public override void OnPlace()
        {
            base.OnPlace();
            GetAllConnectedBlocks();
        }
    }
}
