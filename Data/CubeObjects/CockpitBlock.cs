using Godot;
using Stellacrum.Data.CubeObjects;
using System;
using GameSceneObjects;

namespace Stellacrum.Data.CubeObjects
{
    public partial class CockpitBlock : CubeBlock, IHighlightableObject
    {
        public bool HasTerminal => true;
        public bool HasInventory => false;
        public bool IsSeat => true;

        public CockpitBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
        }

        public CockpitBlock() { }

        public override void _Process(double delta)
        {

        }

        public void OnInteract(player_character player)
        {
            player.TryEnter(this);
        }
    }
}