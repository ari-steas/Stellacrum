using Godot;
using Stellacrum.Data.CubeGridHelpers;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.XmlParser;

namespace Stellacrum.Data.CubeObjects
{
    public partial class CubeNodeBlock : CubeBlock
    {
        private readonly Dictionary<string, List<Node3D>> connectorNodes = new();
        private Dictionary<string, List<CubeBlock>> connectedBlocks = new();

        public CubeNodeBlock() { }
        public CubeNodeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            foreach (Node3D child in GetChildren())
            {
                if (child.Name.ToString().StartsWith("CNode_"))
                {
                    string type = child.Name.ToString().Substring(6);
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

        public GridTreeStructure GetMemberStructure(string type)
        {
            if (MemberStructures.ContainsKey(type) && MemberStructures[type] is GridTreeStructure treeStructure)
                return treeStructure;

            return null;
        }

        public Dictionary<string, List<Node3D>> GetNodes()
        {
            return connectorNodes;
        }

        /// <summary>
        /// Scans for connected blocks.
        /// </summary>
        private void CheckAllConnectedBlocks()
        {
            CubeGrid grid = GetParent() as CubeGrid;
            Vector3 halfSize = size / 2;
            foreach (var typePair in connectorNodes)
            {
                foreach (var node in typePair.Value)
                {
                    Vector3 checkPos = node.Position;
                    
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
                    else
                    {
                        GD.PrintErr($"Node {node.Name} not aligned with block!");
                    }

                    // Rotate to account for block rotation
                    checkPos *= Basis;

                    checkPos += Position;
                    CubeBlock adajent = grid.BlockAt(grid.LocalToGridCoordinates(checkPos));
                    GD.Print("\n\n");
                    if (adajent == null || adajent == this) continue;

                    if (adajent is CubeNodeBlock adajentNode)
                    {
                        // Check every node of same type on adajent block. Hopefully doesn't have that big of a performance impact?

                        // Make sure the connection node types match up, ofc
                        if (!adajentNode.connectorNodes.ContainsKey(typePair.Key))
                            return;

                        foreach (var aNode in adajentNode.connectorNodes[typePair.Key])
                        {
                            // Check if node positions line up
                            if (!AccurateToOne(aNode.GlobalPosition, node.GlobalPosition))
                                continue;

                            // Check for existing structures. if one exists, join. if already in one, merge.

                            // TODO check if this already has structure, if true merge

                            GridTreeStructure adajentStructure = aNode.GetParent<CubeNodeBlock>().GetMemberStructure(typePair.Key);
                            if (adajentStructure == null)
                                continue;
                            adajentStructure.AddStructureBlock(this);
                            GD.Print("Joined structure!");
                        }
                    }
                }

                // Create new structure if none could be found
                if (!MemberStructures.ContainsKey(typePair.Key))
                {
                    GD.Print("Searching for structure of type " + typePair.Key);
                    GridTreeStructure structure = (GridTreeStructure) GridMultiBlockStructure.New(typePair.Key, new List<CubeBlock> { this });
                    structure?.Init();
                }
            }
        }

        public override void OnPlace()
        {
            base.OnPlace();
            CheckAllConnectedBlocks();
        }

        private bool AccurateToOne(Vector3 a, Vector3 b)
        {
            Vector3 c = Vector3.One * 0.1f;
            return a.Snapped(c).IsEqualApprox(b.Snapped(c));
        }
    }
}
