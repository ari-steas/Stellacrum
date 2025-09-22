namespace Stellacrum.Data.CubeObjects;

public interface IHighlightableObject
{
	bool HasTerminal { get; }
	bool HasInventory { get; }
	bool IsSeat { get; }
}