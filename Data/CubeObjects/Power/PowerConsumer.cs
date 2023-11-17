using Godot;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using System;
using Godot.Collections;

namespace Stellacrum.Data.CubeObjects
{
    public partial class PowerConsumer : PowerConduit
    {
        /// <summary>
        /// Current maximum input.
        /// </summary>
        public float MaxInput
        {
            get { return _maxInput; }
            set
            {
                powerStructure?.AddPowerUsage(value - _maxInput);
                _maxInput = value > DefMaxInput ? _maxInput : value;
            }
        }
        private float _maxInput = 0;

        /// <summary>
        /// Definition maximum input.
        /// </summary>
        public readonly float DefMaxInput = 0;

        private bool _enabled = true;
        private bool _hasPower = false;

        public bool Enabled
        {
            get { return _enabled; }
            set { SetEnabled(value); }
        }
        public bool HasPower
        {
            get { return _hasPower; }
            set { SetPower(value); }
        }

        public PowerConsumer(string subTypeId, Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            ReadFromData(blockData, "MaxInput", ref DefMaxInput);
        }

        public override GridTreeStructure CheckConnectedBlocksOfType(string type)
        {
            GridTreeStructure s = base.CheckConnectedBlocksOfType(type);
            MaxInput = DefMaxInput;
            return s;
        }

        public void SetEnabled(bool value)
        {
            // Avoid double-pinging structure
            if (_enabled == value) return;

            _enabled = value;

            if (_enabled)
                powerStructure.AddPowerUsage(MaxInput);
            else
                powerStructure.AddPowerUsage(-MaxInput);
        }

        private void SetPower(bool value)
        {
            if (_hasPower == value) return;

            _hasPower = value;
        }

        public override void RemoveStructureRef(string type)
        {
            base.RemoveStructureRef(type);
            if (type == "Power")
                MaxInput = 0;
        }
    }
}
