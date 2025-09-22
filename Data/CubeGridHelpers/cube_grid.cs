using Godot;
using Stellacrum.Data.CubeGridHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stellacrum.Data.CubeObjects;
using Stellacrum.Data.ObjectLoaders;

public partial class CubeGrid : RigidBody3D
{
	public CubeGrid ParentGrid;

	public Aabb Size { get; private set; } = new Aabb();
	public float Speed { get; private set; } = 0;
	public readonly List<CockpitBlock> Cockpits = new();

	public Vector3 MovementInput = Vector3.Forward;
	public Vector3 DesiredRotation = Vector3.Forward;
	public GridThrustControl ThrustControl { get; private set; } = new();

	bool underControl = false;

	readonly Dictionary<Vector3I, CubeBlock> CubeBlocks = new ();
	readonly List<Vector3I> OccupiedBlocks = new ();
	readonly List<ThrusterBlock> ThrusterBlocks = new();
	readonly internal List<CubeGrid> subGrids = new();

	private float OwnMass = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DesiredRotation = Rotation;
		ThrustControl.Init(ThrusterBlocks);

		ParentGrid = GetParentGrid(this);
		ParentGrid?.subGrids.Add(this);
	}

	/// <summary>
	/// Attempts to find parent of [grid]. If no parent exists, returns null.
	/// </summary>
	/// <param name="grid"></param>
	/// <returns></returns>
	private static CubeGrid? GetParentGrid(Node grid)
	{
		Node parent = grid.GetParent();
		if (parent == null)
			return null;
		if (parent is CubeGrid pGrid)
			return pGrid;
		return GetParentGrid(parent);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Vector3 dRot = underControl ? angularPID.Update(AngularVelocity, DesiredRotation, (float) delta) : Vector3.Zero;

		if (underControl)
		{
			ThrustControl.SetInputLinear(Basis * MovementInput);
			ThrustControl.SetInputAngular(DesiredRotation);
		}

		ThrustControl.Update(LinearVelocity, AngularVelocity, delta);
		Speed = LinearVelocity.Length();
	}

	public override void _PhysicsProcess(double delta)
	{
		// DebugDraw in PhysicsProcess to update 60fps
		DebugDraw.Point(ToGlobal(CenterOfMass), 1, Colors.Yellow);
		DebugDraw.Text3D(Name + ": " + Mass, ToGlobal(CenterOfMass));
	}

	public void ControlGrid()
	{
		underControl = true;
	}

	public void ReleaseControl()
	{
		underControl = false;
		ThrustControl.SetInputLinear(Vector3.Zero);
		ThrustControl.SetInputAngular(Vector3.Zero);
	}

	public Aabb BoundingBox()
	{
		return new Aabb(Position + Size.Position*2.5f - Vector3.One*1.25f, Size.End*2.5f);
	}

	#region blocks


	public readonly bool[] GridMirrors = new[] {false, false, false};
	public bool MirrorEnabled = false;
	public Vector3I MirrorPosition = Vector3I.Zero;

	public void AddBlock(RayCast3D ray, Vector3 rotation, string blockId)
	{
		AddBlock(PlaceProjectionGlobal(ray, CubeBlockLoader.ExistingBaseFromId(blockId).size), rotation, blockId);
	}

	public void AddBlock(Vector3 globalPosition, Vector3 rotation, string blocKid)
	{
		AddBlock(GlobalToGridCoordinates(globalPosition), rotation, blocKid);
	}

	public void AddBlock(Vector3I position, Vector3 rotation, string blocKid)
	{
		AddBlock(position, rotation, CubeBlockLoader.BlockFromId(blocKid));
	}

	public void AddBlock(Vector3I position_GridLocal, Vector3 rotation, CubeBlock block)
    {
        // Check for intersection in existing blocks
        if (CubeBlocks.ContainsKey(position_GridLocal))
            return;

		// Expensive check for intersection
		Vector3I[] blockPositions = block.OccupiedSlots(position_GridLocal);
        foreach (Vector3I blockPos in blockPositions)
			if (OccupiedBlocks.Contains(blockPos))
				return;

        AddChild(block);
        CubeBlocks.Add(position_GridLocal, block);

		// Add to occupied slots
		OccupiedBlocks.AddRange(blockPositions);

        block.Position = (Vector3)position_GridLocal * 2.5f;
		// If this is called when there are zero blocks (i.e. this is first block on grid), Global values throw an error (as they don't exist yet)
		if (CubeBlocks.Count > 1)
			block.GlobalRotation = rotation;

        block.collisionId = CreateShapeOwner(this);
        ShapeOwnerAddShape(block.collisionId, block.collision);
        ShapeOwnerSetTransform(block.collisionId, block.Transform);

        Mass += block.Mass;
        OwnMass += block.Mass;

        RecalcSize();
        RecalcMass();

        if (block is CockpitBlock c)
            Cockpits.Add(c);
        if (block is ThrusterBlock t)
            ThrusterBlocks.Add(t);

        try
        {
            // Place mirrored blocks
            if (MirrorEnabled)
            {
                Vector3I diff = MirrorPosition - position_GridLocal;

                // Flip along Y axis
                block.RotationDegrees += block.Basis * new Vector3(180, 0, 0);
                if (GridMirrors[1])
                    AddBlock(new(position_GridLocal.X, diff.Y, position_GridLocal.Z), block.GlobalRotation, block);
                block.GlobalRotation = rotation;

                // Flip along X axis
                block.GlobalRotate(Basis * Vector3.Forward, Mathf.Pi);
                block.GlobalRotate(Basis * Vector3.Right, Mathf.Pi);
                if (GridMirrors[0])
                    AddBlock(new(diff.X, position_GridLocal.Y, position_GridLocal.Z), block.GlobalRotation, block);
                block.GlobalRotation = rotation;

                // Flip along Z axis
                block.RotationDegrees += block.Basis * new Vector3(180, 0, 0);
                if (GridMirrors[2])
                    AddBlock(new(position_GridLocal.X, position_GridLocal.Y, diff.Z), block.GlobalRotation, block);
                block.GlobalRotation = rotation;
            }
        }
        catch
        {

        }
    }

    /// <summary>
    /// Adds CubeBlock to grid. This method is to be used with fully constructed blocks ONLY, such as those loaded from save files.
    /// </summary>
    /// <param name="block"></param>
    public void AddFullBlock(CubeBlock block)
    {
        Vector3I position_GridLocal = LocalToGridCoordinates(block.Position);

		// Override existing block if exists
		FullRemoveBlock(position_GridLocal);

        AddChild(block);
        CubeBlocks.Add(position_GridLocal, block);

        // Add to occupied slots
        OccupiedBlocks.AddRange(block.OccupiedSlots(position_GridLocal));

        // Add to collision hull
        block.collisionId = CreateShapeOwner(this);
        ShapeOwnerAddShape(block.collisionId, block.collision);
        ShapeOwnerSetTransform(block.collisionId, block.Transform);

        OwnMass += block.Mass;

        RecalcSize();
        RecalcMass();

        if (block is CockpitBlock c)
            Cockpits.Add(c);
        if (block is ThrusterBlock t)
            ThrusterBlocks.Add(t);
    }

	#nullable enable
	public CubeBlock? BlockAt(RayCast3D ray)
	{
		if (ray == null) return null;
		return BlockAt(GlobalToGridCoordinates(ray.GetCollisionPoint() - ray.GetCollisionNormal()));
    }

    public CubeBlock? BlockAt(ShapeCast3D cast, int index = 0)
    {
        if (cast == null) return null;
        return BlockAt(GlobalToGridCoordinates(cast.GetCollisionPoint(index) - cast.GetCollisionNormal(index)));
    }

	public CubeBlock[] GetCubeBlocks()
	{
		return CubeBlocks.Values.ToArray();
	}

    public void RemoveBlock(RayCast3D ray, bool ignoreMirror = false)
	{
		RemoveBlock(BlockAt(ray), ignoreMirror);
	}

	public void RemoveBlock(Vector3 globalPosition, bool ignoreMirror = false)
	{
		RemoveBlock(GlobalToGridCoordinates(globalPosition), ignoreMirror);
	}

	public void RemoveBlock(Vector3I targetPosition, bool ignoreMirror = false)
	{
		RemoveBlock(BlockAt(targetPosition), ignoreMirror);
    }

	public void RemoveBlock(CubeBlock? block, bool ignoreMirror = false)
	{
		if (block == null) return;

        Vector3I blockPosition = LocalToGridCoordinates(block.Position);

        FullRemoveBlock(blockPosition);

        if (block is CockpitBlock c)
            Cockpits.Remove(c);

        if (ignoreMirror)
            return;

        // Remove mirrored blocks. Is recursive, but hopefully the isNull check stops it.
        if (MirrorEnabled)
        {
            Vector3I diff = MirrorPosition - blockPosition;
            if (GridMirrors[0])
                RemoveBlock(new Vector3I(diff.X, blockPosition.Y, blockPosition.Z));
            if (GridMirrors[1])
                RemoveBlock(new Vector3I(blockPosition.X, diff.Y, blockPosition.Z));
            if (GridMirrors[2])
                RemoveBlock(new Vector3I(blockPosition.X, blockPosition.Y, diff.Z));
        }
    }

	/// <summary>
	/// Safe-removes block and closes it.
	/// </summary>
	/// <param name="position"></param>
	private void FullRemoveBlock(Vector3I position)
	{
		if (CubeBlocks.ContainsKey(position))
		{
			CubeBlock blockToRemove = CubeBlocks[position];

			Vector3I[] occupied = blockToRemove.OccupiedSlots(position);
            OccupiedBlocks.RemoveAll(pos => occupied.Contains(pos));

            // Remove from collision
            RemoveShapeOwner(blockToRemove.collisionId);
            RemoveChild(blockToRemove);

            OwnMass -= blockToRemove.Mass;
            RecalcSize();
            RecalcMass();

            blockToRemove.Close();

            CubeBlocks.Remove(position);

            if (IsEmpty())
                Close();
        }
    }

	public CubeBlock? BlockAt(Vector3I position)
	{
		// Cheap check first
		if (CubeBlocks.ContainsKey(position))
            return CubeBlocks[position];

		foreach (var block in CubeBlocks)
			if (block.Value.OccupiedSlots(block.Key).Contains(position))
                return block.Value;
        return null;
	}

    public bool TryGetBlockAt(Vector3I position, out CubeBlock block)
    {
        if (CubeBlocks.TryGetValue(position, out block)) // TODO: populate CubeBlocks with all member blocks
            return true;

        foreach (var cube in CubeBlocks)
        {
            if (cube.Value.OccupiedSlots(cube.Key).Contains(position))
            {
                block = cube.Value;
                return true;
            }
        }
            
        return false;
    }

	public bool IsBlockAt(Vector3I position)
	{
		if (CubeBlocks.ContainsKey(position))
			return true;

		return OccupiedBlocks.Contains(position);
    }

	private void RecalcSize()
	{
		//Vector3I max = CubeBlocks.Keys.ToList()[0];
		//Vector3I min = CubeBlocks.Keys.ToList()[0];
		//
		//foreach (var pos in CubeBlocks.Keys)
		//{
		//	Aabb block = CubeBlocks[pos].Size(pos);
		//	
		//	if (block.End.X > max.X)
		//		max.X = (int) block.End.X;
		//	if (block.End.Y > max.Y)
		//		max.Y = (int) block.End.Y;
		//	if (block.End.Z > max.Z)
		//		max.Z = (int) block.End.Z;
		//
		//	if (block.Position.X < min.X)
		//		min.X = (int) block.Position.X;
		//	if (block.Position.Y < min.Y)
		//		min.Y = (int) block.Position.Y;
		//	if (block.Position.Z < min.Z)
		//		min.Z = (int) block.Position.Z;
		//	
		//}
		//
		//Size = new Aabb(min, max);
	}

	/// <summary>
	/// Custom method to set center of mass.
	/// </summary>
	private void RecalcMass()
	{
		if (OwnMass <= 0)
			return;

		// Add mass of subgrids to own mass
		Mass = OwnMass;
		subGrids.ForEach(s => Mass += s.Mass);

		Vector3 centerOfMass = Vector3.Zero;
		foreach (var block in CubeBlocks.Values)
			centerOfMass += block.Position * block.Mass;
		subGrids.ForEach(s => centerOfMass += s.Position * s.Mass);
		centerOfMass /= Mass;

		if (CenterOfMassMode != CenterOfMassModeEnum.Custom)
			CenterOfMassMode = CenterOfMassModeEnum.Custom;
		CenterOfMass = centerOfMass;

		ThrustControl.SetCenterOfMass(CenterOfMass);
	}

	public Vector3 RoundGlobalCoord(Vector3 global)
	{
		if (IsInsideTree())
			return ToGlobal((Vector3) GlobalToGridCoordinates(global) * 2.5f);
		else
			return global;
	}

	public Vector3 PlaceProjectionGlobal(Vector3 from, Vector3 to)
	{
		Vector3 lTo = RoundGlobalCoord(to);
		lTo = RoundGlobalCoord(lTo.MoveToward(from, 1.25f));
		return lTo;
	}

	public Vector3 PlaceProjectionGlobal(RayCast3D ray, Vector3 blockSize)
	{
		return RoundGlobalCoord(ray.GetCollisionPoint() + ray.GetCollisionNormal() * blockSize/2f);
	}

	public Vector3I GlobalToGridCoordinates(Vector3 global)
	{
		if (!IsInsideTree())
			return Vector3I.Zero;
			
		return LocalToGridCoordinates(ToLocal(global));
	}

	public Vector3I LocalToGridCoordinates(Vector3 local)
	{
		return (Vector3I) (local/2.5f).Round();
	}

	public Vector3 GridToLocalPosition(Vector3I gridCoords)
	{
		return ((Vector3) gridCoords) * 2.5f;
	}

	public Vector3 GridToGlobalPosition(Vector3I gridCoords)
	{
		return ToGlobal(GridToLocalPosition(gridCoords));
	}

	#endregion

	public Godot.Collections.Dictionary<string, Variant> Save()
	{
		Godot.Collections.Array blocks = new();

		foreach (var block in CubeBlocks.Values)
		{
			blocks.Add(block.Save());
		}

		Godot.Collections.Dictionary<string, Variant> saveData = new()
		{
			{ "Name", Name },
			{ "Position", JsonHelper.StoreVec(Position) },
			{ "Rotation", JsonHelper.StoreVec(Rotation) },
			{ "LinearVelocity", JsonHelper.StoreVec(LinearVelocity) },
			{ "AngularVelocity", JsonHelper.StoreVec(AngularVelocity) },
			{ "Blocks", blocks },
		};

		return saveData;
	}

	public virtual void Close()
	{
		// Safe-remove all blocks.
		foreach (var block in CubeBlocks.Values)
		{
			ShapeOwnerClearShapes(block.collisionId);
			RemoveShapeOwner(block.collisionId);
			block.Close();
		}
		GameScene? scene = GameScene.GetGameScene(this);

		// Move children to main scene, just in case PlaceBox or subgrids got caught up.
		// Removes joints so that subgrids detach properly.
		foreach (var child in GetChildren())
		{
			if (child is Joint3D)
				foreach (var subChild in child.GetChildren())
                    subChild.Reparent(scene);
			else
                child.Reparent(scene);
        }

		GameScene.GetGameScene(this)?.grids.Remove(this);
        ParentGrid?.subGrids.Remove(this);

        QueueFree();
	}

	public bool IsEmpty()
	{
		return CubeBlocks.Count == 0;
	}
}
