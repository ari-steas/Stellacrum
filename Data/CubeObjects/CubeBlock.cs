using Godot;
using System;
using System.Collections.Generic;

public partial class CubeBlock : StaticBody3D
{
	public Shape3D collision;
	public uint collisionId = 0;
	public List<MeshInstance3D> meshes;
	public string subTypeId = "";
	public Vector3 size = Vector3.One*2.5f;

	public virtual CubeBlock Init(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData)
	{
		List<MeshInstance3D> model = ModelLoader.Models["ArmorBlock1x1"];
		Vector3 size = Vector3.One*2.5f;

		// Load model from ModelLoader
		if (blockData.ContainsKey("Model") && ModelLoader.Models.ContainsKey((string) blockData["Model"]))
			model = ModelLoader.Models[(string) blockData["Model"]];
		else
			GD.PrintErr($"Missing [Model] in {subTypeId}! Setting to default...");

		// Calc BlockSize
		try
		{
			int[] bSize = blockData["BlockSize"].AsInt32Array();
			size = new Vector3(bSize[0], bSize[1], bSize[2]) * 2.5f;
		}
		catch {
			GD.PrintErr($"Missing [Size] in {subTypeId}! Setting to default...");
		}

		return new (subTypeId, model, size);
	}
	
	private CubeBlock(string subTypeId, List<MeshInstance3D> meshes, Vector3 size)
	{
		this.meshes = meshes;
		this.size = size;
		this.subTypeId = subTypeId;

		collision = new BoxShape3D()
		{
			Size = size
		};

		foreach (MeshInstance3D mesh in meshes)
			AddChild(mesh.Duplicate());

		Name = "CubeBlock." + subTypeId + "." + GetIndex();
	}

	public CubeBlock(Vector3I gridPosition)
	{
		BoxShape3D b = new ()
		{
			Size = new Vector3(2.5f, 2.5f, 2.5f)
		};
		collision = b;

		BoxMesh bm = new ()
		{
			Size = new Vector3(2.5f, 2.5f, 2.5f)
		};

		meshes.Add(new() {Mesh = bm});
		foreach (MeshInstance3D mesh in meshes)
			AddChild(mesh);

		Position = (Vector3) gridPosition * 2.5f;

		Name = "CubeBlock." + subTypeId + "." + GetIndex();
	}

	public CubeBlock() {}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MoveAndCollide(ConstantLinearVelocity * (float)delta);
	}

	public virtual CubeBlock Copy()
	{
		CubeBlock b = (CubeBlock) Duplicate();
		b.Position = Vector3.Zero;
		b.collision = collision;
		b.collisionId = collisionId;
		b.subTypeId = subTypeId;
		b.size = size;

		b.Name = "CubeBlock." + subTypeId + "." + b.GetIndex();
		return b;
	}

	public virtual Aabb Size(Vector3I position)
	{
		Aabb size = new Aabb(position, Vector3I.One);
		return size;
	}
}
