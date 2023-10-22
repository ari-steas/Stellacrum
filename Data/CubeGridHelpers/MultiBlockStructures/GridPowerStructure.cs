using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellacrum.Data.CubeObjects;

namespace Stellacrum.Data.CubeGridHelpers.MultiBlockStructures
{
    /// <summary>
    /// Power structure type.
    /// </summary>
    public partial class GridPowerStructure : GridTreeStructure
    {
        public new const string StructureName = "Power";
        public override string GetStructureName() => StructureName;

        /// <summary>
        /// Power generation capacity, in MW
        /// </summary>
        public float PowerCapacity { get; private set; } = 0;

        /// <summary>
        /// Power consumption, in MW
        /// </summary>
        public float PowerUsage { get; private set; } = 0;

        private bool needsAvailabilityUpdate = true;

        public GridPowerStructure(List<CubeBlock> StructureBlocks) : base(StructureBlocks)
        {
        }

        public void AddPowerCapacity(float cap)
        {
            bool sufficientPower = PowerCapacity > PowerUsage;
            PowerCapacity += cap;
            if (!sufficientPower && PowerCapacity > PowerUsage)
                needsAvailabilityUpdate = true;
        }

        public void AddPowerUsage(float cap)
        {
            bool sufficientPower = PowerCapacity > PowerUsage;
            PowerUsage += cap;
            if (!sufficientPower && PowerCapacity > PowerUsage)
                needsAvailabilityUpdate = true;
        }

        public override bool Merge(GridTreeStructure structure)
        {
            bool r = base.Merge(structure);
            AddPowerCapacity(((GridPowerStructure) structure).PowerCapacity);
            AddPowerUsage(((GridPowerStructure)structure).PowerUsage);
            return r;
        }

        /// <summary>
        /// Notify all contained powerconsumers that power generation is sufficient or insufficent.
        /// </summary>
        internal void UpdatePowerAvailability()
        {
            bool sufficientPower = PowerCapacity > PowerUsage;
            foreach (var block in StructureBlocks)
                if (block is PowerConsumer consumer)
                    consumer.HasPower = sufficientPower;
        }

        public override bool AddStructureBlock(CubeBlock block)
        {
            if (!base.AddStructureBlock(block))
                return false;

            //if (block is GeneratorBlock generator)
            //    PowerCapacity += generator.MaxOutput;

            return true;
        }

        public override bool RemoveStructureBlock(CubeBlock block)
        {
            if (!base.RemoveStructureBlock(block))
                return false;

            //if (block is GeneratorBlock generator)
            //    PowerCapacity -= generator.MaxOutput;

            return true;
        }

        public override void Update()
        {
            base.Update();
            foreach (var block in StructureBlocks)
                if (block.IsInsideTree())
                    DebugDraw.Text3D(PowerCapacity - PowerUsage, block.GlobalPosition, 0, Colors.Magenta);

            if (needsAvailabilityUpdate)
            {
                UpdatePowerAvailability();
                needsAvailabilityUpdate = false;
            }
        }

        public override void Update60()
        {
            base.Update60();
            //GD.PrintErr($"C:{PowerCapacity}\nP:{PowerUsage}");
        }

        public override void Init()
        {
            base.Init();
            //GD.PrintErr("Power structure inited!");
        }
    }
}
