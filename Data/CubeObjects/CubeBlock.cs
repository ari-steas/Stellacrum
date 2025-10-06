using Godot;
using Stellacrum.Data.CubeGridHelpers;
using System;
using System.Collections.Generic;

namespace Stellacrum.Data.CubeObjects
{
    public partial class CubeBlock : StaticBody3D
	{
		public Shape3D collision;
		public uint collisionId = 0;
		public List<Node3D> meshes;
		public string subTypeId = "";
		public Vector3 size = Vector3.One * 2.5f;
        public float GridSize = 2.5f;
		public int Mass { get; private set; } = 100;
		public int Health { get => _health; set => SetHealth(value); }
		private int _health = 100;

        public List<GridOctree> ContainedOctrees = new List<GridOctree>(); // TODO !!!

		internal void SetHealth(int newHealth)
		{
            _health = newHealth;
			if (_health <= 0)
				Remove();
		}

		public CubeBlock() { }
		protected Dictionary<string, GridMultiBlockStructure> MemberStructures { get; private set; } = new();

		public CubeBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false)
		{
            this.subTypeId = subTypeId;
            List<Node3D> model;
			Vector3 size = Vector3.One * GridSize;
			int mass = 100;

			// Load model from ModelLoader
			string modelId = "ArmorBlock1x1";
			ReadFromData(blockData, "Model", ref modelId, verbose);

			model = ModelLoader.GetModel(modelId);

			// Calc BlockSize
			ReadFromData(blockData, "BlockSize", ref size, verbose);
			size *= GridSize;

			ReadFromData(blockData, "Mass", ref mass, verbose);
			ReadFromData(blockData, "Health", ref _health, verbose);

			this.size = size;

			collision = new BoxShape3D()
			{
				Size = size
			};

			Mass = mass;

			if (model != null)
			{
                meshes = model;
                foreach (Node3D mesh in model)
					AddChild(mesh);
			}

			Name = "CubeBlock." + subTypeId + "." + GetIndex();
        }

        public override void _Ready()
        {
            base._Ready();
			CallDeferred(MethodName.OnPlace);
        }

        /// <summary>
        /// Adds structure reference. DOES NOT ADD TO STRUCTURE ITSELF!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="structure"></param>
        public virtual void AddStructureRef(string type, GridMultiBlockStructure structure)
		{
			if (MemberStructures.ContainsKey(type))
				MemberStructures.Remove(type);
			MemberStructures.Add(type, structure);
		}

		/// <summary>
		/// Removes structure reference. DOES NOT REMOVE FROM STRUCTURE ITSELF
		/// </summary>
		/// <param name="type"></param>
        public virtual void RemoveStructureRef(string type)
        {
            if (MemberStructures.ContainsKey(type))
                MemberStructures.Remove(type);
        }

        public Aabb Size(Vector3I position)
		{
			// Offset position to bottom-left corner
			position -= (Vector3I) (this.size / (GridSize*2));
			Aabb size = new(position, this.size/GridSize);
			return size;
		}

		public BoxShape3D BoxShape3D()
		{
			return new BoxShape3D()
			{ 
				Size = size,
			};
		}

        static PackedScene explodeScene = GD.Load<PackedScene>("res://Data/CubeObjects/WeaponObjects/explosion_particle.tscn");

        /// <summary>
        /// Destroy via grid removal. Safe.
        /// </summary>
        public virtual void Remove()
		{
			//GetParent<CubeGrid>().CallDeferred(CubeGrid.MethodName.RemoveBlock, this, true);
			Node3D particle = (Node3D) explodeScene.Instantiate();
			particle.Position = GlobalPosition;
			GameScene.GetGameScene(this).AddChild(particle);
            Grid().RemoveBlock(this, true);
        }

		/// <summary>
		/// Hard-close. 
		/// </summary>
		public virtual void Close()
		{
			foreach (var structure in MemberStructures.Values)
				//structure.CallDeferred("RemoveStructureBlock", this);
				structure.RemoveStructureBlock(this);
			QueueFree();
		}

		/// <summary>
		/// Returns a Json dictionary containing block data.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Triggers on place, after being added to the scene tree.
		/// </summary>
		public virtual void OnPlace()
		{

		}

		/// <summary>
		/// Reads a variable from Json dictionary blockData. If key missing in blockData, does not change variable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="blockData"></param>
		/// <param name="dataKey"></param>
		/// <param name="variable"></param>
		/// <param name="verbose"></param>
		protected void ReadFromData<[MustBeVariant] T>(Godot.Collections.Dictionary<string, Variant> blockData, string dataKey, ref T variable, bool verbose)
		{
            if (blockData.ContainsKey(dataKey))
			{
				if (typeof(T) == typeof(Vector3))
				{
					variable = (T)Convert.ChangeType(JsonHelper.LoadVec(blockData[dataKey]), typeof(T));
				}
				else
				{
					variable = blockData[dataKey].As<T>();
				}

				if (verbose)
					GD.PrintErr(dataKey + " : " + blockData[dataKey].AsString());
            }
            else if (verbose)
                GD.PrintErr($"Missing CubeBlock.{subTypeId} data [{dataKey}]! Setting to default...");
        }

		/// <summary>
		/// Returns parent grid (or null if somehow missing).
		/// </summary>
		/// <returns></returns>
		public CubeGrid Grid()
		{
			return GetParent<CubeGrid>();
		}

        public override int GetHashCode()
        {
            return GetInstanceId().GetHashCode();
        }
    }
}