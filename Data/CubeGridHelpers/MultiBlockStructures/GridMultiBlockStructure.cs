using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using Stellacrum.Data.CubeObjects;

namespace Stellacrum.Data.CubeGridHelpers
{
    /// <summary>
    /// Base class for a multi-block structure. Requires a set static StructureName.
    /// </summary>
    public abstract partial class GridMultiBlockStructure : Node
    {
        private static protected Dictionary<string, Type> StructureTypeMap = new();

        public const string StructureName = "MultiBlock";
        public virtual string GetStructureName() => StructureName;

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

        /// <summary>
        /// Create new GridMultiBlockStructure of type [type] with contents [blocks].
        /// </summary>
        /// <param name="type"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static GridMultiBlockStructure New(string type, List<CubeBlock> blocks)
        {
            if (!StructureTypeMap.ContainsKey(type))
                return null;

            dynamic structure = Activator.CreateInstance(StructureTypeMap[type], args: new object[] { blocks });

            GD.Print("Created new structure of type " + type);

            return structure;
        }

        /// <summary>
        /// Create new GridMultiBlockStructure of type [type].
        /// </summary>
        /// <param name="type"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static GridMultiBlockStructure New(string type) => New(type, new());

        /// <summary>
        /// Register all structure classes for later use.
        /// </summary>
        public static void FindStructureTypes()
        {
            StructureTypeMap.Clear();
            List<Type> allTypes = ReflectiveEnumerator.GetEnumerableOfType<GridMultiBlockStructure>();
            foreach (var type in allTypes)
                if (type.GetFields()[0].GetValue(null) is string structureName)
                    StructureTypeMap.Add(structureName, type);
        }

        public static void ClearStructureTypes() => StructureTypeMap.Clear();


        protected List<CubeBlock> StructureBlocks = new();

        public GridMultiBlockStructure(List<CubeBlock> StructureBlocks)
        {
            foreach (var block in StructureBlocks)
                AddStructureBlock(block);
        }

        public List<CubeBlock> GetStructureBlocks() => StructureBlocks;

        /// <summary>
        /// Combines a structure into this one.
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public virtual bool Merge(GridTreeStructure structure)
        {
            if (structure.GetType() != GetType() || structure == this)
                return false;

            foreach (CubeBlock block in structure.StructureBlocks)
            {
                AddStructureBlock(block);
                //CallDeferred("AddStructureBlock", block);
                //structure.CallDeferred("RemoveStructureBlock", block);
            }

            structure.Destroy();

            return true;
        }

        public virtual bool AddStructureBlock(CubeBlock block)
        {
            if (IsQueuedForDeletion() || block == null || StructureBlocks.Contains(block))
                return false;

            StructureBlocks.Add(block);
            block.AddStructureRef(GetStructureName(), this);

            return true;
        }

        public virtual bool RemoveStructureBlock(CubeBlock block)
        {
            if (IsQueuedForDeletion() || block == null)
                return false;

            if (StructureBlocks.Contains(block))
            {
                StructureBlocks.Remove(block);
                block?.RemoveStructureRef(GetStructureName());
            }

            if (StructureBlocks.Count == 0)
                Destroy();

            return true;
        }

        private long updateCounter = 0;
        public override void _PhysicsProcess(double delta)
        {
            if (IsQueuedForDeletion())
                return;
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
            QueueFree();
        }
    }
}
