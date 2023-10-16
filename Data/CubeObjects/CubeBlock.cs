using Godot;
using Stellacrum.Data.CubeGridHelpers;
using System;
using System.Collections.Generic;

public partial class CubeBlock : StaticBody3D
{
	public Shape3D collision;
	public uint collisionId = 0;
	public List<Node3D> meshes;
	public string subTypeId = "";
	public Vector3 size = Vector3.One*2.5f;
	public int Mass = 100;

    public CubeBlock() { }
    public List<GridMultiBlockStructure> Structures { get; private set; } = new List<GridMultiBlockStructure>();

    public CubeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
	{
        List<Node3D> model = ModelLoader.Models["ArmorBlock1x1"];
        Vector3 size = Vector3.One * 2.5f;
        int mass = 100;

        // Load model from ModelLoader
        if (blockData.ContainsKey("Model") && ModelLoader.Models.ContainsKey((string)blockData["Model"]))
            model = ModelLoader.Models[(string)blockData["Model"]];
        else
            GD.PrintErr($"Missing [Model] in {subTypeId}! Setting to default...");

        // Calc BlockSize
        try
        {
            int[] bSize = blockData["BlockSize"].AsInt32Array();
            size = new Vector3(bSize[0], bSize[1], bSize[2]) * 2.5f;
        }
        catch
        {
            GD.PrintErr($"Missing [Size] in {subTypeId}! Setting to default...");
		}

		try
		{
			mass = blockData["Mass"].AsInt32();
		}
		catch
		{
			GD.PrintErr($"Missing [Mass] in {subTypeId}! Setting to default...");
		}

        this.meshes = model;
        this.size = size;
        this.subTypeId = subTypeId;

        collision = new BoxShape3D()
        {
            Size = size
        };

        Mass = mass;

        foreach (Node3D mesh in model)
            AddChild(mesh.Duplicate());

        Name = "CubeBlock." + subTypeId + "." + GetIndex();
    }

    //public virtual CubeBlock Init(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
	//{
	//	List<Node3D> model = ModelLoader.Models["ArmorBlock1x1"];
	//	Vector3 size = Vector3.One*2.5f;
	//	int mass = 100;
	//
	//	// Load model from ModelLoader
	//	if (blockData.ContainsKey("Model") && ModelLoader.Models.ContainsKey((string) blockData["Model"]))
	//		model = ModelLoader.Models[(string) blockData["Model"]];
	//	else
	//		GD.PrintErr($"Missing [Model] in {subTypeId}! Setting to default...");
	//
	//	// Calc BlockSize
	//	try
	//	{
	//		int[] bSize = blockData["BlockSize"].AsInt32Array();
	//		size = new Vector3(bSize[0], bSize[1], bSize[2]) * 2.5f;
	//	}
	//	catch {
	//		GD.PrintErr($"Missing [Size] in {subTypeId}! Setting to default...");
	//	}
	//
	//	try
	//	{
	//		mass = blockData["Mass"].AsInt32();
	//	}
	//	catch {
	//		GD.PrintErr($"Missing [Mass] in {subTypeId}! Setting to default...");
	//	}
	//
	//	return new (subTypeId, model, size)
	//	{
	//		Mass = mass
	//	};
	//}
	
	private CubeBlock(string subTypeId, List<Node3D> model, Vector3 size)
	{
		this.meshes = model;
		this.size = size;
		this.subTypeId = subTypeId;

		collision = new BoxShape3D()
		{
			Size = size
		};

		foreach (Node3D mesh in model)
			AddChild(mesh.Duplicate());

		Name = "CubeBlock." + subTypeId + "." + GetIndex();
	}

	public Aabb Size(Vector3I position)
	{
		// Offset position to bottom-left corner
		position -= (Vector3I) (this.size / 5f);
        Aabb size = new(position, this.size/2.5f);
		return size;
	}

	public BoxShape3D BoxShape3D()
	{
		return new BoxShape3D()
		{ 
			Size = size,
		};
	}

	public Vector3I[] OccupiedSlots(Vector3I basePosition)
	{
        // Offset position to bottom-left corner
        basePosition -= (Vector3I)(this.size / 5f);

        List<Vector3I> slots = new();
		for (int z = basePosition.Z; z < size.Z / 2.5f + basePosition.Z; z++)
            for (int y = basePosition.Y; y < size.Y / 2.5f + basePosition.Y; y++)
                for (int x = basePosition.X; x < size.X / 2.5f + basePosition.X; x++)
					slots.Add(new Vector3I(x, y, z));

		return slots.ToArray();
    }

	public virtual void Close()
	{
		foreach (var structure in Structures)
			structure.RemoveStructureBlock(this);
	}

	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		Godot.Collections.Dictionary<string, Variant> data = new()
		{
			{ "SubTypeId", subTypeId },
			{ "Position", JsonHelper.StoreVec(Position) },
			{ "Rotation", JsonHelper.StoreVec(Rotation) },
		};

		return data;
	}
}
