using System;
using System.Collections.Generic;
using Godot;

namespace Stellacrum.Data.CubeGridHelpers
{
    /// <summary>
    /// Base class for a multi-block structure.
    /// </summary>
    public abstract partial class GridMultiBlockStructure : Node
    {
        protected List<CubeBlock> StructureBlocks;

        public GridMultiBlockStructure(List<CubeBlock> StructureBlocks)
        {
            this.StructureBlocks = StructureBlocks;
            foreach (var block in StructureBlocks)
                block.Structures.Add(this);
        }

        public List<CubeBlock> GetStructureBlocks() { return this.StructureBlocks; }
        public void AddStructureBlock(CubeBlock block)
        {
            if (!this.StructureBlocks.Contains(block))
                this.StructureBlocks.Add(block);
        }

        public void RemoveStructureBlock(CubeBlock block)
        {
            if (this.StructureBlocks.Contains(block))
                this.StructureBlocks.Remove(block);

            if (this.StructureBlocks.Count == 0)
                Destroy();
        }

        private long updateCounter = 0;
        public override void _Process(double delta)
        {
            updateCounter++;
            Update();
            if (updateCounter % 10 == 0)
            {
                Update10();
            }
            if (updateCounter == 60)
            {
                Update60();
                updateCounter = 0;
            }
        }

        public void Init()
        {
            StructureBlocks[0].GetParent().AddChild(this);
        }

        public abstract void Update();
        public abstract void Update10();
        public abstract void Update60();
        public void Destroy()
        {
            GetParent().RemoveChild(this);
            StructureBlocks = null;
            this.QueueFree();
        }
    }
}
