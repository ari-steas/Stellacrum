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
        private readonly Dictionary<string, List<CubeNodeBlock>> connectedBlocks = new();
        private readonly Dictionary<string, string[]> connectionWhitelist;

        public CubeNodeBlock() { }
        public CubeNodeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
            foreach (Node node in FindChildren("CNode_*", owned: false))
                if (node is Node3D child)
                    CheckNode(child);

            foreach (Node node in FindChildren("CNode_*", owned: true))
                if (node is Node3D child)
                    CheckNode(child);

            Godot.Collections.Dictionary<string, Variant> allowedConnectionsGD = new();
            ReadFromData(blockData, "ConnectionWhitelist", ref allowedConnectionsGD, verbose);
            connectionWhitelist = JsonHelper.GDToDictionary<string, string[]>(allowedConnectionsGD);
        }

        /// <summary>
        /// Checks single node for type, and adds to list.
        /// </summary>
        /// <param name="child"></param>
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
        /// <param name="connectionType"></param>
        public virtual GridTreeStructure CheckConnectedBlocksOfType(string connectionType)
        {
            CubeGrid grid = GetParent() as CubeGrid;
            Vector3 halfSize = size / 2;
            connectedBlocks.Remove(connectionType);
            connectedBlocks.Add(connectionType, new());

            if (!connectorNodes.ContainsKey(connectionType))
                return null;

            bool joinedType = false;
            foreach (var node in connectorNodes[connectionType])
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

                grid.TryGetBlockAt(grid.ToLocal(checkPos), out CubeBlock adajentCubeBlock); // TODO

                if (adajentCubeBlock == null || adajentCubeBlock == this)
                    continue;

                if (adajentCubeBlock is CubeNodeBlock adajentNodeBlock)
                {
                    // Check every node of same type on adajent block. Hopefully doesn't have that big of a performance impact?
                    if (adajentCubeBlock.IsQueuedForDeletion() || !adajentCubeBlock.IsInsideTree())
                        continue;

                    // Make sure the connection node types match up, ofc
                    if (!adajentNodeBlock.connectorNodes.ContainsKey(connectionType))
                        continue;

                    // Skips if the block's subtypeid is not in the connectiontype whitelist.
                    // Does not check if the whitelist is missing.
                    if (connectionWhitelist.ContainsKey(connectionType) && !connectionWhitelist[connectionType].Contains(adajentNodeBlock.subTypeId))
                        continue;

                    // Skips if the adajent block's connectiontype whitelist contains own subTypeId
                    // Does not check if the whitelist is missing.
                    if (adajentNodeBlock.connectionWhitelist.ContainsKey(connectionType) && !adajentNodeBlock.connectionWhitelist[connectionType].Contains(subTypeId))
                        continue;

                    foreach (var aNode in adajentNodeBlock.connectorNodes[connectionType])
                    {
                        // Check if node positions line up
                        if (!AccurateToOne(aNode.GlobalPosition, node.GlobalPosition))
                            continue;

                        // Check for existing structures. if one exists, join. if already in one, merge.
                        GridTreeStructure adajentStructure = adajentNodeBlock.GetMemberStructure(connectionType);
                        if (adajentStructure == null)
                            continue;

                        connectedBlocks[connectionType].Add(adajentNodeBlock);

                        if (adajentNodeBlock.connectedBlocks.ContainsKey(connectionType))
                            adajentNodeBlock.connectedBlocks[connectionType].Add(this);

                        // Check if this already has structure, if true merge
                        if (MemberStructures.ContainsKey(connectionType))
                            joinedType |= MemberStructures[connectionType].Merge(adajentStructure);
                        else
                        {
                            // Join existing structure
                            adajentStructure.AddStructureBlock(this);
                            joinedType = true;
                        }
                    }
                }
            }

            // Create new structure if none could be found
            if (!joinedType)
            {
                GridTreeStructure structure = (GridTreeStructure)GridMultiBlockStructure.New(connectionType, new List<CubeBlock> { this });
                structure?.Init();
            }

            return (GridTreeStructure) MemberStructures[connectionType];
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

        /// <summary>
        /// Checks if two vectors are within 0.1m of eachother.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool AccurateToOne(Vector3 a, Vector3 b)
        {
            Vector3 c = Vector3.One * 0.1f;
            return a.Snapped(c).IsEqualApprox(b.Snapped(c));
        }
    }
}
