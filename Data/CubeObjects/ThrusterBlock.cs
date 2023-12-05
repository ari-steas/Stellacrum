using Godot;
using Stellacrum.Data.CubeObjects;
using System;
using System.Collections.Generic;

namespace Stellacrum.Data.CubeObjects
{
	public partial class ThrusterBlock : PowerConsumer
	{
		CubeGrid parent;
		public float ThrustPercent = 0;
		public float MaximumThrust = 0;
		public bool Dampen = true;
		//public Vector3 ThrustForwardVector { get; private set; } = Vector3.Zero;
		private Node3D thrustNode;
		private GpuParticles3D particles;
		private StandardMaterial3D coneMaterial;

		public ThrusterBlock(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
		{
			ReadFromData(blockData, "ThrusterStrength", ref MaximumThrust, verbose);

			// ThrustNode shows where particles should be emitted
			foreach (var node in meshes)
			{
				if (node.Name.ToString().StartsWith("ThrustNode"))
				{
					thrustNode = node;
					break;
				}
			}
			if (thrustNode == null)
				throw new($"{subTypeId} missing ThrustNode!");

			foreach (var node in meshes)
				if (node is MeshInstance3D meshInstance)
					for (int i = 0; i < meshInstance.GetSurfaceOverrideMaterialCount(); i++)
						if (meshInstance.GetActiveMaterial(i).ResourceName == "ThrustConeMaterial")
						{
							//StandardMaterial3D m = (StandardMaterial3D) meshInstance.GetActiveMaterial(i);
							coneMaterial = (StandardMaterial3D) meshInstance.GetActiveMaterial(i);
                            //meshInstance.Mesh.SurfaceSetMaterial(i, coneMaterial);
						}

			Random r = new Random();
            coneMaterial.Emission = new Color(r.NextSingle(), r.NextSingle(), r.NextSingle());
			coneMaterial.EmissionEnabled = true;
        }

		public void SetThrustPercent(float pct)
		{
			ThrustPercent = Mathf.Clamp(pct, 0, 1);
		}

		public override void _Ready()
		{
			base._Ready();
			parent = GetParent<CubeGrid>();
			particles = new()
			{
				Position = thrustNode.Position,
				Rotation = thrustNode.Rotation,
				ProcessMaterial = GD.Load<ParticleProcessMaterial>("res://Assets/Images/Particles/ThrusterProcessMaterial.tres"),
				DrawPass1 = GD.Load<Mesh>("res://Assets/Images/Particles/ThrusterDrawPass.tres"),
				Amount = 64,
				TransformAlign = GpuParticles3D.TransformAlignEnum.ZBillboardYToVelocity,
				Emitting = false
			};
			AddChild(particles);
		}

		VectorPID pid;
		Vector3 angularDesired = Vector3.Zero;
		Vector3 linearDesired = Vector3.Zero;

        public float CurrentRate => throw new NotImplementedException();

        public float MaxRate => throw new NotImplementedException();

        public float MinRate => throw new NotImplementedException();

        public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);

			if (Enabled)
				MaxInput = DefMaxInput * ThrustPercent;

			if (!Enabled || !HasPower)
			{
				particles.Emitting = false;
				return;
			}

			if (Dampen)
			{
				//SetThrustPercent(AngularControl());
				//SetThrustPercent(LinearControl());
				SetThrustPercent(AngularControl() + LinearControl());
			}

			particles.Emitting = ThrustPercent > 0;
			if (!((int)(64 * ThrustPercent) > 0.01))
				particles.Emitting = false;

			//coneMaterial.Emission = new Color(0.1f + 0.9f*ThrustPercent, 0.1f, 0.1f);
			//GD.Print(coneMaterial.Emission);

            // Make particles inherit velocity of parent
            particles.ProcessMaterial.Set("initial_velocity_min", parent.Speed + 30);
			particles.ProcessMaterial.Set("initial_velocity_max", parent.Speed + 40);

            parent.ApplyForce(GlobalTransform.Basis * Vector3.Forward * ThrustPercent * MaximumThrust * (float)delta, GlobalPosition - parent.GlobalPosition);
        }

        float AngularControl()
		{
			//float pct = parent.MovementInput.Dot(Transform.Basis * Vector3.Forward);
			//Vector3 pidOut = pid.Update(parent.AngularVelocity, desired, (float) delta);
			Vector3 angularDiff = -parent.AngularVelocity - angularDesired;

			float rotationThrottle = angularDiff.Dot(parent.Basis * GetAngularAccel());

			return rotationThrottle;
		}

		float LinearControl()
		{
			Vector3 linearDiff = linearDesired - parent.LinearVelocity;

			float linearThrottle = linearDiff.Dot(parent.Basis * ThrustForwardVector());

			return linearThrottle;
		}

		/// <summary>
		/// Sets local desired angular velocity for PID control.
		/// </summary>
		public void SetDesiredAngularVelocity(Vector3 vel)
		{
			angularDesired = vel;
		}

		public void SetDesiredLinearVelocity(Vector3 vel)
		{
			linearDesired = vel;
		}

		public Vector3 ThrustForwardVector()
		{
			return Basis * Vector3.Forward;
		}

		public Vector3 GetForce()
		{
			return ThrustForwardVector() * MaximumThrust;
		}

		public Vector3 GetTorque()
		{
			return (Position - parent.CenterOfMass).Cross(GetForce());
		}

		public Vector3 GetAngularAccel()
		{
			return GetTorque() / PhysicsServer3D.BodyGetDirectState(parent.GetRid()).InverseInertia.Inverse();
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			particles.QueueFree();
		}
	}
}