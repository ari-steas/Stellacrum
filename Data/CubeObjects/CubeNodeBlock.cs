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
            foreach (Node3D child in FindChildren("CNode_*", owned:false))
            {
                string type = child.Name.ToString()[6..];
                
                int idx = type.IndexOf('.');
                if (idx != -1)
                    type = type[..idx];
                idx = type.IndexOf('_');
                if (idx != -1)
                    type = type[..idx];
                
                if (connectorNodes.ContainsKey(type))
                    connectorNodes[type].Add(child);
                else
                    connectorNodes.Add(type, new List<Node3D> { child });
            }
        }

        public override void _Process(double delta)
        {
            foreach (var nodeL in connectorNodes.Values)
                foreach (var node in nodeL)
                    DebugDraw.Point(node.GlobalPosition, 0.5f, Colors.Turquoise);
        }

        /// <summary>
        /// Gets adajent connected blocks. Nodes must overlap. Node types are formatted as: CNode_[name]_[num]
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
                bool joinedType = false;
                foreach (var node in typePair.Value)
                {
                    node.Position = node.Position.Snapped(Vector3.One / 100);
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
                        GD.PrintErr("Position: " + node.Position + "/" + halfSize);
                    }

                    // Rotate to account for block rotation
                    checkPos = ToGlobal(checkPos);
                    //checkPos *= Basis;

                    //checkPos += Position;
                    CubeBlock adajent = grid.BlockAt(grid.GlobalToGridCoordinates(checkPos));
                    DebugDraw.Point(grid.GridToGlobalPosition(grid.GlobalToGridCoordinates(checkPos)), 0.5f, Colors.Red, 1f);

                    GD.Print("\n");
                    if (adajent == null || adajent == this)
                    {
                        GD.Print("Adajent is null or self");
                        continue;
                    }

                    if (adajent is CubeNodeBlock adajentNodeBlock)
                    {
                        // Check every node of same type on adajent block. Hopefully doesn't have that big of a performance impact?

                        // Make sure the connection node types match up, ofc
                        if (!adajentNodeBlock.connectorNodes.ContainsKey(typePair.Key))
                        {
                            GD.Print("Types don't match up");
                            return;
                        }

                        foreach (var aNode in adajentNodeBlock.connectorNodes[typePair.Key])
                        {
                            // Check if node positions line up
                            if (!AccurateToOne(aNode.GlobalPosition, node.GlobalPosition))
                            {
                                GD.Print("Positions don't line up");
                                continue;
                            }

                            // Check for existing structures. if one exists, join. if already in one, merge.
                            GridTreeStructure adajentStructure = adajentNodeBlock.GetMemberStructure(typePair.Key);
                            if (adajentStructure == null)
                            {
                                GD.Print("Null adajentStructure");
                                continue;
                            }

                            // TODO check if this already has structure, if true merge
                            if (MemberStructures.ContainsKey(typePair.Key))
                            {
                                // Merge structures
                                joinedType = MemberStructures[typePair.Key].Merge(adajentStructure);
                                GD.Print("Merge attempt");
                            }
                            else
                            {
                                // Join existing structure
                                adajentStructure.AddStructureBlock(this);
                                joinedType = true;
                                GD.Print("Joined structure!");
                            }
                        }
                    }
                    else { GD.Print("Not a CubeNodeBlock");  }
                }

                // Create new structure if none could be found
                if (!joinedType)
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
            CallDeferred("CheckAllConnectedBlocks");
        }

        private bool AccurateToOne(Vector3 a, Vector3 b)
        {
            Vector3 c = Vector3.One * 0.1f;
            return a.Snapped(c).IsEqualApprox(b.Snapped(c));
        }
    }
}
