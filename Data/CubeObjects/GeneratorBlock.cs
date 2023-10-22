using Godot;
using Godot.Collections;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using System;

namespace Stellacrum.Data.CubeObjects
{
	public partial class GeneratorBlock : PowerConduit
	{
        /// <summary>
        /// Current maximum output.
        /// </summary>
		public float MaxOutput
        {
            get { return _maxOutput; }
            set
            { 
                powerStructure?.AddPowerCapacity(value - _maxOutput);
                _maxOutput = value > DefMaxOutput ? _maxOutput : value;
            }
        }
        private float _maxOutput = 0;

        /// <summary>
        /// Definition maximum output.
        /// </summary>
        readonly float DefMaxOutput = 0;

        public float CurrentOutputPercent { get; private set; } = 0;

		private bool _enabled = true;
		private bool _hasPower = false;

		public bool Enabled {
            get { return _enabled; }
            set { SetEnabled(value); }
        }

        public GeneratorBlock(string subTypeId, Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            ReadFromData(blockData, "MaxOutput", ref DefMaxOutput);
        }

        public override GridTreeStructure CheckConnectedBlocksOfType(string type)
        {
            GridTreeStructure s = base.CheckConnectedBlocksOfType(type);
            MaxOutput = DefMaxOutput;
            return s;
        }

        public void SetEnabled(bool value)
		{
			// Avoid double-pinging structure
			if (_enabled == value) return;

            _enabled = value;

            if (_enabled)
			    powerStructure.AddPowerCapacity(MaxOutput);
            else
                powerStructure.AddPowerCapacity(-MaxOutput);
        }

        public override void RemoveStructureRef(string type)
        {
            base.RemoveStructureRef(type);
            if (type == "Power")
                MaxOutput = 0;
        }
    }
}