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
        private Dictionary<string, List<CubeNodeBlock>> connectedBlocks = new();

        public CubeNodeBlock() { }
        public CubeNodeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            foreach (Node node in FindChildren("CNode_*", owned: false))
                if (node is Node3D child)
                    CheckNode(child);

            foreach (Node node in FindChildren("CNode_*", owned: true))
                if (node is Node3D child)
                    CheckNode(child);
        }

        private void CheckNode(Node3D child)
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

        public override void _Process(double delta)
        {
            //foreach (var nodeL in connectorNodes.Values)
            //    foreach (var node in nodeL)
            //        DebugDraw.Point(node.GlobalPosition, 0.5f, Colors.Turquoise);
        }

        /// <summary>
        /// Gets adajent connected blocks. Nodes must overlap. Node types are formatted as: CNode_[name]_[num]
        /// </summary>
        /// <param name="nodeType"></param>
        public List<CubeNodeBlock> GetConnectedBlocks(string nodeType)
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
        /// Checks for connected blocks of a specific type.
        /// </summary>
        /// <param name="type"></param>
        public virtual GridTreeStructure CheckConnectedBlocksOfType(string type)
        {
            CubeGrid grid = GetParent() as CubeGrid;
            Vector3 halfSize = size / 2;
            connectedBlocks.Remove(type);
            connectedBlocks.Add(type, new());

            if (!connectorNodes.ContainsKey(type))
                return null;

            bool joinedType = false;
            foreach (var node in connectorNodes[type])
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
                    //GD.PrintErr($"Node {node.Name} not aligned with block!");
                    //GD.PrintErr("Position: " + node.Position + "/" + halfSize);
                }

                // Rotate to account for block rotation
                checkPos = ToGlobal(checkPos);
                //checkPos *= Basis;

                //checkPos += Position;
                CubeBlock adajent = grid.BlockAt(grid.GlobalToGridCoordinates(checkPos));
                DebugDraw.Point(grid.GridToGlobalPosition(grid.GlobalToGridCoordinates(checkPos)), 0.5f, Colors.Red, 1f);

                //GD.Print("\n");
                if (adajent == null || adajent == this)
                {
                    //GD.Print("Adajent is null or self");
                    continue;
                }

                if (adajent is CubeNodeBlock adajentNodeBlock)
                {
                    // Check every node of same type on adajent block. Hopefully doesn't have that big of a performance impact?
                    if (adajent.IsQueuedForDeletion() || !adajent.IsInsideTree())
                        continue;

                    // Make sure the connection node types match up, ofc
                    if (!adajentNodeBlock.connectorNodes.ContainsKey(type))
                    {
                        //GD.Print("Types don't match up");
                        continue;
                    }

                    foreach (var aNode in adajentNodeBlock.connectorNodes[type])
                    {
                        // Check if node positions line up
                        if (!AccurateToOne(aNode.GlobalPosition, node.GlobalPosition))
                        {
                            //GD.Print("Positions don't line up");
                            continue;
                        }

                        // Check for existing structures. if one exists, join. if already in one, merge.
                        GridTreeStructure adajentStructure = adajentNodeBlock.GetMemberStructure(type);
                        if (adajentStructure == null)
                        {
                            //GD.Print("Null adajentStructure");
                            continue;
                        }

                        connectedBlocks[type].Add(adajentNodeBlock);

                        if (adajentNodeBlock.connectedBlocks.ContainsKey(type))
                            adajentNodeBlock.connectedBlocks[type].Add(this);

                        // Check if this already has structure, if true merge
                        if (MemberStructures.ContainsKey(type))
                        {
                            // Merge structures
                            joinedType |= MemberStructures[type].Merge(adajentStructure);
                            //GD.Print("Merge attempt");
                        }
                        else
                        {
                            // Join existing structure
                            adajentStructure.AddStructureBlock(this);
                            joinedType = true;
                            //GD.Print("Joined structure!");
                        }
                    }
                }
                //else { GD.Print("Not a CubeNodeBlock"); }
            }

            // Create new structure if none could be found
            if (!joinedType)
            {
                //GD.Print("Searching for structure of type " + type);
                GridTreeStructure structure = (GridTreeStructure)GridMultiBlockStructure.New(type, new List<CubeBlock> { this });
                structure?.Init();
            }

            return (GridTreeStructure) MemberStructures[type];
        }

        public override void Close()
        {
            base.Close();
            foreach (var bListP in connectedBlocks)
                foreach (CubeNodeBlock block in bListP.Value)
                    if (block.connectedBlocks.ContainsKey(bListP.Key))
                        block.connectedBlocks[bListP.Key].Remove(this);
        }

        /// <summary>
        /// Scans for connected blocks.
        /// </summary>
        public void CheckAllConnectedBlocks()
        {
            foreach (var type in connectorNodes.Keys)
                CheckConnectedBlocksOfType(type);
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
