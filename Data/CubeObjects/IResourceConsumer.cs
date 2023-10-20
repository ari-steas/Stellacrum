namespace Stellacrum.Data.CubeObjects
{
    internal interface IResourceConsumer
    {
        float CurrentRate { get; }
        float MaxRate { get; }
        float MinRate { get; }
    }
}