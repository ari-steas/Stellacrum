using System;
using Godot;
using System.Collections.Generic;
using Stellacrum.Data.CubeGridHelpers;
using Stellacrum.Data.CubeObjects;
using Stellacrum.Data.ObjectLoaders;

public partial class CubeGrid : RigidBody3D
{
    //public const float MinGridSize = 2.5f;
    public const float MinGridSize = 0.625f;

	public CubeGrid ParentGrid;

	public Aabb Size { get; private set; } = new Aabb();
	public float Speed { get; private set; } = 0;
	public readonly List<CockpitBlock> Cockpits = new();

	public GridThrustControl ThrustControl { get; private set; }

    protected GridOctree GridTree = new(Vector3.Zero, MinGridSize, null);
    protected HashSet<CubeBlock> CubeBlocks = new HashSet<CubeBlock>();

    public Action<CubeBlock> OnBlockAdded;
    public Action<CubeBlock> OnBlockRemoved;
    public int BlockCount => CubeBlocks.Count;

	readonly internal List<CubeGrid> subGrids = new();

	private float OwnMass = 0;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        ThrustControl = new GridThrustControl(this);

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
		ThrustControl.Update(LinearVelocity, AngularVelocity, delta);
		Speed = LinearVelocity.Length();

		// octree debug display

        //Stack<GridOctree> trees = new Stack<GridOctree>();
		//trees.Push(GridTree);
        //while (trees.TryPop(out var tree))
        //{
		//	DebugDraw.Shape(GlobalPosition + Quaternion * (tree.RootPosition + Vector3.One * tree.CellWidth), Quaternion, new BoxShape3D
        //    {
		//		Size = Vector3.One * tree.CellWidth * 2,
        //    }, duration: 0);
		//
        //    if (tree.IsLeaf)
        //        continue;
		//
        //    foreach (var subtree in tree.Subtrees)
        //    {
        //        if (subtree != null)
        //            trees.Push(subtree);
        //    }
        //}
    }

	public override void _PhysicsProcess(double delta)
	{
		// DebugDraw in PhysicsProcess to update 60fps
		DebugDraw.Point(ToGlobal(CenterOfMass), 1, Colors.Yellow);
		DebugDraw.Text3D(Name + ": " + Mass, ToGlobal(CenterOfMass));
	}

	public Aabb BoundingBox()
	{
		return new Aabb(Position + Size.Position*2.5f - Vector3.One*1.25f, Size.End*2.5f);
	}

	#region blocks


	public readonly bool[] GridMirrors = new[] {false, false, false};
	public bool MirrorEnabled = false;
	public Vector3I MirrorPosition = Vector3I.Zero;

	public void AddBlock(RayCast3D ray, Basis rotation, string blockId)
	{
		AddBlock(GetPlaceProjectionGlobal(ray, CubeBlockLoader.ExistingBaseFromId(blockId).size), rotation, blockId);
	}

	public void AddBlock(Vector3 globalPosition, Basis rotation, string blockId)
	{
        if (CubeBlocks.Count == 0)
        {
			AddBlockLocal(Vector3.Zero, rotation, blockId);
            return;
        }

		AddBlockLocal(ToLocal(globalPosition), rotation, blockId);
	}

	public void AddBlockLocal(Vector3 localPos, Basis rotation, string blocKid)
	{
        AddBlockLocal(localPos, rotation, CubeBlockLoader.BlockFromId(blocKid));
	}

	public void AddBlockLocal(Vector3 localPos, Basis rotation, CubeBlock block)
    {
        localPos -= block.size / 2; // translate to lower left corner
		
        HashSet<CubeBlock> intersects = new HashSet<CubeBlock>(); // TODO move this to a shared pool
        if (GridTree.GetBlocksInVolume(localPos, block.size, ref intersects) || !CubeBlocks.Add(block))
        {
			foreach (var b in intersects)
			    DebugDraw.Box(b.GlobalPosition, GlobalTransform.Basis.GetRotationQuaternion(), b.size, new Color(1,0,0), duration: 2);

            DebugDraw.Box(ToGlobal(localPos + block.size / 2), GlobalTransform.Basis.GetRotationQuaternion(), block.size, new Color(1,1,0), duration: 2);
            return;
        }

		GridOctree.ExpandTree(ref GridTree, localPos, block.size);

		// Try adding first; if this fails, don't parent the block.
        if (!GridTree.SetBlockAt(localPos, CubeBlocks.Count > 1 ? Basis.Inverse() * rotation : Basis.Identity, block))
        {
            CubeBlocks.Remove(block);
            GD.PrintErr($"Failed to set block at {localPos}! Internal fail.");
            return;
        }

        localPos += block.size / 2; // translate back to block center
        AddChild(block);

        block.Position = localPos;
		// If this is called when there are zero blocks (i.e. this is first block on grid), Global values throw an error (as they don't exist yet)
		if (CubeBlocks.Count > 1)
			block.GlobalRotation = rotation.GetEuler(); // ik this sucks but it's good enough

        block.collisionId = CreateShapeOwner(this);
        ShapeOwnerAddShape(block.collisionId, block.collision);
        ShapeOwnerSetTransform(block.collisionId, block.Transform);

        Mass += block.Mass;
        OwnMass += block.Mass;

        RecalcSize();
        RecalcMass();

        OnBlockAdded?.Invoke(block);

        if (block is CockpitBlock c)
            Cockpits.Add(c);

        //try
        //{
		//	// TODO reintroduce
        //     Place mirrored blocks
        //    if (MirrorEnabled)
        //    {
        //        Vector3I diff = MirrorPosition - localPos;
		//	
        //        // Flip along Y axis
        //        block.RotationDegrees += block.Basis * new Vector3(180, 0, 0);
        //        if (GridMirrors[1])
        //            AddBlock(new(position_GridLocal.X, diff.Y, position_GridLocal.Z), block.GlobalRotation, block);
        //        block.GlobalRotation = rotation;
		//	
        //        // Flip along X axis
        //        block.GlobalRotate(Basis * Vector3.Forward, Mathf.Pi);
        //        block.GlobalRotate(Basis * Vector3.Right, Mathf.Pi);
        //        if (GridMirrors[0])
        //            AddBlock(new(diff.X, position_GridLocal.Y, position_GridLocal.Z), block.GlobalRotation, block);
        //        block.GlobalRotation = rotation;
		//	
        //        // Flip along Z axis
        //        block.RotationDegrees += block.Basis * new Vector3(180, 0, 0);
        //        if (GridMirrors[2])
        //            AddBlock(new(position_GridLocal.X, position_GridLocal.Y, diff.Z), block.GlobalRotation, block);
        //        block.GlobalRotation = rotation;
        //    }
        //}	
        //catch
        //{
		//
        //}
    }

    /// <summary>
    /// Adds CubeBlock to grid. This method is to be used with fully constructed blocks ONLY, such as those loaded from save files.
    /// </summary>
    /// <param name="block"></param>
    public void AddFullBlock(CubeBlock block)
    {
		// Override existing block if exists
		FullRemoveBlock(block);

        var localPos = block.Position - block.size / 2; // translate to lower left corner

		GridOctree.ExpandTree(ref GridTree, localPos, block.size);

        if (!GridTree.SetBlockAt(localPos, block.Transform.Basis, block))
            return;

        AddChild(block);
        CubeBlocks.Add(block);

        // Add to collision hull
        block.collisionId = CreateShapeOwner(this);
        ShapeOwnerAddShape(block.collisionId, block.collision);
        ShapeOwnerSetTransform(block.collisionId, block.Transform);

        OwnMass += block.Mass;

        RecalcSize();
        RecalcMass();

        OnBlockAdded?.Invoke(block);

        if (block is CockpitBlock c)
            Cockpits.Add(c);
    }

	#nullable enable
	public bool TryGetBlockAt(RayCast3D ray, out CubeBlock block)
	{
		return TryGetBlockAtGlobal(ray.GetCollisionPoint() - ray.GetCollisionNormal() * MinGridSize/2, out block);
    }

    public bool TryGetBlockAt(ShapeCast3D cast, out CubeBlock block, int index = 0)
    {
        return TryGetBlockAtGlobal(cast.GetCollisionPoint(index) - cast.GetCollisionNormal(index) * MinGridSize/2, out block);
    }

	public IEnumerable<CubeBlock> GetCubeBlocks()
    {
        return CubeBlocks;
    }

    public void RemoveBlock(RayCast3D ray, bool ignoreMirror = false)
	{
		if (TryGetBlockAt(ray, out CubeBlock block))
		    RemoveBlock(block, ignoreMirror);
	}

	public void RemoveBlock(CubeBlock? block, bool ignoreMirror = false)
	{
		if (block == null) return;
        FullRemoveBlock(block);

        if (ignoreMirror)
            return;

		// TODO please return this
        //// Remove mirrored blocks. Is recursive, but hopefully the isNull check stops it.
        //if (MirrorEnabled)
        //{
        //    Vector3I diff = MirrorPosition - blockPosition;
        //    if (GridMirrors[0])
        //        RemoveBlock(new Vector3I(diff.X, blockPosition.Y, blockPosition.Z));
        //    if (GridMirrors[1])
        //        RemoveBlock(new Vector3I(blockPosition.X, diff.Y, blockPosition.Z));
        //    if (GridMirrors[2])
        //        RemoveBlock(new Vector3I(blockPosition.X, blockPosition.Y, diff.Z));
        //}
    }

	/// <summary>
	/// Safe-removes block and closes it.
	/// </summary>
	/// <param name="position"></param>
	private void FullRemoveBlock(Vector3 position)
    {
        if (!GridTree.TryGetBlockAt(position, out CubeBlock toRemove))
            return;
		FullRemoveBlock(toRemove);
    }

    /// <summary>
    /// Safe-removes block and closes it.
    /// </summary>
    /// <param name="position"></param>
    private void FullRemoveBlock(CubeBlock toRemove)
    {
        if (!CubeBlocks.Remove(toRemove))
            return;

        GridTree.RemoveBlock(toRemove);

        OnBlockRemoved?.Invoke(toRemove);

        if (toRemove is CockpitBlock c)
            Cockpits.Remove(c);

        // Remove from collision
        RemoveShapeOwner(toRemove.collisionId);
        RemoveChild(toRemove);

        OwnMass -= toRemove.Mass;
        RecalcSize();
        RecalcMass();

        toRemove.Close();

        if (IsEmpty())
            Close();
    }

    public bool TryGetBlockAtGlobal(Vector3 position, out CubeBlock block)
    {
        return GridTree.TryGetBlockAt(ToLocal(position), out block);
    }

    public bool TryGetBlockAtLocal(Vector3 position, out CubeBlock block)
    {
        return GridTree.TryGetBlockAt(position, out block);
    }

	public bool IsBlockAtGlobal(Vector3 position)
	{
		return GridTree.TryGetBlockAt(ToLocal(position), out _);
    }

    public bool IsBlockAtLocal(Vector3 position)
    {
        return GridTree.TryGetBlockAt(position, out _);
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
		foreach (var block in CubeBlocks)
			centerOfMass += block.Position * block.Mass;
		subGrids.ForEach(s => centerOfMass += s.Position * s.Mass);
		centerOfMass /= Mass;

		if (CenterOfMassMode != CenterOfMassModeEnum.Custom)
			CenterOfMassMode = CenterOfMassModeEnum.Custom;
		CenterOfMass = centerOfMass;
	}
	
	public Vector3 GetPlaceProjectionGlobal(RayCast3D ray, Vector3 blockSize)
    {
		Vector3 vec = ToLocal(ray.GetCollisionPoint() + ray.GetCollisionNormal() * blockSize/2f) - blockSize / 2;

		vec /= MinGridSize;
        vec = new Vector3((float)Math.Round(vec.X), (float)Math.Round(vec.Y), (float)Math.Round(vec.Z));
        vec *= MinGridSize;
        vec += blockSize / 2;

        return ToGlobal(vec);
    }

	#endregion

	public Godot.Collections.Dictionary<string, Variant> Save()
	{
		Godot.Collections.Array blocks = new();

		foreach (var block in CubeBlocks)
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
		foreach (var block in CubeBlocks)
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
