using Godot;
using System;
using System.Collections.Generic;

public partial class CubeGrid : Node3D
{
	public Aabb size { get; private set; } = new Aabb();
	private readonly Dictionary<Vector3I, CubeBlock> CubeBlocks = new ();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	float rotate = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Aabb BoundingBox()
	{
		return new Aabb(Position + size.Position*2.5f - Vector3.One*1.25f, size.End*2.5f);
	}

	public void AddBlock(RayCast3D ray, Vector3 rotation, string blockId)
	{
		AddBlock(ray.GetCollisionPoint() + ray.GetCollisionNormal(), rotation, blockId);
	}

	public void AddBlock(Vector3 globalPosition, Vector3 rotation, string blockId)
	{
		AddBlock(GlobalToBlockCoord(globalPosition), rotation, blockId);
	}

	public void AddBlock(Vector3I position, Vector3 rotation, string blockId)
	{
		AddBlock(position, rotation, CubeBlock.BlockFromID(blockId));
	}

	public void AddBlock(Vector3I position, Vector3 rotation, CubeBlock block)
	{
		if (!CubeBlocks.ContainsKey(position))
		{
			block.Position = (Vector3) position*2.5f;
			block.Rotation = rotation;
			AddChild(block);
			CubeBlocks.Add(position, block);

			RecalcSize();
			DebugDraw.Text("Placed block @ " + position, 5);
		}
		else
		{
			DebugDraw.Text("Failed to place block @ " + position, 5);
		}
	}

	public void RemoveBlock(RayCast3D ray)
	{
		RemoveBlock(ray.GetCollisionPoint() - ray.GetCollisionNormal());
	}

	public void RemoveBlock(Vector3 globalPosition)
	{
		RemoveBlock(GlobalToBlockCoord(globalPosition));
	}

	public void RemoveBlock(Vector3I position)
	{
		if (CubeBlocks.ContainsKey(position))
		{
			RemoveChild(CubeBlocks[position]);
			CubeBlocks.Remove(position);

			RecalcSize();
		}
	}

	private void RecalcSize()
	{
		Vector3I max = Vector3I.Zero;
		Vector3I min = Vector3I.Zero;

		foreach (var pos in CubeBlocks.Keys)
		{
			Aabb block = CubeBlocks[pos].Size(pos);
			
			if (block.End.X > max.X)
				max.X = (int) block.End.X;
			if (block.End.Y > max.Y)
				max.Y = (int) block.End.Y;
			if (block.End.Z > max.Z)
				max.Z = (int) block.End.Z;

			if (block.Position.X < min.X)
				min.X = (int) block.Position.X;
			if (block.Position.Y < min.Y)
				min.Y = (int) block.Position.Y;
			if (block.Position.Z < min.Z)
				min.Z = (int) block.Position.Z;
			
		}

		//GD.Print(size + " -> " + new Aabb(min, max));
		size = new Aabb(min, max);
	}

	public Vector3 RoundGlobalCoord(Vector3 global)
	{
		return ToGlobal((Vector3) GlobalToBlockCoord(global) * 2.5f);
	}

	public Vector3 PlaceProjectionGlobal(Vector3 from, Vector3 to)
	{
		Aabb box = BoundingBox();
		//rotate += 1f/60f;
		//Vector3 r = Vector3.Up.Rotated(Vector3.Right, rotate);
		//Vector3 pos = box.GetSupport(r);
		//
		//DebugDraw.Point(pos);
		//DebugDraw.Line(box.GetCenter(), box.GetCenter() + r);
		//DebugDraw.Text(rotate + ", " + pos);
		//DebugDraw.Text("Looking at " + GlobalToBlockCoord(to));

		for (int i = 0; i < 8; i++)
			DebugDraw.Point(box.GetEndpoint(i), 0.5f, Colors.Red);

		Vector3 lTo = RoundGlobalCoord(to);
		lTo = RoundGlobalCoord(lTo.MoveToward(from, 1.25f));
		return lTo;
	}

	public Vector3 PlaceProjectionGlobal(RayCast3D ray)
	{
		Aabb box = BoundingBox();

		for (int i = 0; i < 8; i++)
			DebugDraw.Point(box.GetEndpoint(i), 0.5f, Colors.Red);

		return RoundGlobalCoord(ray.GetCollisionPoint() + ray.GetCollisionNormal());
	}

	public Vector3I GlobalToBlockCoord(Vector3 global)
	{
		return (Vector3I) (ToLocal(global)/2.5f).Round();
	}
}
