using Godot;
using System;

public partial class player_character : CharacterBody3D
{
	GameScene scene;


	#region movement

	public const float Speed = 60.0f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float lastX = 0, lastY = 0, lastZ = 0;
	private bool _dampenersEnabled = true;

	#endregion


	private long nextPlaceTime;	

	private SpotLight3D light;
	private Camera3D camera;
	private hud_scene HUD;
	private long DelayedEnableCollision = 0;
	private Node3D shipCrosshair;
	private CollisionShape3D collision;

	public PlaceBox PlayerPlaceBox { get; private set; }
	public Vector3 lookPosition;

	public RayCast3D interactCast;

	public override void _Ready()
	{
		scene = GetParent<GameScene>();

		light = GetNode<SpotLight3D>("SpotLight3D");
		camera = GetNode<Camera3D>("PlayerCamera");
		interactCast = GetNode<RayCast3D>("InteractCast");
		interactCast.TargetPosition = new (0, 0, -10);

		PlayerPlaceBox = GetNode<PlaceBox>("PlaceBox");
		HUD = GetNode<hud_scene>("HudScene");
		shipCrosshair = GetNode<Node3D>("ShipCrosshair3D");
		collision = FindChild("PlayerCollision") as CollisionShape3D;

		// Wait 1 second after scene start to place blocks
		nextPlaceTime = DateTime.Now.Ticks + 10_000_000;
	}

	private bool justLookedAtGrid = false;
	public override void _Process(double delta)
	{
		if (!scene.isActive)
			return;

		InputHandler();
		
		if (interactCast.IsColliding())
		{
			if (interactCast.GetCollider() is CubeGrid grid)
			{
				if (!justLookedAtGrid)
				{
					justLookedAtGrid = true;

					// Snap PlayerPlaceBox to looked-at grid
					RemoveChild(PlayerPlaceBox);
					grid.AddChild(PlayerPlaceBox);
				
					// Clamp to 90 degree rotations
					PlayerPlaceBox.SnapLocal();
				}

				lookPosition = grid.PlaceProjectionGlobal(interactCast);
			}
		}
		else
		{
			lookPosition = ToGlobal(interactCast.TargetPosition);

			if (justLookedAtGrid)
			{
				Vector3 rot = PlayerPlaceBox.GlobalRotation;
				PlayerPlaceBox.GetParent().RemoveChild(PlayerPlaceBox);
				AddChild(PlayerPlaceBox);
				if (PlayerPlaceBox.IsInsideTree())
					PlayerPlaceBox.GlobalRotation = rot;

				justLookedAtGrid = false;
			}
		}

		if (PlayerPlaceBox.IsInsideTree())
			PlayerPlaceBox.GlobalPosition = lookPosition;

		if (DelayedEnableCollision > 0 && (DateTime.Now.Ticks - DelayedEnableCollision > 0))
		{
			DelayedEnableCollision = 0;
			collision.Disabled = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!scene.isActive)
			return;

		HandleMovement(delta);
		HandleRotation(delta);
		lastX = 0;
		lastY = 0;
		lastZ = 0;
	}

	public bool IsInCockpit = false;
	public CubeGrid currentGrid;

	private Vector3 relativePosition = Vector3.Zero;
	private CockpitBlock currentCockpit;
	private void TryEnter(CubeGrid grid)
	{
		if (grid.Cockpits.Count > 0)
		{
			CockpitBlock closest = grid.Cockpits[0];
			float closestD = closest.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			foreach (var cockpit in grid.Cockpits)
			{
				float d = cockpit.GlobalPosition.DistanceSquaredTo(GlobalPosition);
				if (d < closestD)
				{
					closestD = d;
					closest = cockpit;
				}
			}

			relativePosition = GlobalPosition - grid.GlobalPosition;

			Position = Vector3.Zero;
			Rotation = Vector3.Zero;

			collision.Disabled = true;
			GetParent().RemoveChild(this);
			closest.AddChild(this);

			shipCrosshair.Visible = true;
			shipCrosshair.Rotation = Vector3.Zero;

			IsInCockpit = true;
			currentGrid = grid;
			currentCockpit = closest;
		}
	}

	private void TryExit()
	{
		if (IsInCockpit)
		{
			Velocity = currentGrid.LinearVelocity;
			currentGrid.DesiredRotation = Vector3.Zero;
			GetParent().RemoveChild(this);
			scene.AddChild(this);
			// Add 0.1s to re-enable collision, because otherwise the grid gets flung
			DelayedEnableCollision = DateTime.Now.Ticks + 1_000_000;

			GlobalPosition = currentGrid.ToGlobal(relativePosition);

			shipCrosshair.Visible = false;

			IsInCockpit = false;
			currentGrid = null;
			relativePosition = Vector3.Zero;
		}
	}

	private void InputHandler()
	{
		if (Input.IsActionJustPressed("Interact"))
		{
			if (!IsInCockpit && interactCast.GetCollider() is CubeGrid grid)
				TryEnter(grid);
			else if (IsInCockpit)
				TryExit();
		}

		if (IsInCockpit)
			return;            

		// Prevents placing 60 blocks per second. Rate-limited to 6 per second.
		if (PlayerPlaceBox.IsHoldingBlock && nextPlaceTime < DateTime.Now.Ticks)
		{
			// TODO: variable distance
			if (Input.IsActionJustPressed("MousePressL"))
			{
				scene.TryPlaceBlock(PlayerPlaceBox.CurrentBlockId, interactCast, PlayerPlaceBox.GlobalRotation);
				nextPlaceTime = DateTime.Now.Ticks + 1_000_000;
			}
			
			if (Input.IsActionJustPressed("MousePressR"))
			{
				scene.RemoveBlock(interactCast);
				nextPlaceTime = DateTime.Now.Ticks + 1_000_000;
			}
		}

		// Surely there's a better way to do this, but would be a waste of dev time.
		if (Input.IsActionJustPressed("Toolbar0"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[0]);
		if (Input.IsActionJustPressed("Toolbar1"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[1]);
		if (Input.IsActionJustPressed("Toolbar2"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[2]);
		if (Input.IsActionJustPressed("Toolbar3"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[3]);
		if (Input.IsActionJustPressed("Toolbar4"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[4]);
		if (Input.IsActionJustPressed("Toolbar5"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[5]);
		if (Input.IsActionJustPressed("Toolbar6"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[6]);
		if (Input.IsActionJustPressed("Toolbar7"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[7]);
		if (Input.IsActionJustPressed("Toolbar8"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[8]);
		if (Input.IsActionJustPressed("Toolbar9"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[9]);

		// Why IsActionJustReleased? God knows. I sure don't.
		if (Input.IsActionJustReleased("MoveBlockOut"))
			interactCast.TargetPosition = new(0, 0, MoveTowardsFloat(interactCast.TargetPosition.Z, -20, 0.5f));
		if (Input.IsActionJustReleased("MoveBlockIn"))
			interactCast.TargetPosition = new(0, 0, MoveTowardsFloat(interactCast.TargetPosition.Z, -2, 0.5f));

		// Block rotation
		if (PlayerPlaceBox.IsHoldingBlock)
		{
			// Rotate 90 degrees when looking at grid
			if (interactCast.GetCollider() is CubeGrid)
			{
				if (Input.IsActionJustPressed("BlockRotateX+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, nd);
				if (Input.IsActionJustPressed("BlockRotateX-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, -nd);
				if (Input.IsActionJustPressed("BlockRotateY+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, nd);
				if (Input.IsActionJustPressed("BlockRotateY-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, -nd);
				if (Input.IsActionJustPressed("BlockRotateZ+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, nd);
				if (Input.IsActionJustPressed("BlockRotateZ-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, -nd);
			}
			else // Else rotate 90 degrees per second
			{
				if (Input.IsActionPressed("BlockRotateX+"))
					PlayerPlaceBox.Rotate(Vector3.Right, ohd);
				if (Input.IsActionPressed("BlockRotateX-"))
					PlayerPlaceBox.Rotate(Vector3.Right, -ohd);
				if (Input.IsActionPressed("BlockRotateY+"))
					PlayerPlaceBox.Rotate(Vector3.Up, ohd);
				if (Input.IsActionPressed("BlockRotateY-"))
					PlayerPlaceBox.Rotate(Vector3.Up, -ohd);
				if (Input.IsActionPressed("BlockRotateZ+"))
					PlayerPlaceBox.Rotate(Vector3.Forward, ohd);
				if (Input.IsActionPressed("BlockRotateZ-"))
					PlayerPlaceBox.Rotate(Vector3.Forward, -ohd);
			}
		}
	}

	// 1.5 degrees, in radians
	private const float ohd = Mathf.Pi/120;
	// 90 degrees in radians
	private const float nd = Mathf.Pi/2;

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion motion)
		{
			lastX = -motion.Relative.X;
			lastY = -motion.Relative.Y;
			return;
		}
	}


	private void _ToggleDampeners(bool enabled)
	{
		_dampenersEnabled = enabled;
	}

	private void _ToggleLight(bool enabled)
	{
		light.Visible = enabled;
	}

	private void _ToggleThirdPerson(bool enabled)
	{
		if (enabled)
			camera.Position = new Vector3(0, 1.5f, 10f);
		else
			camera.Position = new Vector3(0, 0.5f, 0);
	}

	#region movement

	private Vector3 prevCrosshair = Vector3.Zero;
	private void HandleRotation(double delta)
	{
		// Q and E keys
		if (Input.IsActionPressed("RotateClockwise"))
			lastZ = 1;
		if (Input.IsActionPressed("RotateAntiClockwise"))
			lastZ = -1;

		if (IsInCockpit)
		{
			shipCrosshair.GlobalRotation = prevCrosshair;
			
			Vector3 crosshairForward = shipCrosshair.Basis * Vector3.Forward;
			

			Vector3 rotatedPos = crosshairForward.Rotated(Vector3.Up, (float)(lastX*delta));
			rotatedPos = rotatedPos.Rotated(Vector3.Right, (float)(lastY*delta));

			float scalar = Vector3.Forward.Dot(rotatedPos);

			DebugDraw.Text(scalar);

			if (Vector3.Forward.Dot(rotatedPos) > 0.5f)
			{
				shipCrosshair.Rotate(Vector3.Up, (float)(lastX * scalar * delta));
				shipCrosshair.Rotate(Vector3.Right, (float)(lastY * scalar * delta));
				crosshairForward = shipCrosshair.Basis * Vector3.Forward;

				currentGrid.DesiredRotation = currentCockpit.GlobalTransform.Basis * new Vector3(-crosshairForward.Y, crosshairForward.X, lastZ);
			}

			prevCrosshair = shipCrosshair.GlobalRotation;

			// Direct input, kinda sucks
			//currentGrid.DesiredRotation = currentCockpit.GlobalTransform.Basis * new Vector3(-lastY, -lastX, lastZ);
			return;
		}

		RotateObjectLocal(Vector3.Up, (float)(lastX*delta));
		RotateObjectLocal(Vector3.Right, (float)(lastY*delta));
		RotateObjectLocal(Vector3.Forward, (float)(lastZ*delta));
	}

	private void HandleMovement(double delta)
	{
		Vector3 inputDir = MovementVector();

		if (IsInCockpit)
		{
			currentGrid.MovementInput = currentCockpit.Basis * inputDir.Clamp(-Vector3.One, Vector3.One);
			return;
		}

		Vector3 velocity = Velocity;
		
		Vector3 direction = Quaternion * inputDir;
		if (direction != Vector3.Zero)
			velocity += ClampIndividual(direction, 1) * Speed * (float) delta;
		else if (_dampenersEnabled)
			velocity = IndividualDampen(velocity, Vector3.Zero, Speed*(float) delta*1.15f);
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private Vector3 MovementVector()
	{
		Vector2 horizontalInput = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
		float verticalInput = Input.GetAxis("MoveDown", "MoveUp");

		return new (horizontalInput.X, verticalInput, horizontalInput.Y);
	}

	private Vector3 IndividualDampen(Vector3 vel, Vector3 endVel, float delta)
	{
		// Simulates directional jetpack thrusters, rather than a gimballed system
		vel = Quaternion.Inverse() * vel;
		endVel = Quaternion.Inverse() * endVel;

		return Quaternion * new Vector3(
			MoveTowardsFloat(vel.X, endVel.X, delta),
			MoveTowardsFloat(vel.Y, endVel.Y, delta),
			MoveTowardsFloat(vel.Z, endVel.Z, delta)
		);
	}

	private float MoveTowardsFloat(float value, float endValue, float delta)
	{
		float evDelta = Math.Abs(value - endValue);

		if (evDelta > delta)
		{
			if (value > endValue)
				return value - delta;
			if (value < endValue)
				return value + delta;
		}

		return endValue;
	}

	private Vector3 ClampIndividual(Vector3 vec, float max)
	{
		if (vec.X > max)
			vec.X = max;
		if (vec.Y > max)
			vec.Y = max;
		if (vec.Z > max)
			vec.Z = max;

		return vec;
	}

	#endregion
}
