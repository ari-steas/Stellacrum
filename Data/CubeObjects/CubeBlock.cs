using Godot;
using System;
using System.Collections.Generic;

public partial class CubeBlock : StaticBody3D
{
	public CollisionShape3D collision { get; private set; } = new ();
	public List<MeshInstance3D> meshes { get; private set; } = new ();
	public string subTypeId { get; private set; } = "";

	private Vector3 size;

	public CubeBlock(string subTypeId, List<MeshInstance3D> meshes, Vector3 size)
	{
		this.meshes = meshes;
		this.size = size;

		BoxShape3D b = new ()
		{
			Size = size
		};
		collision.Shape = b;
		AddChild(collision);

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
		collision.Shape = b;
		AddChild(collision);

		BoxMesh bm = new ()
		{
			Size = new Vector3(2.5f, 2.5f, 2.5f)
		};

		meshes.Add(new() {Mesh = bm});
		foreach (MeshInstance3D mesh in meshes)
			AddChild(mesh);

		Position = (Vector3) gridPosition * 2.5f;

		Name = "CubeBlock." + GetIndex();
	}

	private CubeBlock() {}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public CubeBlock Copy()
	{
		return new CubeBlock(subTypeId, meshes, size)
		{
			Position = Vector3.Zero
		};
	}

	public static CubeBlock BlockFromID(string blockId)
	{
		CubeBlock block = CubeBlockLoader.FromId(blockId).Copy();

		return block;
	}

	public Aabb Size(Vector3I position)
	{
		Aabb size = new Aabb(position, Vector3I.One);
		return size;
	}
}
