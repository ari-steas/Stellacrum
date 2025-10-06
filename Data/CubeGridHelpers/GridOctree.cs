using Godot;
using Stellacrum.Data.CubeObjects;
using System;
using System.Collections.Generic;

namespace Stellacrum.Data.CubeGridHelpers
{
    public class GridOctree
    {
        private static readonly float Slack = CubeGrid.MinGridSize/16;
        private static readonly Vector3 SlackVec = Vector3.One * Slack;

        /// <summary>
        /// Cell width in meters
        /// </summary>
        public readonly float CellWidth;
        public Vector3 RootPosition { get; protected set; }
        public GridOctree Parent;

        public readonly bool IsLeaf;
        protected readonly GridOctree[] _subtrees;
        protected readonly CubeBlock[] _blocks;

        public IEnumerable<GridOctree> Subtrees => _subtrees;

        public GridOctree(Vector3 rootPosition, float cellWidth, GridOctree parent)
        {
            RootPosition = rootPosition;
            CellWidth = cellWidth;
            Parent = parent;

            IsLeaf = Math.Abs(cellWidth - CubeGrid.MinGridSize) < Slack;
            if (IsLeaf)
                _blocks = new CubeBlock[8];
            else
                _subtrees = new GridOctree[8];
        }

        /// <summary>
        /// Adds a new subtree to this tree's hierarchy.
        /// </summary>
        /// <param name="vec"></param>
        /// <exception cref="ArgumentException">Thrown if the cell is already occupied.</exception>
        public GridOctree AddSubtree(Vector3I vec)
        {
            return AddSubtree(VecToIndex(vec));
        }

        /// <summary>
        /// Adds a new subtree to this tree's hierarchy.
        /// </summary>
        /// <param name="idx"></param>
        /// <exception cref="ArgumentException">Thrown if the cell is already occupied.</exception>
        public GridOctree AddSubtree(int idx)
        {
            if (IsLeaf)
                throw new Exception("Leaf nodes cannot have subtrees.");

            if (_subtrees[idx] != null)
                throw new ArgumentException($"Cell index {idx} is already occupied.");

            Vector3I vec = IndexToVec3I(idx);
            _subtrees[idx] = new GridOctree(RootPosition + (Vector3) vec * CellWidth, CellWidth/2, this);
            //GD.Print($"New subtree: {vec} offset @ {CellWidth}m: {RootPosition} -> {_subtrees[idx].RootPosition}");
            return _subtrees[idx];
        }

        /// <summary>
        /// Creates a new octree containing an existing tree.
        /// </summary>
        /// <param name="idx"></param>
        /// <exception cref="ArgumentException">Thrown if the cell is already occupied.</exception>
        public static GridOctree AddSupertree(Vector3I vec, GridOctree tree)
        {
            int idx = VecToIndex(vec);
            GridOctree superTree = new GridOctree(tree.RootPosition - (Vector3) vec * tree.CellWidth * 2, tree.CellWidth * 2, null);
            superTree._subtrees[idx] = tree;
            tree.Parent = superTree;
            //GD.Print($"New supertree: {vec} offset @ {tree.CellWidth}m: {tree.RootPosition} -> {superTree.RootPosition}");
            return superTree;
        }

        public static void ExpandTree(ref GridOctree tree, Vector3 position, Vector3 extents)
        {
            while (!tree.ContainsBox(position, extents))
            {
                // Offset octree generation to fit block
                Vector3 rel = position - tree.RootPosition;
                Vector3I newRoot = new Vector3I(rel.X < 0 ? 1 : 0, rel.Y < 0 ? 1 : 0, rel.Z < 0 ? 1 : 0);
                tree = AddSupertree(newRoot, tree);
                //GD.Print($"Expand tree {tree.CellWidth}m, {newRoot}");
            }
        }

        public void RemoveAt(Vector3I vec)
        {
            int idx = VecToIndex(vec);
            if (IsLeaf)
            {
                _blocks[idx] = null;
            }
            else
            {
                _subtrees[idx] = null;
            }
        }

        ///// <summary>
        ///// Item at internal position.
        ///// </summary>
        ///// <param name="vec">Scales from 0,0,0 to 1,1,1</param>
        ///// <param name="tree"></param>
        ///// <param name="block"></param>
        //public void ItemAt(Vector3I vec, out GridOctree tree, out CubeBlock block)
        //{
        //    // 0,0,0 = 0
        //    // 0,0,1 = 1
        //    // 0,1,0 = 2
        //    // 0,1,1 = 3
        //    // 1,0,0 = 4
        //    // 1,0,1 = 5
        //    // 1,1,0 = 6
        //    // 1,1,1 = 7
        //
        //    int idx = VecToIndex(vec);
        //    tree = _subtrees[idx];
        //    block = _blocks[idx];
        //}

        public bool TryGetBlockAt(Vector3 position, out CubeBlock block)
        {
            block = null;

            if (!ContainsPoint(position))
                return false;

            GridOctree toCheck = this;

            // Check trees internal to the volume
            while (!toCheck.IsLeaf)
            {
                var prevCheck = toCheck;

                foreach (var subtree in toCheck._subtrees)
                {
                    if (subtree == null || !subtree.ContainsPoint(position))
                        continue;

                    toCheck = subtree;
                    break;
                }

                if (toCheck == prevCheck)
                    return false; // failsafe
            }

            for (int i = 0; i < toCheck._blocks.Length; i++)
            {
                if (toCheck.CellContainsPoint(i, position))
                {
                    block = toCheck._blocks[i];
                    return block != null;
                }
            }

            return false;
        }

        /// <summary>
        /// ASSUMES POSITIVE EXTENTS
        /// </summary>
        /// <param name="position"></param>
        /// <param name="extents"></param>
        /// <param name="cubes"></param>
        /// <returns>True if any blocks were located</returns>
        public bool GetBlocksInVolume(Vector3 position, Vector3 extents, ref HashSet<CubeBlock> cubes)
        {
            if (!IntersectsBox(position, extents))
                return false;

            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            toCheck.Push(this);

            // Check trees internal to the volume
            bool didAdd = false;
            while (toCheck.TryPop(out GridOctree current))
            {
                if (current.IsLeaf)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (current._blocks[i] == null)
                            continue;

                        if (current.CellIntersectsBox(i, position + SlackVec, extents - SlackVec * 2))
                        {
                            cubes?.Add(current._blocks[i]);
                            didAdd = true;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (current._subtrees[i] == null)
                            continue;

                        if (current.CellIntersectsBox(i, position + SlackVec, extents - SlackVec * 2))
                        {
                            toCheck.Push(current._subtrees[i]);
                        }
                    }
                }
            }

            return didAdd;
        }

        public bool SetBlockAt(Vector3 position, Basis rotation, CubeBlock block)
        {
            //Vector3 extents = block.size * rotation; // TODO
            Vector3 extents = block.size;

            if (!IntersectsBox(position + SlackVec, extents - SlackVec * 2))
                return false;

            // Narrow down to smallest fully enveloping octree
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            toCheck.Push(SmallestTreeForVolume(position, extents));

            // Check trees internal to the volume
            bool didAdd = false;
            while (toCheck.TryPop(out GridOctree current))
            {
                //GD.Print($"[{++count}] SetBlockAt invoked.\n    Size: {current.CellWidth:F}m (matches: {current.CellWidth <= minExtents})\n    Bounds: {position} to {position + extents}");

                Vector3 cellSize = Vector3.One * current.CellWidth;

                if (current.IsLeaf)
                {
                    for (int i = 0; i < current._blocks.Length; i++)
                    {
                        Vector3 cellPos = IndexToVec3(i) * current.CellWidth + current.RootPosition;

                        if (AabbContains(position, position + extents, cellPos + SlackVec, cellPos + cellSize - SlackVec * 2))
                        {
                            //GD.Print($"    Pass BlockCell [{i}] {IndexToVec3(i)}m");

                            current._blocks[i] = block;
                            if (!block.ContainedOctrees.Contains(current))
                                block.ContainedOctrees.Add(current);
                            didAdd = true;
                        }
                    }
                }
                else
                {
                    // Otherwise generate new subtrees to occupy
                    for (var i = 0; i < current._subtrees.Length; i++)
                    {
                        var subtree = current._subtrees[i];

                        Vector3 cellPos = IndexToVec3(i) * current.CellWidth + current.RootPosition;

                        if (!AabbIntersects(position + SlackVec, position + extents - SlackVec * 2, cellPos, cellPos + cellSize))
                            continue;

                        //GD.Print($"    Pass SubtreeCell [{i}] {IndexToVec3(i)}m. Creating new: {subtree == null}");

                        if (subtree == null)
                            subtree = current.AddSubtree(i);

                        toCheck.Push(subtree);
                    }
                }
            }

            return didAdd;
        }

        public void RemoveBlock(CubeBlock block)
        {
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            foreach (var tree in block.ContainedOctrees)
            {
                // assume the block is *only* in these trees
                // remove the block first to prevent order-of-operations issues
                for (int i = 0; i < 8; i++)
                {
                    if (tree._blocks[i] == block)
                        tree._blocks[i] = null;
                }

                toCheck.Push(tree);
            }

            while (toCheck.TryPop(out GridOctree tree))
            {
                bool isTreeEmpty = true;

                if (tree.IsLeaf)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        if (tree._blocks[i] != null)
                        {
                            isTreeEmpty = false;
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < 8; i++)
                    {
                        if (tree._subtrees[i] != null)
                        {
                            isTreeEmpty = false;
                            break;
                        }
                    }
                }

                if (tree.Parent == null)
                    continue;

                if (isTreeEmpty)
                {
                    toCheck.Push(tree.Parent);
                    for (int i = 0; i < 8; i++)
                    {
                        if (tree.Parent._subtrees[i] == tree)
                        {
                            tree.Parent._subtrees[i] = null;
                            break;
                        }
                    }

                    tree.Parent = null;
                }
            }
        }

        private GridOctree SmallestTreeForVolume(Vector3 position, Vector3 extents)
        {
            position -= RootPosition;

            // Narrow down to smallest fully enveloping octree
            GridOctree rootTree = this;
            Vector3 vec = position;
            float maxExtent = Math.Max(Math.Max(Math.Abs(extents.X), Math.Abs(extents.Y)), Math.Abs(extents.Z));

            //int depth = 0;
            while (rootTree.CellWidth > maxExtent)
            {
                if (rootTree.IsLeaf)
                    return rootTree;

                //GD.Print($"[{++depth}] STFVSize: {rootTree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / rootTree.CellWidth);

                GridOctree subTree = rootTree._subtrees[VecToIndex(searchPosition)];
                vec -= (Vector3) searchPosition * rootTree.CellWidth;
                if (subTree == null)
                    return rootTree;

                rootTree = subTree;
            }

            return this;
        }

        public bool PointInVolume(Vector3 point)
        {
            Vector3 relPos = point - RootPosition;
            float extents = CellWidth * 2;

            bool passes = (0 <= relPos.X && extents >= relPos.X) &&
                          (0 <= relPos.Y && extents >= relPos.Y) &&
                          (0 <= relPos.Z && extents >= relPos.Z);
            
            return passes;
        }

        public bool IsEmpty()
        {
            if (IsLeaf)
            {
                foreach (var block in _blocks)
                    if (block != null)
                        return false;
            }
            else
            {
                foreach (var tree in _subtrees)
                    if (tree != null)
                        return false;
            }

            return true;
        }

        public bool ContainsBox(Vector3 position, Vector3 extents) => AabbContains(RootPosition, RootPosition + Vector3.One * CellWidth * 2, position, position + extents);
        public bool IntersectsBox(Vector3 position, Vector3 extents) => AabbIntersects(RootPosition, RootPosition + Vector3.One * CellWidth * 2, position, position + extents);
        public bool ContainsPoint(Vector3 position) => AabbContains(RootPosition, RootPosition + Vector3.One * CellWidth * 2, position);

        public bool CellIntersectsBox(int cell, Vector3 position, Vector3 extents)
        {
            Vector3 cellPos = IndexToVec3(cell) * CellWidth + RootPosition;
            return AabbIntersects(cellPos, cellPos + Vector3.One * CellWidth, position, position + extents);
        }
        public bool CellContainsPoint(int cell, Vector3 position)
        {
            Vector3 cellPos = IndexToVec3(cell) * CellWidth + RootPosition;
            return AabbContains(cellPos, cellPos + Vector3.One * CellWidth, position);
        }

        private static int VecToIndex(Vector3I vec) => (vec.X << 2) + (vec.Y << 1) + (vec.Z);
        private static Vector3I IndexToVec3I(int idx) => new Vector3I((idx & 0b100) >> 2, (idx & 0b010) >> 1, idx & 0b001);
        private static Vector3 IndexToVec3(int idx) => new Vector3((idx & 0b100) >> 2, (idx & 0b010) >> 1, idx & 0b001);

        private static bool VecAnyLt(Vector3 v1, Vector3 v2) => v1.X < v2.X || v1.Y < v2.Y || v1.Z < v2.Z;
        private static bool VecAnyGt(Vector3 v1, Vector3 v2) => v1.X > v2.X || v1.Y > v2.Y || v1.Z > v2.Z;

        private static bool AabbIntersects(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            bool passes = (aMin.X <= bMax.X && aMax.X >= bMin.X) &&
                          (aMin.Y <= bMax.Y && aMax.Y >= bMin.Y) &&
                          (aMin.Z <= bMax.Z && aMax.Z >= bMin.Z);

            return passes;
        }

        private static bool AabbContains(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            bool passes = (aMin.X <= bMin.X && aMax.X >= bMax.X) &&
                          (aMin.Y <= bMin.Y && aMax.Y >= bMax.Y) &&
                          (aMin.Z <= bMin.Z && aMax.Z >= bMax.Z);
            
            return passes;
        }

        private static bool AabbContains(Vector3 aMin, Vector3 aMax, Vector3 b)
        {
            bool passes = (aMin.X <= b.X && aMax.X > b.X) &&
                          (aMin.Y <= b.Y && aMax.Y > b.Y) &&
                          (aMin.Z <= b.Z && aMax.Z > b.Z);
            
            return passes;
        }
    }
}
