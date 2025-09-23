using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Stellacrum.Data.CubeObjects;

namespace Stellacrum.Data.CubeGridHelpers
{
    public class GridOctree
    {
        /// <summary>
        /// Cell width in meters
        /// </summary>
        public readonly float CellWidth;
        public Vector3 RootPosition { get; protected set; }
        public GridOctree Parent;

        protected GridOctree[] _subtrees = new GridOctree[8];
        protected CubeBlock[] _blocks = new CubeBlock[8];

        public IEnumerable<GridOctree> Subtrees => _subtrees;

        public GridOctree(Vector3 rootPosition, float cellWidth, GridOctree parent)
        {
            RootPosition = rootPosition;
            CellWidth = cellWidth;
            Parent = parent;
        }

        /// <summary>
        /// Adds a new subtree to this tree's hierarchy.
        /// </summary>
        /// <param name="idx"></param>
        /// <exception cref="ArgumentException">Thrown if the cell is already occupied.</exception>
        public GridOctree AddSubtree(Vector3I vec)
        {
            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            if (_subtrees[idx] != null || _blocks[idx] != null)
                throw new ArgumentException($"Cell index {idx} is already occupied.");
            _subtrees[idx] = new GridOctree(RootPosition + (Vector3) vec * CellWidth, CellWidth/2, this);
            GD.Print($"New subtree: {vec} offset @ {CellWidth}m: {RootPosition} -> {_subtrees[idx].RootPosition}");

            return _subtrees[idx];
        }

        /// <summary>
        /// Creates a new octree containing an existing tree.
        /// </summary>
        /// <param name="idx"></param>
        /// <exception cref="ArgumentException">Thrown if the cell is already occupied.</exception>
        public static GridOctree AddSupertree(Vector3I vec, GridOctree tree)
        {
            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            GridOctree superTree = new GridOctree(tree.RootPosition - (Vector3) vec * tree.CellWidth * 2, tree.CellWidth * 2, null);
            superTree._subtrees[idx] = tree;
            tree.Parent = superTree;
            GD.Print($"New supertree: {vec} offset @ {tree.CellWidth}m: {tree.RootPosition} -> {superTree.RootPosition}");
            return superTree;
        }

        public void SetBlock(Vector3I vec, CubeBlock block)
        {
            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            if (_subtrees[idx] != null)
                throw new ArgumentException($"Cell index {idx} is already occupied.");
            _blocks[idx] = block;
        }

        public void SetTree(Vector3I vec, GridOctree tree)
        {
            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            if (_blocks[idx] != null)
                throw new ArgumentException($"Cell index {idx} is already occupied.");
            _subtrees[idx] = tree;
        }

        public void RemoveAt(Vector3I vec)
        {
            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            _subtrees[idx] = null;
            _blocks[idx] = null;
        }

        /// <summary>
        /// Item at internal position.
        /// </summary>
        /// <param name="vec">Scales from 0,0,0 to 1,1,1</param>
        /// <param name="tree"></param>
        /// <param name="block"></param>
        public void ItemAt(Vector3I vec, out GridOctree tree, out CubeBlock block)
        {
            // 0,0,0 = 0
            // 0,0,1 = 1
            // 0,1,0 = 2
            // 0,1,1 = 3
            // 1,0,0 = 4
            // 1,0,1 = 5
            // 1,1,0 = 6
            // 1,1,1 = 7

            int idx = (vec.X << 2) + (vec.Y << 1) + (vec.Z);
            tree = _subtrees[idx];
            block = _blocks[idx];
        }

        public bool TryGetBlockAt(Vector3 vec, out CubeBlock block)
        {
            block = null;

            vec -= RootPosition;
            if (vec.X+1 > CellWidth * 2 || vec.X < 0)
                return false;
            if (vec.Y+1 > CellWidth * 2 || vec.Y < 0)
                return false;
            if (vec.Z+1 > CellWidth * 2 || vec.Z < 0)
                return false;

            GridOctree tree = this;

            int depth = 0;
            while (true)
            {
                GD.Print($"[{++depth}] Size: {tree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / tree.CellWidth);

                GridOctree subTree = tree._subtrees[(searchPosition.X << 2) + (searchPosition.Y << 1) + (searchPosition.Z)];
                vec -= (Vector3) searchPosition * tree.CellWidth;
                if (subTree == null)
                    break;

                tree = subTree;
            }

            Vector3I blockPosition = (Vector3I)(vec / tree.CellWidth);
            block = tree._blocks[(blockPosition.X << 2) + (blockPosition.Y << 1) + (blockPosition.Z)];
            return true;
        }

        public CubeBlock BlockAt(Vector3 vec, out GridOctree tree)
        {
            vec -= RootPosition;
            tree = null;
            if (vec.X+1 > CellWidth * 2 || vec.X < 0)
                return null;
            if (vec.Y+1 > CellWidth * 2 || vec.Y < 0)
                return null;
            if (vec.Z+1 > CellWidth * 2 || vec.Z < 0)
                return null;

            tree = this;

            int depth = 0;
            while (true)
            {
                GD.Print($"[{++depth}] Size: {tree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / tree.CellWidth);

                GridOctree subTree = tree._subtrees[(searchPosition.X << 2) + (searchPosition.Y << 1) + (searchPosition.Z)];
                vec -= (Vector3) searchPosition * tree.CellWidth;
                if (subTree == null)
                    break;

                tree = subTree;
            }

            Vector3I blockPosition = (Vector3I)(vec / tree.CellWidth);
            return tree._blocks[(blockPosition.X << 2) + (blockPosition.Y << 1) + (blockPosition.Z)];
        }

        /// <summary>
        /// ASSUMES POSITIVE EXTENTS
        /// </summary>
        /// <param name="position"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public void BlocksInVolume(Vector3 position, Vector3 extents, ref HashSet<CubeBlock> cubes)
        {
            Vector3 relative = position - RootPosition;
            if (relative.X+1 > CellWidth * 2 || relative.X < 0)
                return;
            if (relative.Y+1 > CellWidth * 2 || relative.Y < 0)
                return;
            if (relative.Z+1 > CellWidth * 2 || relative.Z < 0)
                return;

            // Narrow down to smallest fully enveloping octree
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            toCheck.Push(SmallestTreeForVolume(position, extents));

            // Check trees internal to the volume
            int count = 0;
            Vector3 slack = Vector3.One * CubeGrid.MinGridSize/4;
            while (toCheck.TryPop(out GridOctree current))
            {
                GD.Print($"[{++count}] Size: {current.CellWidth:F}m");

                foreach (var subtree in current._subtrees)
                {
                    if (subtree == null)
                        continue;

                    for (int i = 0; i < current._blocks.Length; i++)
                    {
                        if (current._blocks[i] == null)
                            continue;

                        Vector3 cellPos = new Vector3(i & 0b100, i & 0b010, i & 0b001) * current.CellWidth + current.RootPosition;
                        if (cellPos >= position - slack && cellPos - position <= extents + slack)
                            cubes.Add(current._blocks[i]);
                    }

                    for (int i = 0; i < current._subtrees.Length; i++)
                    {
                        if (current._subtrees[i] == null)
                            continue;

                        Vector3 cellPos = new Vector3(i & 0b100, i & 0b010, i & 0b001) * current.CellWidth + current.RootPosition;
                        if (cellPos >= position - slack && cellPos - position <= extents + slack)
                            toCheck.Push(current._subtrees[i]);
                    }
                }
            }
        }

        /// <summary>
        /// ASSUMES POSITIVE EXTENTS
        /// </summary>
        /// <param name="position"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public bool HasBlocksInVolume(Vector3 position, Vector3 extents)
        {
            Vector3 relative = position - RootPosition;
            if (relative.X+1 > CellWidth * 2 || relative.X < 0)
                return false;
            if (relative.Y+1 > CellWidth * 2 || relative.Y < 0)
                return false;
            if (relative.Z+1 > CellWidth * 2 || relative.Z < 0)
                return false;

            // Narrow down to smallest fully enveloping octree
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            toCheck.Push(SmallestTreeForVolume(position, extents));

            // Check trees internal to the volume
            int count = 0;
            Vector3 slack = Vector3.One * CubeGrid.MinGridSize/4;
            while (toCheck.TryPop(out GridOctree current))
            {
                GD.Print($"[{++count}] Size: {current.CellWidth:F}m");

                foreach (var subtree in current._subtrees)
                {
                    if (subtree == null)
                        continue;

                    for (int i = 0; i < current._blocks.Length; i++)
                    {
                        if (current._blocks[i] == null)
                            continue;

                        Vector3 cellPos = new Vector3(i & 0b100, i & 0b010, i & 0b001) * current.CellWidth + current.RootPosition;
                        if (cellPos >= position - slack && cellPos - position <= extents + slack)
                            return true;
                    }

                    for (int i = 0; i < current._subtrees.Length; i++)
                    {
                        if (current._subtrees[i] == null)
                            continue;

                        Vector3 cellPos = new Vector3(i & 0b100, i & 0b010, i & 0b001) * current.CellWidth + current.RootPosition;
                        if (cellPos >= position - slack && cellPos - position <= extents + slack)
                            toCheck.Push(current._subtrees[i]);
                    }
                }
            }

            return false;
        }

        public bool SetBlockAt(Vector3 position, Basis rotation, CubeBlock block)
        {
            Vector3 relative = position - RootPosition;
            if (relative.X+1 > CellWidth * 2 || relative.X < 0)
                return false;
            if (relative.Y+1 > CellWidth * 2 || relative.Y < 0)
                return false;
            if (relative.Z+1 > CellWidth * 2 || relative.Z < 0)
                return false;

            Vector3 extents = block.size * rotation;

            // Narrow down to smallest fully enveloping octree
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            toCheck.Push(SmallestTreeForVolume(position, extents));

            // Check trees internal to the volume
            int count = 0;
            float minExtents = Math.Min(Math.Min(Math.Abs(extents.X), Math.Abs(extents.Y)), Math.Abs(extents.Z));
            Vector3 slack = Vector3.One * CubeGrid.MinGridSize/4;
            bool didAdd = false;
            while (toCheck.TryPop(out GridOctree current))
            {
                GD.Print($"[{++count}] SetBlockAt Size: {current.CellWidth:F}m (block {minExtents}, matches: {current.CellWidth <= minExtents})");

                if (Math.Abs(current.CellWidth - minExtents) <= CubeGrid.MinGridSize/4)
                {
                    for (int i = 0; i < current._blocks.Length; i++)
                    {
                        Vector3 cellPos = new Vector3((i & 0b100) >> 2, (i & 0b010) >> 1, i & 0b001) * current.CellWidth + current.RootPosition;
                        GD.Print($"    SBA CHKB [{i}] {new Vector3((i & 0b100) >> 2, (i & 0b010) >> 1, i & 0b001)}m for {cellPos >= position - slack} && {cellPos - position <= extents + slack}");

                        if (cellPos >= position - slack && cellPos - position <= extents + slack)
                        {
                            current._blocks[i] = block;
                            current._subtrees[i] = null;
                            if (!block.ContainedOctrees.Contains(current))
                                block.ContainedOctrees.Add(current);
                            didAdd = true;
                        }
                    }
                    continue;
                }

                for (var i = 0; i < current._subtrees.Length; i++)
                {
                    var subtree = current._subtrees[i];

                    Vector3 cellPos = new Vector3(i & 0b100, i & 0b010, i & 0b001) * current.CellWidth + current.RootPosition;

                    GD.Print($"    SBA CHKS [{i}] {new Vector3((i & 0b100) >> 2, (i & 0b010) >> 1, i & 0b001)}m for {cellPos >= position - slack} && {cellPos - position <= extents + slack}");

                    if (!(cellPos >= position - slack && cellPos - position <= extents + slack))
                        continue;

                    if (subtree == null)
                        subtree = AddSubtree(new Vector3I(i & 0b100, i & 0b010, i & 0b001));

                    toCheck.Push(subtree);
                }
            }

            return didAdd;
        }

        public void RemoveBlock(CubeBlock block)
        {
            Stack<GridOctree> toCheck = new Stack<GridOctree>();
            foreach (var tree in block.ContainedOctrees)
                toCheck.Push(tree);

            while (toCheck.TryPop(out GridOctree tree))
            {
                if (tree.Parent == null)
                    continue;

                bool hasOthers = false;
                for (int i = 0; i < 8; i++)
                {
                    if (tree._blocks[i] == block)
                        tree._blocks[i] = null;
                    else
                        hasOthers = true;

                    if (tree._subtrees[i] != null)
                        hasOthers = true;
                }

                if (!hasOthers)
                {
                    toCheck.Push(tree.Parent);
                    for (int i = 0; i < tree.Parent._blocks.Length; i++)
                    {
                        if (tree.Parent._subtrees[i] == tree)
                            tree.Parent._subtrees[i] = null;
                        break;
                    }

                    tree.Parent = null;
                }
            }
        }

        private bool SetSingleBlockAt(Vector3 vec, CubeBlock block)
        {
            vec -= RootPosition;
            if (vec.X+1 > CellWidth * 2 || vec.X < 0)
                return false;
            if (vec.Y+1 > CellWidth * 2 || vec.Y < 0)
                return false;
            if (vec.Z+1 > CellWidth * 2 || vec.Z < 0)
                return false;

            GridOctree tree = this;

            int depth = 0;
            while (true)
            {
                GD.Print($"[{++depth}] Size: {tree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / tree.CellWidth);

                GridOctree subTree = tree._subtrees[(searchPosition.X << 2) + (searchPosition.Y << 1) + (searchPosition.Z)];
                vec -= (Vector3) searchPosition * tree.CellWidth;
                if (subTree == null)
                    break;

                tree = subTree;
            }

            Vector3I blockPosition = (Vector3I)(vec / tree.CellWidth);
            tree._blocks[(blockPosition.X << 2) + (blockPosition.Y << 1) + (blockPosition.Z)] = block;

            return true;
        }

        public GridOctree TreeAt(Vector3 vec)
        {
            vec -= RootPosition;
            if (vec.X+1 > CellWidth * 2 || vec.X < 0)
                return null;
            if (vec.Y+1 > CellWidth * 2 || vec.Y < 0)
                return null;
            if (vec.Z+1 > CellWidth * 2 || vec.Z < 0)
                return null;

            GridOctree tree = this;

            int depth = 0;
            while (true)
            {
                GD.Print($"[{++depth}] Size: {tree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / tree.CellWidth);

                GridOctree subTree = tree._subtrees[(searchPosition.X << 2) + (searchPosition.Y << 1) + (searchPosition.Z)];
                if (subTree == null)
                    return tree;

                vec -= (Vector3) searchPosition * tree.CellWidth;
                tree = subTree;
            }
        }

        public GridOctree SmallestTreeForVolume(Vector3 position, Vector3 extents)
        {
            position -= RootPosition;
            if (position.X+1 > CellWidth * 2 || position.X < 0)
                return null;
            if (position.Y+1 > CellWidth * 2 || position.Y < 0)
                return null;
            if (position.Z+1 > CellWidth * 2 || position.Z < 0)
                return null;

            // Narrow down to smallest fully enveloping octree
            GridOctree rootTree = this;
            Vector3 vec = position;
            float maxExtent = Math.Max(Math.Max(Math.Abs(extents.X), Math.Abs(extents.Y)), Math.Abs(extents.Z));

            int depth = 0;
            while (rootTree.CellWidth > maxExtent)
            {
                GD.Print($"[{++depth}] Size: {rootTree.CellWidth:F}m");
                Vector3I searchPosition = (Vector3I)(vec / rootTree.CellWidth);

                GridOctree subTree = rootTree._subtrees[(searchPosition.X << 2) + (searchPosition.Y << 1) + (searchPosition.Z)];
                vec -= (Vector3) searchPosition * rootTree.CellWidth;
                if (subTree == null)
                    return rootTree;

                rootTree = subTree;
            }

            return this;
        }

        public bool PointInVolume(Vector3 point)
        {
            return point >= RootPosition && (point - RootPosition).X <= CellWidth * 2 && (point - RootPosition).Y <= CellWidth * 2 && (point - RootPosition).Z <= CellWidth * 2;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < _subtrees.Length; i++)
                if (_subtrees[i] != null)
                    return false;
            for (int i = 0; i < _blocks.Length; i++)
                if (_blocks[i] != null)
                    return false;

            return true;
        }
    }
}
