using GameSceneObjects;

namespace Stellacrum.Data.CubeObjects;

public interface IHighlightableObject
{
	bool HasTerminal { get; }
	bool HasInventory { get; }
	bool IsSeat { get; }

    void OnInteract(player_character player);
}