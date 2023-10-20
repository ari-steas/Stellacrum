using Godot;
using Godot.Collections;
using Stellacrum.Data.CubeGridHelpers.MultiBlockStructures;
using System;

namespace Stellacrum.Data.CubeObjects
{
	public partial class GeneratorBlock : PowerConduit
	{
		public float MaxOutput = 0;
		private bool _enabled = true;
		public bool Enabled {
            get { return _enabled; }
            set { SetEnabled(value); }
        }
		private GridPowerStructure powerStructure;

        public GeneratorBlock(string subTypeId, Dictionary<string, Variant> blockData) : base(subTypeId, blockData)
        {
            ReadFromData(blockData, "MaxOutput", ref MaxOutput);
        }

		public void SetEnabled(bool value)
		{
			// Avoid double-pinging structure
			if (_enabled == value) return;

            _enabled = value;
			GetMemberStructure("Power");
		}
	}
}