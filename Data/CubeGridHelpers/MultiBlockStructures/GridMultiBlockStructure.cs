using System;
using System.Collections.Generic;
using Godot;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;

namespace Stellacrum.Data.CubeGridHelpers
{
    /// <summary>
    /// Base class for a multi-block structure.
    /// </summary>
    public abstract partial class GridMultiBlockStructure : Node
    {
        private static protected Dictionary<string, Type> StructureTypeMap = new();

        /// <summary>
        /// Add reference for new structure type
        /// </summary>
        public static void AddStructureType(string name, Type type)
        {
            if (!StructureTypeMap.ContainsKey(name))
                StructureTypeMap.Add(name, type);
        }

        public static Type GetStructureType(string name)
        {
            if (!StructureTypeMap.ContainsKey(name))
                return null;
            return StructureTypeMap[name];
        }

        public static GridMultiBlockStructure New(string type, List<CubeBlock> blocks)
        {
            if (!StructureTypeMap.ContainsKey(type))
                return null;

            dynamic structure = Activator.CreateInstance(StructureTypeMap[type], args: new object[] { blocks });

            GD.Print("Created new structure of type " + type);

            return structure;
        }

        public static GridMultiBlockStructure New(string type)
        {
            return New(type, new());
        }


        protected List<CubeBlock> StructureBlocks = new();

        public GridMultiBlockStructure(List<CubeBlock> StructureBlocks)
        {
            foreach (var block in StructureBlocks)
                AddStructureBlock(block);

            
        }

        public List<CubeBlock> GetStructureBlocks() { return this.StructureBlocks; }

        /// <summary>
        /// Combines a structure into this one.
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public virtual bool Merge(GridTreeStructure structure)
        {
            if (structure.GetType() != GetType())
                return false;

            foreach (CubeBlock block in structure.StructureBlocks)
            {
                AddStructureBlock(block);
                structure.RemoveStructureBlock(block);
            }

            structure.Destroy();

            return true;
        }

        public virtual void AddStructureBlock(CubeBlock block)
        {
            if (block != null && !this.StructureBlocks.Contains(block))
            {
                this.StructureBlocks.Add(block);
                block.Structures.Add(this);
            }
        }

        public virtual void RemoveStructureBlock(CubeBlock block)
        {
            if (this.StructureBlocks.Contains(block))
            {
                this.StructureBlocks.Remove(block);
                if (block != null)
                    block.Structures.Remove(this);
            }

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

        public virtual void Init()
        {
            // Fail-safe
            if (StructureBlocks.Count == 0)
                Destroy();

            StructureBlocks[0].GetParent().AddChild(this);
        }

        public abstract void Update();
        public abstract void Update10();
        public abstract void Update60();
        public void Destroy()
        {
            try
            {
                GetParent().RemoveChild(this);
            }
            catch
            {
                GD.PrintErr("Failed to remove structure parent! Was this the last block?");
            }
            StructureBlocks = null;
            this.QueueFree();
        }
    }
}
