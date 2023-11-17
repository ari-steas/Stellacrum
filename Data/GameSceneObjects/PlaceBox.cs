using Godot;
using System;

namespace GameSceneObjects
{
	public partial class PlaceBox : Node3D
	{
		public string CurrentBlockId { get; private set; } = "";
		public bool IsHoldingBlock { get; private set; } = false;

		private MeshInstance3D ProjectMesh;

		public Vector3 CurrentSize = Vector3.One;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Visible = false;
			ProjectMesh = GetChild<MeshInstance3D>(0);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{

		}

		public override void _PhysicsProcess(double delta)
		{
			if (!Visible)
				return;
			DebugDraw.Text3D("Forward", ToGlobal(Vector3.Forward * 1.25f), 0, Colors.Green);
			DebugDraw.Text3D("Right", ToGlobal(Vector3.Right * 1.25f), 0, Colors.Red);
			DebugDraw.Text3D("Up", ToGlobal(Vector3.Up * 1.25f), 0, Colors.Blue);
		}

		public void SetBlock(string subTypeId)
		{
			// Prevents lag on double-tap
			if (subTypeId == CurrentBlockId)
				return;
			CurrentBlockId = subTypeId;

			IsHoldingBlock = false;

			// If not showing block, don't try to show block...
			if (subTypeId == "")
			{
				IsHoldingBlock = false;
				Visible = false;
				return;
			}
			else
			{
				Visible = true;
				IsHoldingBlock = true;
			}

			// Remove existing block
			foreach (var child in GetChildren())
				if (child != ProjectMesh)
					RemoveChild(child);

			// Pull mesh from CubeBlockLoader
			foreach (var mesh in CubeBlockLoader.ExistingBaseFromId(subTypeId).meshes)
				AddChild(mesh.Duplicate());

			// Make mesh semi-transparent
			foreach (var node in GetChildren())
			{
				if (node is MeshInstance3D mesh && node != ProjectMesh)
				{
					mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
					mesh.Transparency = 0.5f;
				}
			}

			// Change ProjectMesh size to match block
			CurrentSize = CubeBlockLoader.ExistingBaseFromId(CurrentBlockId).size;
			ProjectMesh.Scale = CurrentSize / 2.5f;

			GD.Print("Set held block to " + subTypeId);
		}

		public void SnapRotationLocal()
		{
			Rotation = Rotation.Snapped(Vector3.One * nd);

			//Vector3 rotation = Rotation;
			//Vector3 mod = rotation % nd;


			// Attempts to round to closest snap rotation
			//if (mod.X > nd/2)
			//	rotation.X += nd - mod.X;
			//else
			//	rotation.X -= mod.X;
			//
			//if (mod.Y > nd/2)
			//	rotation.Y += nd - mod.Y;
			//else
			//	rotation.Y -= mod.Y;
			//
			//if (mod.Z > nd/2)
			//	rotation.Z += nd - mod.Z;
			//else
			//	rotation.Z -= mod.Z;
			//
			//Rotation = rotation;
		}

		private const float nd = Mathf.Pi / 2;
	}
}